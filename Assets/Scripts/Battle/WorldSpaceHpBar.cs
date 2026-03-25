using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Вешается на Canvas (World Space).
/// 
/// Правильная иерархия:
/// Player
/// └── HpBarAnchor (пустой GO, Pos Y = 1.5 над головой)
///     └── HpBarCanvas (Canvas World Space, Scale 0.01/0.01/0.01)
///         └── WorldSpaceHpBar (этот компонент)
///             ├── Slider
///             └── TMP_Text
///
/// Позиция задаётся через HpBarAnchor — просто поставь его выше головы.
/// Canvas Rect Transform оставь по умолчанию (Pos 0,0,0).
/// </summary>
public class WorldSpaceHpBar : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;

    private Camera _cam;
    private float _maxHp;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Start()
    {
        // Start вызывается после всех Awake — к этому моменту
        // BattleManager уже мог вызвать Show(), поэтому проверяем
        // нужно ли скрывать (скрываем только если Show ещё не звали)
        if (!_wasShown)
            gameObject.SetActive(false);
    }

    private bool _wasShown;

    private void LateUpdate()
    {
        // Только разворачиваем к камере — позиция управляется якорем
        if (_cam != null)
            transform.rotation = _cam.transform.rotation;
    }

    public void Show()
    {
        _wasShown = true;
        gameObject.SetActive(true);
    }
    public void Hide() => gameObject.SetActive(false);

    public void Setup(float maxHp)
    {
        _maxHp = maxHp;
        SetValue(maxHp, maxHp);
    }

    public void SetText(string text)
    {
        if (hpText != null) hpText.text = $"HP: {text}";
        if (hpSlider != null) hpSlider.value = 1f;
    }

    public void SetValue(float current, float max)
    {
        if (max > 0f) _maxHp = max;
        if (hpSlider != null) hpSlider.value = _maxHp > 0 ? current / _maxHp : 0f;
        if (hpText != null) hpText.text = $"HP: {Mathf.CeilToInt(current)}";
    }
}