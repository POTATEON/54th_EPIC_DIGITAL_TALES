using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Вешается на пустой GO с Collider2D (isTrigger = true).
///
/// Когда игрок входит в зону:
///   1. Показывает подсказку "нажми E чтобы подобрать"
///   2. По нажатию Interact (E) — делает AbilityButton видимой и регистрирует в AbilityRegistry
///   3. Показывает InfoPopup с описанием способности
///   4. Уничтожает себя (опционально)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AbilityPickup : MonoBehaviour
{
    [Header("Способность")]
    [Tooltip("AbilityButton который нужно разблокировать. Должен быть скрыт (SetActive false) в сцене.")]
    [SerializeField] private AbilityButton abilityButton;

    [Header("Подсказка при входе в зону")]
    [SerializeField] private GameObject interactHint;                  // GO с текстом "Нажми E"
    [SerializeField] private string hintText = "Нажми [E] чтобы изучить способность";

    [Header("Popup после подбора")]
    [SerializeField] private bool showPopupAfterPickup = true;
    [TextArea(2, 4)]
    [SerializeField] private string popupMessage = "Новая способность получена!";
    [SerializeField] private float popupDuration = 3f;

    [Header("Настройки")]
    [SerializeField] private bool destroyAfterPickup = true;           // уничтожить триггер после подбора

    // ---------------------------------------------------------------

    private PlayerControls _playerControls;
    private bool _playerInRange;
    private bool _picked;

    // Для автосозданного попапа (если InfoPopup не назначен)
    private GameObject _popup;
    private Camera _cam;

    private void Awake()
    {
        _playerControls = new PlayerControls();
        _cam = Camera.main;

        var col = GetComponent<Collider2D>();
        if (!col.isTrigger) col.isTrigger = true;

        // Кнопка должна быть скрыта до подбора
        if (abilityButton != null)
            abilityButton.gameObject.SetActive(false);

        if (interactHint != null)
            interactHint.SetActive(false);
    }

    private void OnEnable() => _playerControls?.Player.Enable();
    private void OnDisable() => _playerControls?.Player.Disable();

    private void Update()
    {
        if (!_playerInRange || _picked) return;

        if (_playerControls.Player.Interact.WasPressedThisFrame())
            Pickup();
    }

    // ---------------------------------------------------------------
    // Trigger
    // ---------------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_picked) return;
        if (!other.CompareTag("Player")) return;

        _playerInRange = true;

        if (interactHint != null)
            interactHint.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInRange = false;

        if (interactHint != null)
            interactHint.SetActive(false);
    }

    // ---------------------------------------------------------------
    // Pickup
    // ---------------------------------------------------------------

    private void Pickup()
    {
        _picked = true;

        if (interactHint != null)
            interactHint.SetActive(false);

        // Показываем кнопку — AbilityButton.Start() сам зарегистрируется в AbilityRegistry
        if (abilityButton != null)
        {
            abilityButton.gameObject.SetActive(true);
            Debug.Log($"[AbilityPickup] Способность '{abilityButton.AbilityId}' разблокирована");
        }
        else
        {
            Debug.LogWarning("[AbilityPickup] abilityButton не назначен!");
        }

        if (showPopupAfterPickup)
            StartCoroutine(ShowPopupAndFinish());
        else
            Finish();
    }

    // ---------------------------------------------------------------
    // Popup
    // ---------------------------------------------------------------

    private IEnumerator ShowPopupAndFinish()
    {
        SpawnPopup();
        yield return new WaitForSeconds(popupDuration);
        DestroyPopup();
        Finish();
    }

    private void SpawnPopup()
    {
        _popup = new GameObject("[AbilityPickup] Popup");
        _popup.transform.position = transform.position + Vector3.up * 1.5f;

        // Фон
        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(_popup.transform, false);
        var bg = bgGo.AddComponent<SpriteRenderer>();
        bg.sprite = CreateRectSprite();
        bg.color = new Color(0f, 0f, 0f, 0.8f);
        bg.sortingOrder = 10;

        // Текст
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(_popup.transform, false);
        textGo.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        var tmp = textGo.AddComponent<TextMeshPro>();
        tmp.text = popupMessage;
        tmp.fontSize = 3f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.GetComponent<MeshRenderer>().sortingOrder = 11;

        tmp.ForceMeshUpdate();
        float w = tmp.preferredWidth + 0.3f * 2f;
        float h = tmp.preferredHeight + 0.15f * 2f;
        bg.transform.localScale = new Vector3(w, h, 1f);
    }

    private void DestroyPopup()
    {
        if (_popup != null) Destroy(_popup);
    }

    private void LateUpdate()
    {
        if (_popup == null) return;
        if (_cam != null)
            _popup.transform.rotation = _cam.transform.rotation;
    }

    // ---------------------------------------------------------------
    // Finish
    // ---------------------------------------------------------------

    private void Finish()
    {
        if (destroyAfterPickup)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------

    private static Sprite CreateRectSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private void OnDestroy()
    {
        DestroyPopup();
        _playerControls?.Dispose();
    }
}