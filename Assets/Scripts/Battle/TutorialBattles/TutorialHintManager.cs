using UnityEngine;
using System.Collections;

public class TutorialHintManager : MonoBehaviour
{
    public static TutorialHintManager Instance { get; private set; }

    [SerializeField] private TutorialPointer pointerPrefab;
    [SerializeField] private float waitForButtonTime = 1f;

    private TutorialPointer activePointer;
    private Coroutine showCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("[TutorialHintManager] Awake completed, Instance set");

        if (pointerPrefab != null)
        {
            activePointer = Instantiate(pointerPrefab, transform);
            Debug.Log("[TutorialHintManager] Pointer instantiated");
        }
        else
        {
            Debug.LogError("[TutorialHintManager] Pointer Prefab is NULL!");
        }
    }

    private void Start()
    {
        Debug.Log($"[TutorialHintManager] Start - Instance is {(Instance == this ? "this" : "other")}");
    }

    public void ShowHint(TutorialHintData hint)
    {
        Debug.Log($"[TutorialHintManager] ShowHint called with hint: {(hint != null ? hint.name : "NULL")}");

        if (hint == null)
        {
            Debug.LogWarning("[TutorialHintManager] Hint is NULL!");
            return;
        }

        if (activePointer == null)
        {
            Debug.LogError("[TutorialHintManager] activePointer is NULL! Check Pointer Prefab.");
            return;
        }

        if (showCoroutine != null)
        {
            Debug.Log("[TutorialHintManager] Stopping previous show coroutine");
            StopCoroutine(showCoroutine);
        }

        showCoroutine = StartCoroutine(ShowHintWithRetry(hint));
    }

    private IEnumerator ShowHintWithRetry(TutorialHintData hint)
    {
        Debug.Log($"[TutorialHintManager] Starting retry coroutine for '{hint.targetAbilityId}'");

        AbilityButton targetButton = null;
        float elapsed = 0f;
        float checkInterval = 0.1f;

        while (targetButton == null && elapsed < waitForButtonTime)
        {
            if (AbilityRegistry.Instance == null)
            {
                Debug.LogWarning("[TutorialHintManager] AbilityRegistry.Instance is NULL!");
            }
            else
            {
                targetButton = AbilityRegistry.Instance.Get(hint.targetAbilityId);
                Debug.Log($"[TutorialHintManager] Checking for '{hint.targetAbilityId}': {(targetButton != null ? "FOUND" : "not found")}");
            }

            if (targetButton == null)
            {
                yield return new WaitForSeconds(checkInterval);
                elapsed += checkInterval;
            }
        }

        if (targetButton != null)
        {
            Debug.Log($"[TutorialHintManager] Button found! Showing pointer for '{hint.targetAbilityId}'");
            activePointer.Show(targetButton.GetComponent<RectTransform>(), hint);
        }
        else
        {
            Debug.LogWarning($"[TutorialHintManager] Button '{hint.targetAbilityId}' not found after {waitForButtonTime}s");
        }

        showCoroutine = null;
    }

    public void HideHint(bool instant = true)
    {
        Debug.Log("[TutorialHintManager] HideHint called");

        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        if (activePointer != null)
            activePointer.Hide(instant);
    }
}