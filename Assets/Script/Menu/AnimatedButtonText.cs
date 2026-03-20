// AnimatedButtonText.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class AnimatedButtonText : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler,
    ISelectHandler, IDeselectHandler
{
    [Header("Настройки компонента")]
    [Tooltip("Дочерний узел TextMeshPro внутри кнопки. Если оставить пустым, найдёт автоматически")]
    public TMP_Text buttonText;

    [Header("Цвета кнопки")]
    [Space(10)]
    public Color normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    public Color hoverColor = new Color(0.1f, 0.3f, 0.6f, 1f);
    public Color pressedColor = new Color(0.8f, 0.1f, 0.1f, 1f);
    public Color selectedColor = new Color(0f, 0.6f, 0f, 1f);
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [Header("Настройки анимации")]
    [Space(10)]
    [Range(1f, 30f)]
    public float colorChangeSpeed = 8f;
    public bool useSmoothTransition = true;
    public bool returnToNormalOnRelease = false;

    [Header("Дополнительные эффекты")]
    [Space(10)]
    public bool scaleOnHover = false;
    [Range(0.5f, 2f)]
    public float hoverScale = 1.1f;
    [Range(1f, 20f)]
    public float scaleSpeed = 10f;
    public bool addTextShadow = true;
    public Color shadowColor = new Color(0f, 0f, 0f, 0.5f);

    // Внутренние переменные
    private Color targetColor;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Button button;
    private CanvasGroup textCanvasGroup;
    private Shadow textShadow;

    // FIX: отслеживаем предыдущее состояние interactable чтобы
    // реагировать на момент изменения, а не только в одну сторону
    private bool wasInteractable;

    void Start()
    {
        InitializeComponents();

        originalScale = buttonText.transform.localScale;
        targetScale = originalScale;

        bool interactable = button == null || button.interactable;
        wasInteractable = interactable;
        targetColor = interactable ? normalColor : disabledColor;
        buttonText.color = targetColor;

        if (addTextShadow && textShadow == null)
            AddTextShadow();

        if (textCanvasGroup != null)
            textCanvasGroup.alpha = interactable ? 1f : 0.5f;
    }

    void InitializeComponents()
    {
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TMP_Text>(true);
            if (buttonText == null)
            {
                Debug.LogWarning("AnimatedButtonText: TMP_Text не найден на: " + gameObject.name);
                enabled = false;
                return;
            }
        }

        button = GetComponent<Button>();

        textCanvasGroup = buttonText.GetComponent<CanvasGroup>();
        if (textCanvasGroup == null)
            textCanvasGroup = buttonText.gameObject.AddComponent<CanvasGroup>();

        textShadow = buttonText.GetComponent<Shadow>();
    }

    void AddTextShadow()
    {
        textShadow = buttonText.gameObject.AddComponent<Shadow>();
        textShadow.effectColor = shadowColor;
        textShadow.effectDistance = new Vector2(2f, -2f);
    }

    void Update()
    {
        if (buttonText == null) return;

        // --- Плавное изменение цвета ---
        buttonText.color = useSmoothTransition
            ? Color.Lerp(buttonText.color, targetColor, Time.deltaTime * colorChangeSpeed)
            : targetColor;

        // --- Плавное изменение масштаба ---
        if (scaleOnHover)
        {
            buttonText.transform.localScale = Vector3.Lerp(
                buttonText.transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        }

        // --- Отслеживание изменения interactable ---
        // FIX: реагируем на оба направления: enabled→disabled и disabled→enabled.
        // Раньше была только одна ветка (→ disabled), поэтому при восстановлении
        // кнопки цвет оставался серым до наведения курсора.
        if (button == null) return;

        bool interactableNow = button.interactable;
        if (interactableNow == wasInteractable) return; // состояние не изменилось

        wasInteractable = interactableNow;

        if (!interactableNow)
        {
            // Кнопка стала disabled
            targetColor = disabledColor;
            if (textCanvasGroup != null) textCanvasGroup.alpha = 0.5f;
        }
        else
        {
            // Кнопка снова стала enabled — сбрасываем в normalColor
            targetColor = normalColor;
            if (textCanvasGroup != null) textCanvasGroup.alpha = 1f;
        }
    }

    // === Pointer Events ===

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        targetColor = hoverColor;
        if (scaleOnHover) targetScale = originalScale * hoverScale;
        if (textCanvasGroup != null) textCanvasGroup.alpha = 1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button != null && !button.interactable)
        {
            targetColor = disabledColor;
            if (textCanvasGroup != null) textCanvasGroup.alpha = 0.5f;
        }
        else
        {
            targetColor = normalColor;
        }
        if (scaleOnHover) targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        targetColor = pressedColor;
        if (scaleOnHover) targetScale = originalScale * 0.95f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        targetColor = returnToNormalOnRelease ? normalColor : hoverColor;
        if (scaleOnHover) targetScale = originalScale * hoverScale;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (button != null && !button.interactable) return;
        targetColor = selectedColor;
        if (scaleOnHover) targetScale = originalScale * hoverScale;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (button != null && !button.interactable)
        {
            targetColor = disabledColor;
            if (textCanvasGroup != null) textCanvasGroup.alpha = 0.5f;
        }
        else
        {
            targetColor = normalColor;
        }
        if (scaleOnHover) targetScale = originalScale;
    }

    // === Публичные методы ===

    public void SetTextColor(Color color, bool immediate = false)
    {
        targetColor = color;
        if (immediate) buttonText.color = color;
    }

    public void ResetToNormalColor(bool immediate = false)
    {
        targetColor = normalColor;
        if (immediate) buttonText.color = normalColor;
        if (scaleOnHover) targetScale = originalScale;
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null) button.interactable = interactable;
        // wasInteractable обновится автоматически в следующем Update
    }

    public void ChangeColors(Color newNormal, Color newHover, Color newPressed)
    {
        normalColor = newNormal;
        hoverColor = newHover;
        pressedColor = newPressed;
        if (targetColor == normalColor) targetColor = newNormal;
    }

    // === Unity Editor ===

    void OnValidate()
    {
        if (buttonText == null)
            buttonText = GetComponentInChildren<TMP_Text>(true);
    }

    void OnDestroy()
    {
        if (buttonText != null)
        {
            buttonText.color = normalColor;
            buttonText.transform.localScale = originalScale;
        }
    }
}