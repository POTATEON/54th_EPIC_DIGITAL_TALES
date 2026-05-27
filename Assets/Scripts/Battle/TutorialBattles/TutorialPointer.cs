using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialPointer : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform arrowRect;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private RectTransform messageBackground;
    [SerializeField] private float bounceHeight = 10f;
    [SerializeField] private float bounceSpeed = 2f;

    private Vector3 arrowStartPos;

    private void Awake()
    {
        Debug.Log($"[TutorialPointer] Awake on {gameObject.name}");

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            Debug.Log($"[TutorialPointer] canvasGroup auto-assigned: {canvasGroup != null}");
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);

        if (arrowRect != null)
            arrowStartPos = arrowRect.localPosition;

        Debug.Log($"[TutorialPointer] Awake complete. canvasGroup={canvasGroup}, arrowRect={arrowRect}, messageText={messageText}");
    }

    public void Show(RectTransform targetButton, TutorialHintData hint)
    {
        Debug.Log($"[TutorialPointer] ========================================");
        Debug.Log($"[TutorialPointer] SHOW CALLED");
        Debug.Log($"[TutorialPointer] targetButton: {(targetButton != null ? targetButton.name : "NULL")}");
        Debug.Log($"[TutorialPointer] targetButton position (world): {targetButton?.position}");
        Debug.Log($"[TutorialPointer] targetButton rect: {targetButton?.rect}");
        Debug.Log($"[TutorialPointer] hint: {hint?.name}, message: {hint?.message}");
        Debug.Log($"[TutorialPointer] direction: {hint?.direction}, offset: {hint?.offset}");
        Debug.Log($"[TutorialPointer] ========================================");

        StopAllCoroutines();
        gameObject.SetActive(true);

        Debug.Log($"[TutorialPointer] gameObject.activeSelf: {gameObject.activeSelf}");
        Debug.Log($"[TutorialPointer] canvasGroup.alpha before: {canvasGroup.alpha}");

        messageText.text = hint.message;

        // œ–Œ¬≈– ¿  ŒÃœŒÕ≈Õ“Œ¬
        Debug.Log($"[TutorialPointer] Checking components:");
        Debug.Log($"[TutorialPointer]   canvasGroup: {(canvasGroup != null ? "OK" : "NULL!")}");
        Debug.Log($"[TutorialPointer]   arrowRect: {(arrowRect != null ? "OK" : "NULL!")}");
        Debug.Log($"[TutorialPointer]   messageText: {(messageText != null ? "OK" : "NULL!")}");
        Debug.Log($"[TutorialPointer]   messageBackground: {(messageBackground != null ? "OK" : "NULL!")}");

        // œ–Œ¬≈– ¿ PARENT CANVAS
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        Debug.Log($"[TutorialPointer] Parent Canvas: {(parentCanvas != null ? parentCanvas.name : "NULL")}");
        if (parentCanvas != null)
        {
            Debug.Log($"[TutorialPointer]   renderMode: {parentCanvas.renderMode}");
            Debug.Log($"[TutorialPointer]   sortOrder: {parentCanvas.sortingOrder}");
            Debug.Log($"[TutorialPointer]   pixelRect: {parentCanvas.pixelRect}");
        }

        // œ–Œ¬≈– ¿ RECTTRANSFORM
        RectTransform myRect = GetComponent<RectTransform>();
        Debug.Log($"[TutorialPointer] My RectTransform:");
        Debug.Log($"[TutorialPointer]   anchoredPosition: {myRect.anchoredPosition}");
        Debug.Log($"[TutorialPointer]   sizeDelta: {myRect.sizeDelta}");
        Debug.Log($"[TutorialPointer]   anchorMin: {myRect.anchorMin}, anchorMax: {myRect.anchorMax}");
        Debug.Log($"[TutorialPointer]   pivot: {myRect.pivot}");

        PositionRelativeTo(targetButton, hint.direction, hint.offset);
        StartCoroutine(ShowAfterDelay(hint.showDelay));
    }

    private IEnumerator ShowAfterDelay(float delay)
    {
        Debug.Log($"[TutorialPointer] ShowAfterDelay: waiting {delay}s");
        yield return new WaitForSeconds(delay);

        Debug.Log($"[TutorialPointer] Starting fade in...");
        float elapsed = 0f;

        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / 0.2f);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        Debug.Log($"[TutorialPointer] Fade in complete, alpha={canvasGroup.alpha}");
        StartCoroutine(BounceRoutine());
    }

    public void Hide(bool instant = false)
    {
        Debug.Log($"[TutorialPointer] Hide called, instant={instant}");
        StopAllCoroutines();

        if (instant)
        {
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            return;
        }

        StartCoroutine(HideRoutine());
    }

    private IEnumerator HideRoutine()
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < 0.15f)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / 0.15f);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    private void PositionRelativeTo(RectTransform target, PointerDirection dir, float offset)
    {
        Debug.Log($"[TutorialPointer] === POSITIONING START ===");

        if (target == null)
        {
            Debug.LogError("[TutorialPointer] target is NULL!");
            return;
        }

        if (arrowRect == null)
        {
            Debug.LogError("[TutorialPointer] arrowRect is NULL!");
            return;
        }

        // »Õ‘Œ Œ ÷≈À»
        Debug.Log($"[TutorialPointer] Target info:");
        Debug.Log($"[TutorialPointer]   name: {target.name}");
        Debug.Log($"[TutorialPointer]   position (world): {target.position}");
        Debug.Log($"[TutorialPointer]   rect.size: {target.rect.size}");
        Debug.Log($"[TutorialPointer]   lossyScale: {target.lossyScale}");
        Debug.Log($"[TutorialPointer]   anchoredPosition: {target.anchoredPosition}");

        //  ŒÕ¬≈–“¿÷»ﬂ ¬ › –¿ÕÕ€≈  ŒŒ–ƒ»Õ¿“€
        Vector3 targetScreenPos = RectTransformUtility.WorldToScreenPoint(null, target.position);
        Debug.Log($"[TutorialPointer] targetScreenPos: {targetScreenPos}");

        // œ–Œ¬≈– ¿, ¬ œ–≈ƒ≈À¿’ À» › –¿Õ¿
        if (targetScreenPos.x < 0 || targetScreenPos.x > Screen.width ||
            targetScreenPos.y < 0 || targetScreenPos.y > Screen.height)
        {
            Debug.LogWarning($"[TutorialPointer] Target is OFF SCREEN! Screen: {Screen.width}x{Screen.height}");
        }

        //  ŒÕ¬≈–“¿÷»ﬂ ¬ ÀŒ ¿À‹Õ€≈  ŒŒ–ƒ»Õ¿“€  ¿Õ¬¿—¿
        RectTransform parentRect = (RectTransform)transform.parent;
        Debug.Log($"[TutorialPointer] Parent RectTransform: {parentRect.name}");
        Debug.Log($"[TutorialPointer]   rect: {parentRect.rect}");
        Debug.Log($"[TutorialPointer]   sizeDelta: {parentRect.sizeDelta}");

        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            targetScreenPos,
            null,
            out Vector2 localPos
        );

        Debug.Log($"[TutorialPointer] ScreenPointToLocalPointInRectangle success: {success}");
        Debug.Log($"[TutorialPointer] localPos: {localPos}");

        Vector2 targetSize = target.rect.size * target.lossyScale;
        Debug.Log($"[TutorialPointer] targetSize: {targetSize}");

        Vector2 arrowPos = localPos;
        Vector2 messagePos = localPos;

        // œŒ«»÷»ŒÕ»–Œ¬¿Õ»≈ œŒ Õ¿œ–¿¬À≈Õ»ﬁ
        switch (dir)
        {
            case PointerDirection.Top:
                arrowRect.rotation = Quaternion.Euler(0, 0, 0);
                arrowPos.y += targetSize.y / 2 + offset;
                messagePos = arrowPos + new Vector2(0, 40);
                messageText.alignment = TextAlignmentOptions.Center;
                Debug.Log($"[TutorialPointer] Direction: TOP");
                break;

            case PointerDirection.Bottom:
                arrowRect.rotation = Quaternion.Euler(0, 0, 180);
                arrowPos.y -= targetSize.y / 2 + offset;
                messagePos = arrowPos - new Vector2(0, 40);
                messageText.alignment = TextAlignmentOptions.Center;
                Debug.Log($"[TutorialPointer] Direction: BOTTOM");
                break;

            case PointerDirection.Left:
                arrowRect.rotation = Quaternion.Euler(0, 0, 90);
                arrowPos.x -= targetSize.x / 2 + offset;
                messagePos = arrowPos - new Vector2(100, 0);
                messageText.alignment = TextAlignmentOptions.Right;
                Debug.Log($"[TutorialPointer] Direction: LEFT");
                break;

            case PointerDirection.Right:
                arrowRect.rotation = Quaternion.Euler(0, 0, -90);
                arrowPos.x += targetSize.x / 2 + offset;
                messagePos = arrowPos + new Vector2(100, 0);
                messageText.alignment = TextAlignmentOptions.Left;
                Debug.Log($"[TutorialPointer] Direction: RIGHT");
                break;
        }

        Debug.Log($"[TutorialPointer] Final arrowPos: {arrowPos}");
        Debug.Log($"[TutorialPointer] Final messagePos: {messagePos}");

        // œ–Œ¬≈– ¿, Õ≈ ”À≈“≈À» À»  ŒŒ–ƒ»Õ¿“€
        if (float.IsNaN(arrowPos.x) || float.IsNaN(arrowPos.y))
        {
            Debug.LogError("[TutorialPointer] arrowPos is NaN!");
        }

        if (Mathf.Abs(arrowPos.x) > 10000 || Mathf.Abs(arrowPos.y) > 10000)
        {
            Debug.LogWarning($"[TutorialPointer] arrowPos is very large: {arrowPos}");
        }

        arrowRect.localPosition = arrowPos;
        messageBackground.localPosition = messagePos;
        arrowStartPos = arrowPos;

        Debug.Log($"[TutorialPointer] Arrow localPosition set to: {arrowRect.localPosition}");
        Debug.Log($"[TutorialPointer] Message localPosition set to: {messageBackground.localPosition}");
        Debug.Log($"[TutorialPointer] === POSITIONING END ===");
    }

    private IEnumerator BounceRoutine()
    {
        Debug.Log("[TutorialPointer] Bounce animation started");
        while (true)
        {
            float offsetY = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
            arrowRect.localPosition = arrowStartPos + new Vector3(0, offsetY, 0);
            yield return null;
        }
    }

    private void OnDisable()
    {
        Debug.Log("[TutorialPointer] OnDisable");
        StopAllCoroutines();
    }

    private void OnEnable()
    {
        Debug.Log("[TutorialPointer] OnEnable");
    }
}