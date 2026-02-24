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
    [Header("��������� ���������")]
    [Tooltip("�������� ���� TextMeshPro ����� ������. ���� �������� ������, ������ �������������")]
    public TMP_Text buttonText;

    [Header("����� ������")]
    [Space(10)]

    [Tooltip("���� ������ � ������� ���������")]
    public Color normalColor = new Color(0.2f, 0.2f, 0.2f, 1f); // �����-�����

    [Tooltip("���� ������ ��� ��������� �������")]
    public Color hoverColor = new Color(0.1f, 0.3f, 0.6f, 1f);   // �����

    [Tooltip("���� ������ ��� ������� ������ ����")]
    public Color pressedColor = new Color(0.8f, 0.1f, 0.1f, 1f); // �������

    [Tooltip("���� ������ ����� ������ ������� (��������, ����� Tab)")]
    public Color selectedColor = new Color(0f, 0.6f, 0f, 1f);    // �������

    [Tooltip("���� ������ ����� ������ ���������")]
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // ����� ��������������

    [Header("��������� ��������")]
    [Space(10)]

    [Tooltip("�������� �������� ��������� ����� (������������� 8-15)")]
    [Range(1f, 30f)]
    public float colorChangeSpeed = 8f;

    [Tooltip("������������ ������� ��������� �����")]
    public bool useSmoothTransition = true;

    [Tooltip("������������ � normalColor ��� ���������� ������ (����� �������� � hoverColor)")]
    public bool returnToNormalOnRelease = false;

    [Header("�������������� �������")]
    [Space(10)]

    [Tooltip("�������� ������ ������ ��� ���������")]
    public bool scaleOnHover = false;

    [Tooltip("��������� ������� ��� ���������")]
    [Range(0.5f, 2f)]
    public float hoverScale = 1.1f;

    [Tooltip("�������� ��������� �������")]
    [Range(1f, 20f)]
    public float scaleSpeed = 10f;

    [Tooltip("�������� ���� ������")]
    public bool addTextShadow = true;

    [Tooltip("���� ���� ������")]
    public Color shadowColor = new Color(0f, 0f, 0f, 0.5f);

    // ��������� ����������
    private Color targetColor;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Button button;
    private CanvasGroup textCanvasGroup;
    private Shadow textShadow;

    void Start()
    {
        // ������������� �����������
        InitializeComponents();

        // ��������� ������������ ������
        originalScale = buttonText.transform.localScale;
        targetScale = originalScale;

        // ������������� ��������� ����
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

        // ��������� ���� ���� �����
        if (addTextShadow && textShadow == null)
        {
            AddTextShadow();
        }

        // ����������� ������������ ��� CanvasGroup
        if (textCanvasGroup != null)
        {
            textCanvasGroup.alpha = (button != null && !button.interactable) ? 0.5f : 1f;
        }
    }

    void InitializeComponents()
    {
        // ������� ��������� ��������� ���� �� ��������
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TMP_Text>(true);

            if (buttonText == null)
            {
                Debug.LogWarning("TextMeshPro ����� �� ������ �� ������: " + gameObject.name);
                enabled = false;
                return;
            }
        }

        // ������� ��������� Button
        button = GetComponent<Button>();

        // ��������� ��� ������� CanvasGroup ��� ������ (��� ���������� �������������)
        textCanvasGroup = buttonText.GetComponent<CanvasGroup>();
        if (textCanvasGroup == null)
        {
            textCanvasGroup = buttonText.gameObject.AddComponent<CanvasGroup>();
        }

        // ��������� ���� �� ����
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

        // ������� ��������� �����
        if (useSmoothTransition)
        {
            buttonText.color = Color.Lerp(buttonText.color, targetColor,
                Time.deltaTime * colorChangeSpeed);
        }
        else
        {
            buttonText.color = targetColor;
        }

        // ������� ��������� �������
        if (scaleOnHover)
        {
            buttonText.transform.localScale = Vector3.Lerp(
                buttonText.transform.localScale,
                targetScale,
                Time.deltaTime * scaleSpeed
            );
        }

        // ������������� � ���������� ������
        if (button != null && !button.interactable && targetColor != disabledColor)
        {
            targetColor = disabledColor;
            if (textCanvasGroup != null) textCanvasGroup.alpha = 0.5f;
        }
    }

    // === ����������� ������� ===

    // ��� ��������� ����
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        targetColor = hoverColor;
        if (scaleOnHover) targetScale = originalScale * hoverScale;

        // ��������������� ������������ ���� ���� ���������
        if (textCanvasGroup != null) textCanvasGroup.alpha = 1f;
    }

    // ��� ����� ����
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

    // ��� ������� ������ ����
    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        targetColor = pressedColor;

        // ������ "�������" ���������
        if (scaleOnHover)
        {
            targetScale = originalScale * 0.95f;
        }
    }

    // ��� ���������� ������ ����
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

    // ��� ������ ������ (��������, ����� Tab)
    public void OnSelect(BaseEventData eventData)
    {
        if (button != null && !button.interactable) return;

        targetColor = selectedColor;
        if (scaleOnHover) targetScale = originalScale * hoverScale;
    }

    // ��� ������ ������
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

    // === ��������� ������ ��� ������� ���������� ===

    // ������������� ���������� ����
    public void SetTextColor(Color color, bool immediate = false)
    {
        targetColor = color;
        if (immediate) buttonText.color = color;
    }

    // ������� ������������ ����
    public void ResetToNormalColor(bool immediate = false)
    {
        targetColor = normalColor;
        if (immediate) buttonText.color = normalColor;
        if (scaleOnHover) targetScale = originalScale;
    }

    // ��������/��������� ������
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

    // �������� ���� � �������� �������
    public void ChangeColors(Color newNormal, Color newHover, Color newPressed)
    {
        normalColor = newNormal;
        hoverColor = newHover;
        pressedColor = newPressed;

        // ���� ������ � ���������� ��������� - ��������� ����
        if (targetColor == normalColor)
        {
            targetColor = newNormal;
        }
    }

    // === ������ ��� ��������� UNITY ===

    // �������� � ���������
    void OnValidate()
    {
        // ������������� ������� ����� ��� ��������� � ���������
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TMP_Text>(true);
        }

        // ��������� ���� ���� �����
        if (addTextShadow && Application.isEditor && !Application.isPlaying)
        {
            if (buttonText != null)
            {
                Shadow existingShadow = buttonText.GetComponent<Shadow>();
                // if (existingShadow == null)
                // {
                //     // � ��������� ������ �������������, ��������� ��� �������
                //     Debug.Log("���� ����� ��������� ��� ������� ����");
                // }
            }
        }
    }

    // ������� ��� �������� ����������
    void OnDestroy()
    {
        // ���������� ������������ ���� � ������
        if (buttonText != null)
        {
            buttonText.color = normalColor;
            buttonText.transform.localScale = originalScale;
        }
    }
}