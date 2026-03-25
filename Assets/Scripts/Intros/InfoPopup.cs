using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Вешается на любой GO с Collider2D (isTrigger = true).
/// Когда игрок входит — показывает всплывающее сообщение над объектом.
///
/// Режимы исчезновения:
///   AutoHide  — исчезает через showDuration секунд
///   OnExit    — исчезает когда игрок выходит из триггера
/// </summary>
public class InfoPopup : MonoBehaviour
{
    public enum HideMode { AutoHide, OnExit }
    public enum BgShape { Rectangle, Rounded, Circle, Custom }

    // ---------------------------------------------------------------
    // Инспектор
    // ---------------------------------------------------------------

    [Header("Текст сообщения")]
    [TextArea(2, 5)]
    [SerializeField] private string message = "Введи текст...";

    [Header("Позиция (смещение от центра триггера)")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, 0f);

    [Header("Настройки показа")]
    [SerializeField] private HideMode hideMode = HideMode.AutoHide;
    [SerializeField] private float showDuration = 3f;
    [SerializeField] private bool showOnce = true;

    [Header("Анимация")]
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("Текст (TMP)")]
    [SerializeField] private float fontSize = 3f;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private TMP_FontAsset fontAsset;                          // null = TMP default
    [SerializeField] private FontStyles fontStyle = FontStyles.Normal;
    [SerializeField] private TextAlignmentOptions textAlignment = TextAlignmentOptions.Center;
    [SerializeField] private bool enableWordWrapping = false;
    [SerializeField] private float maxWidth = 0f;                              // 0 = без ограничений

    [Header("Фон")]
    [SerializeField] private Color bgColor = new Color(0f, 0f, 0f, 0.75f);
    [SerializeField] private Vector2 padding = new Vector2(0.3f, 0.15f);
    [SerializeField] private BgShape bgShape = BgShape.Rectangle;
    [Tooltip("Только для Rounded: доля скругления от меньшей стороны (0 = острые углы, 0.5 = таблетка)")]
    [SerializeField][Range(0f, 0.5f)] private float cornerRadius = 0.2f;
    [Tooltip("Только для Custom: свой спрайт формы")]
    [SerializeField] private Sprite customBgSprite;

    [Header("Опционально: кастомный префаб (иначе создаётся автоматически)")]
    [SerializeField] private GameObject popupPrefab;

    // ---------------------------------------------------------------
    // Приватное состояние
    // ---------------------------------------------------------------

    private GameObject _popup;
    private TMP_Text _tmpText;
    private SpriteRenderer _bg;
    private Coroutine _fadeCoroutine;
    private bool _wasShown;
    private bool _isVisible;
    private Camera _cam;

    // ---------------------------------------------------------------
    // Init
    // ---------------------------------------------------------------

    private void Awake()
    {
        _cam = Camera.main;

        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"[InfoPopup] Collider2D на {gameObject.name} не isTrigger — включаю принудительно.");
            col.isTrigger = true;
        }
    }

    // ---------------------------------------------------------------
    // Trigger
    // ---------------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (showOnce && _wasShown) return;

        _wasShown = true;
        Show();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (hideMode == HideMode.OnExit)
            Hide();
    }

    // ---------------------------------------------------------------
    // Public API
    // ---------------------------------------------------------------

    public void Show()
    {
        if (_isVisible) return;

        EnsurePopup();
        _popup.transform.position = transform.position + offset;
        _popup.SetActive(true);
        _isVisible = true;

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeRoutine(0f, 1f, () =>
        {
            if (hideMode == HideMode.AutoHide)
                _fadeCoroutine = StartCoroutine(AutoHideRoutine());
        }));
    }

    public void Hide()
    {
        if (!_isVisible) return;

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeRoutine(1f, 0f, () =>
        {
            _popup?.SetActive(false);
            _isVisible = false;
        }));
    }

    /// <summary>Меняет текст на лету, фон автоматически пересчитывается.</summary>
    public void SetMessage(string text)
    {
        message = text;
        if (_tmpText != null)
        {
            _tmpText.text = text;
            ResizeBackground();
        }
    }

    // ---------------------------------------------------------------
    // Popup construction
    // ---------------------------------------------------------------

    private void EnsurePopup()
    {
        if (_popup != null) return;

        if (popupPrefab != null)
        {
            _popup = Instantiate(popupPrefab, transform.position + offset, Quaternion.identity);
            _tmpText = _popup.GetComponentInChildren<TMP_Text>();
            _bg = _popup.GetComponentInChildren<SpriteRenderer>();
            if (_tmpText != null) _tmpText.text = message;
            return;
        }

        _popup = new GameObject($"[InfoPopup] {gameObject.name}");

        BuildBackground();
        BuildText();

        _tmpText.ForceMeshUpdate();
        ResizeBackground();

        _popup.SetActive(false);
        SetAlpha(0f);
    }

    private void BuildBackground()
    {
        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(_popup.transform, false);

        _bg = bgGo.AddComponent<SpriteRenderer>();
        _bg.color = bgColor;
        _bg.sortingOrder = 10;
        _bg.sprite = bgShape == BgShape.Custom && customBgSprite != null
                               ? customBgSprite
                               : CreateShapeSprite(bgShape, cornerRadius);
    }

    private void BuildText()
    {
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(_popup.transform, false);
        // Z-смещение — текст физически ближе к камере чем фон
        textGo.transform.localPosition = new Vector3(0f, 0f, -0.1f);

        _tmpText = textGo.AddComponent<TextMeshPro>();
        _tmpText.text = message;
        _tmpText.fontSize = fontSize;
        _tmpText.color = textColor;
        _tmpText.alignment = textAlignment;
        _tmpText.fontStyle = fontStyle;
        _tmpText.enableWordWrapping = enableWordWrapping;

        if (fontAsset != null)
            _tmpText.font = fontAsset;

        if (maxWidth > 0f)
        {
            _tmpText.enableWordWrapping = true;
            _tmpText.rectTransform.sizeDelta = new Vector2(maxWidth, 0f);
        }

        // sortingOrder на MeshRenderer TMP — гарантируем рендер поверх фона
        _tmpText.GetComponent<MeshRenderer>().sortingOrder = 11;
    }

    // ---------------------------------------------------------------
    // Background sprite generation
    // ---------------------------------------------------------------

    private static Sprite CreateShapeSprite(BgShape shape, float cornerFraction)
    {
        return shape switch
        {
            BgShape.Circle => CreateCircleSprite(64),
            BgShape.Rounded => CreateRoundedSprite(128, 128, cornerFraction),
            _ => CreateRectSprite(),
        };
    }

    /// <summary>1×1 белый квадрат — растягивается через localScale.</summary>
    private static Sprite CreateRectSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    /// <summary>Круг через SDF в текстуре res×res.</summary>
    private static Sprite CreateCircleSprite(int res)
    {
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float r = res * 0.5f;

        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                float dx = x - r + 0.5f;
                float dy = y - r + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01((r - dist) / 1.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
    }

    /// <summary>
    /// Прямоугольник со скруглёнными углами.
    /// cornerFraction — доля от половины меньшей стороны (0 = острые, 0.5 = таблетка).
    /// </summary>
    private static Sprite CreateRoundedSprite(int w, int h, float cornerFraction)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float r = Mathf.Min(w, h) * 0.5f * Mathf.Clamp(cornerFraction, 0f, 0.5f);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = Mathf.Max(0f, Mathf.Abs(x - w * 0.5f + 0.5f) - (w * 0.5f - r));
                float dy = Mathf.Max(0f, Mathf.Abs(y - h * 0.5f + 0.5f) - (h * 0.5f - r));
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                // Если r == 0 — просто белый пиксель (прямой угол)
                float a = r > 0f ? Mathf.Clamp01((r - dist) / 1.5f) : 1f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), Mathf.Min(w, h));
    }

    // ---------------------------------------------------------------
    // Resize background to fit text
    // ---------------------------------------------------------------

    private void ResizeBackground()
    {
        if (_bg == null || _tmpText == null) return;
        _tmpText.ForceMeshUpdate();

        float w = _tmpText.preferredWidth + padding.x * 2f;
        float h = _tmpText.preferredHeight + padding.y * 2f;

        // Для круга берём квадрат чтобы спрайт не вытягивался в эллипс
        if (bgShape == BgShape.Circle)
        {
            float size = Mathf.Max(w, h);
            _bg.transform.localScale = new Vector3(size, size, 1f);
        }
        else
        {
            _bg.transform.localScale = new Vector3(w, h, 1f);
        }
    }

    // ---------------------------------------------------------------
    // Billboard (разворот к камере)
    // ---------------------------------------------------------------

    private void LateUpdate()
    {
        if (_popup == null || !_popup.activeSelf) return;
        if (_cam != null)
            _popup.transform.rotation = _cam.transform.rotation;
    }

    // ---------------------------------------------------------------
    // Fade
    // ---------------------------------------------------------------

    private IEnumerator FadeRoutine(float from, float to, System.Action onComplete = null)
    {
        float elapsed = 0f;
        SetAlpha(from);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, elapsed / fadeDuration));
            yield return null;
        }

        SetAlpha(to);
        onComplete?.Invoke();
    }

    private IEnumerator AutoHideRoutine()
    {
        yield return new WaitForSeconds(showDuration);
        Hide();
    }

    private void SetAlpha(float a)
    {
        if (_tmpText != null)
        {
            var c = _tmpText.color;
            _tmpText.color = new Color(c.r, c.g, c.b, a);
        }
        if (_bg != null)
        {
            var c = bgColor;
            _bg.color = new Color(c.r, c.g, c.b, c.a * a);
        }
    }

    // ---------------------------------------------------------------
    // Cleanup
    // ---------------------------------------------------------------

    private void OnDestroy()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        if (_popup != null) Destroy(_popup);
    }
}