using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueBodyText;
    [SerializeField] private Image speakerPortraitImage;
    [SerializeField] private GameObject portraitContainer;
    [SerializeField] private TMP_Text continueHintText;

    [Header("Choices")]
    [SerializeField] private GameObject choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;

    [Header("Typewriter Settings")]
    [SerializeField] private float typewriterSpeed = 0.04f;

    [Header("Animation Settings")]
    [SerializeField] private float fadeSpeed = 5f;

    private CanvasGroup _canvasGroup;
    private Coroutine _typewriterCoroutine;
    private bool _isTyping;
    private bool _isOpen;

    // Колбэк когда игрок выбрал ответ
    private System.Action<bool> _onChoiceSelected;

    private readonly List<GameObject> _spawnedButtons = new();

    public bool IsOpen => _isOpen;
    public bool IsTyping => _isTyping;
    public bool HasChoices => choicesContainer != null && choicesContainer.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _canvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = dialoguePanel.AddComponent<CanvasGroup>();

        dialoguePanel.SetActive(false);
        _canvasGroup.alpha = 0f;

        if (choicesContainer != null)
            choicesContainer.SetActive(false);
    }

    public void ShowLine(DialogueLine line, System.Action<bool> onChoiceSelected = null)
    {
        if (!_isOpen) OpenPanel();

        _onChoiceSelected = onChoiceSelected;

        // Имя
        if (speakerNameText != null)
            speakerNameText.text = string.IsNullOrEmpty(line.speakerName) ? "" : line.speakerName;

        // Портрет
        if (portraitContainer != null)
        {
            bool hasPortrait = line.speakerPortrait != null;
            portraitContainer.SetActive(hasPortrait);
            if (hasPortrait && speakerPortraitImage != null)
                speakerPortraitImage.sprite = line.speakerPortrait;
        }

        // Скрываем подсказку и выборы
        if (continueHintText != null) continueHintText.gameObject.SetActive(false);
        HideChoices();

        // Запускаем печать
        if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
        _typewriterCoroutine = StartCoroutine(TypewriterRoutine(line));
    }

    public void SkipTypewriter(DialogueLine line)
    {
        if (!_isTyping) return;
        StopCoroutine(_typewriterCoroutine);
        _isTyping = false;
        dialogueBodyText.maxVisibleCharacters = dialogueBodyText.text.Length;
        OnTypingFinished(line);
    }

    public void ClosePanel()
    {
        HideChoices();
        StartCoroutine(FadeOut());
    }

    private void OpenPanel()
    {
        _isOpen = true;
        dialoguePanel.SetActive(true);
        StopCoroutine(nameof(FadeOut));
        StartCoroutine(FadeIn());
    }

    private void OnTypingFinished(DialogueLine line)
    {
        // Если есть варианты — показываем их, иначе подсказку "продолжить"
        if (line.choices != null && line.choices.Length > 0)
            ShowChoices(line.choices);
        else
            ShowContinueHint();
    }

        private void ShowChoices(DialogueChoice[] choices)
    {
        if (choicesContainer == null || choiceButtonPrefab == null)
        {
            Debug.LogError("DialogueUI: choicesContainer или choiceButtonPrefab не назначены!");
            ShowContinueHint();
            return;
        }

        HideChoices();
        choicesContainer.SetActive(true);

        foreach (var choice in choices)
        {
            var btn = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            var choiceBtn = btn.GetComponent<DialogueChoiceButton>();

            if (choiceBtn == null)
            {
                Debug.LogError("DialogueChoiceButton компонент не найден на префабе!");
                continue;
            }

            // Копируем в локальные переменные чтобы лямбда захватила правильные значения
            bool isCorrect = choice.isCorrect;
            DialogueLine response = isCorrect ? choice.correctResponse : choice.wrongResponse;
            string choiceText = choice.choiceText;

            choiceBtn.Setup(choiceText, () =>
            {
                HideChoices();
                _onChoiceSelected?.Invoke(isCorrect);
                if (response != null)
                    ShowLine(response);
                else
                    ShowContinueHint();
            });

            _spawnedButtons.Add(btn);
        }
    }

    private void HideChoices()
    {
        foreach (var btn in _spawnedButtons)
            Destroy(btn);
        _spawnedButtons.Clear();

        if (choicesContainer != null)
            choicesContainer.SetActive(false);
    }

    private void ShowContinueHint()
    {
        if (continueHintText != null)
            continueHintText.gameObject.SetActive(true);
    }

    private IEnumerator TypewriterRoutine(DialogueLine line)
    {
        _isTyping = true;
        dialogueBodyText.text = line.text;
        dialogueBodyText.maxVisibleCharacters = 0;

        foreach (char _ in line.text)
        {
            dialogueBodyText.maxVisibleCharacters++;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        _isTyping = false;
        OnTypingFinished(line);
    }

    private IEnumerator FadeIn()
    {
        _canvasGroup.alpha = 0f;
        while (_canvasGroup.alpha < 1f)
        {
            _canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        _canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        while (_canvasGroup.alpha > 0f)
        {
            _canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        _canvasGroup.alpha = 0f;
        dialoguePanel.SetActive(false);
        _isOpen = false;
    }
}