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
    [Header("Текстовый компонент")]
    [Tooltip("Перетащи сюда TextMeshPro текст кнопки. Если оставить пустым, найдет автоматически")]
    public TMP_Text buttonText;

    [Header("ЦВЕТА ТЕКСТА")]
    [Space(10)]

    [Tooltip("Цвет текста в обычном состоянии")]
    public Color normalColor = new Color(0.2f, 0.2f, 0.2f, 1f); // Темно-серый

    [Tooltip("Цвет текста при наведении курсора")]
    public Color hoverColor = new Color(0.1f, 0.3f, 0.6f, 1f);   // Синий

    [Tooltip("Цвет текста при нажатии кнопки мыши")]
    public Color pressedColor = new Color(0.8f, 0.1f, 0.1f, 1f); // Красный

    [Tooltip("Цвет текста когда кнопка выбрана (например, через Tab)")]
    public Color selectedColor = new Color(0f, 0.6f, 0f, 1f);    // Зеленый

    [Tooltip("Цвет текста когда кнопка отключена")]
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Серый полупрозрачный

    [Header("НАСТРОЙКИ АНИМАЦИИ")]
    [Space(10)]

    [Tooltip("Скорость плавного изменения цвета (рекомендуется 8-15)")]
    [Range(1f, 30f)]
    public float colorChangeSpeed = 8f;

    [Tooltip("Использовать плавное изменение цвета")]
    public bool useSmoothTransition = true;

    [Tooltip("Возвращаться к normalColor при отпускании кнопки (иначе вернется к hoverColor)")]
    public bool returnToNormalOnRelease = false;

    [Header("ДОПОЛНИТЕЛЬНЫЕ ЭФФЕКТЫ")]
    [Space(10)]

    [Tooltip("Изменять размер текста при наведении")]
    public bool scaleOnHover = false;

    [Tooltip("Множитель размера при наведении")]
    [Range(0.5f, 2f)]
    public float hoverScale = 1.1f;

    [Tooltip("Скорость изменения размера")]
    [Range(1f, 20f)]
    public float scaleSpeed = 10f;

    [Tooltip("Добавить тень тексту")]
    public bool addTextShadow = true;

    [Tooltip("Цвет тени текста")]
    public Color shadowColor = new Color(0f, 0f, 0f, 0.5f);

    // Приватные переменные
    private Color targetColor;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Button button;
    private CanvasGroup textCanvasGroup;
    private Shadow textShadow;

    void Start()
    {
        // Инициализация компонентов
        InitializeComponents();

        // Сохраняем оригинальный размер
        originalScale = buttonText.transform.localScale;
        targetScale = originalScale;

        // Устанавливаем начальный цвет
        if (button != null && !button.interactable)
        {
            targetColor = disabledColor;
            buttonText.color = disabledColor;
        }
        else
        {
            targetColor = normalColor;
            buttonText.color = normalColor;
        }

        // Добавляем тень если нужно
        if (addTextShadow && textShadow == null)
        {
            AddTextShadow();
        }

        // Настраиваем прозрачность для CanvasGroup
        if (textCanvasGroup != null)
        {
            textCanvasGroup.alpha = (button != null && !button.interactable) ? 0.5f : 1f;
        }
    }

    void InitializeComponents()
    {
        // Находим текстовый компонент если не назначен
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TMP_Text>(true);

            if (buttonText == null)
            {
                Debug.LogWarning("TextMeshPro текст не найден на кнопке: " + gameObject.name);
                enabled = false;
                return;
            }
        }

        // Находим компонент Button
        button = GetComponent<Button>();

        // Добавляем или находим CanvasGroup для текста (для управления прозрачностью)
        textCanvasGroup = buttonText.GetComponent<CanvasGroup>();
        if (textCanvasGroup == null)
        {
            textCanvasGroup = buttonText.gameObject.AddComponent<CanvasGroup>();
        }

        // Проверяем есть ли тень
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

        // Плавное изменение цвета
        if (useSmoothTransition)
        {
            buttonText.color = Color.Lerp(buttonText.color, targetColor,
                Time.deltaTime * colorChangeSpeed);
        }
        else
        {
            buttonText.color = targetColor;
        }

        // Плавное изменение размера
        if (scaleOnHover)
        {
            buttonText.transform.localScale = Vector3.Lerp(
                buttonText.transform.localScale,
                targetScale,
                Time.deltaTime * scaleSpeed
            );
        }

        // Синхронизация с состоянием кнопки
        if (button != null && !button.interactable && targetColor != disabledColor)
        {
            targetColor = disabledColor;
            if (textCanvasGroup != null) textCanvasGroup.alpha = 0.5f;
        }
    }

    // === ОБРАБОТЧИКИ СОБЫТИЙ ===

    // При наведении мыши
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        targetColor = hoverColor;
        if (scaleOnHover) targetScale = originalScale * hoverScale;

        // Восстанавливаем прозрачность если была отключена
        if (textCanvasGroup != null) textCanvasGroup.alpha = 1f;
    }

    // При уходе мыши
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

    // При нажатии кнопки мыши
    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        targetColor = pressedColor;

        // Эффект "нажатия" масштабом
        if (scaleOnHover)
        {
            targetScale = originalScale * 0.95f;
        }
    }

    // При отпускании кнопки мыши
    public void OnPointerUp(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        if (returnToNormalOnRelease)
        {
            targetColor = normalColor;
        }
        else
        {
            targetColor = hoverColor;
        }

        if (scaleOnHover) targetScale = originalScale * hoverScale;
    }

    // При выборе кнопки (например, через Tab)
    public void OnSelect(BaseEventData eventData)
    {
        if (button != null && !button.interactable) return;

        targetColor = selectedColor;
        if (scaleOnHover) targetScale = originalScale * hoverScale;
    }

    // При снятии выбора
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

    // === ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ РУЧНОГО УПРАВЛЕНИЯ ===

    // Принудительно установить цвет
    public void SetTextColor(Color color, bool immediate = false)
    {
        targetColor = color;
        if (immediate) buttonText.color = color;
    }

    // Вернуть оригинальный цвет
    public void ResetToNormalColor(bool immediate = false)
    {
        targetColor = normalColor;
        if (immediate) buttonText.color = normalColor;
        if (scaleOnHover) targetScale = originalScale;
    }

    // Включить/выключить кнопку
    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }

        if (interactable)
        {
            targetColor = normalColor;
            if (textCanvasGroup != null) textCanvasGroup.alpha = 1f;
        }
        else
        {
            targetColor = disabledColor;
            if (textCanvasGroup != null) textCanvasGroup.alpha = 0.5f;
        }
    }

    // Изменить цвет в реальном времени
    public void ChangeColors(Color newNormal, Color newHover, Color newPressed)
    {
        normalColor = newNormal;
        hoverColor = newHover;
        pressedColor = newPressed;

        // Если сейчас в нормальном состоянии - обновляем цвет
        if (targetColor == normalColor)
        {
            targetColor = newNormal;
        }
    }

    // === МЕТОДЫ ДЛЯ РЕДАКТОРА UNITY ===

    // Проверка в редакторе
    void OnValidate()
    {
        // Автоматически находим текст при изменении в редакторе
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TMP_Text>(true);
        }

        // Обновляем тень если нужно
        if (addTextShadow && Application.isEditor && !Application.isPlaying)
        {
            if (buttonText != null)
            {
                Shadow existingShadow = buttonText.GetComponent<Shadow>();
                if (existingShadow == null)
                {
                    // В редакторе просто предупреждаем, добавится при запуске
                    Debug.Log("Тень будет добавлена при запуске игры");
                }
            }
        }
    }

    // Очистка при удалении компонента
    void OnDestroy()
    {
        // Возвращаем оригинальный цвет и размер
        if (buttonText != null)
        {
            buttonText.color = normalColor;
            buttonText.transform.localScale = originalScale;
        }
    }
}