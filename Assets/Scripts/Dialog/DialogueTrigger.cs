using System.Collections;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueData dialogueData;

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnEnter = false;
    [SerializeField] private GameObject interactHint;

    [Header("После окончания диалога")]
    [SerializeField]
    private DialogueTriggerEndBehaviour endBehaviour
        = DialogueTriggerEndBehaviour.ShowRepeatPrompt;

    private PlayerControls _playerControls;
    private bool _playerInRange;
    private int _currentLineIndex;
    private bool _dialogueActive;
    private bool _waitingForChoice;
    private bool _waitingForClose;
    private bool _dialogueCompleted;
    private Coroutine _waitAndCloseCoroutine;

    // Свойства для доступа извне
    public bool IsDialogueActive => _dialogueActive;
    public bool IsDialogueCompleted => _dialogueCompleted;

    private void Awake()
    {
        _playerControls = new PlayerControls();
        Debug.Log($"[DialogueTrigger] Awake на {gameObject.name}");
    }

    private void OnEnable()
    {
        _playerControls?.Player.Enable();
    }

    private void OnDisable()
    {
        _playerControls?.Player.Disable();
    }

    private void Update()
    {
        if (_playerControls == null) return;
        if (!_playerInRange) return;
        if (_waitingForChoice) return;
        if (_waitingForClose) return;

        if (_playerControls.Player.Interact.WasPressedThisFrame())
        {
            Debug.Log($"[DialogueTrigger] Interact нажат | _dialogueActive={_dialogueActive} | _dialogueCompleted={_dialogueCompleted} | _currentLineIndex={_currentLineIndex}");
            OnInteract();
        }
    }

    private void OnInteract()
    {
        // Если диалог завершён и не активен
        if (_dialogueCompleted && !_dialogueActive)
        {
            Debug.Log("[DialogueTrigger] Диалог завершён → HandleRepeatInteraction");
            HandleRepeatInteraction();
            return;
        }

        // Если диалог не активен - начинаем новый
        if (!_dialogueActive)
        {
            StartDialogue();
            return;
        }

        // Если идёт печать текста - пропускаем
        if (DialogueUI.Instance.IsTyping)
        {
            Debug.Log("[DialogueTrigger] Пропускаем печать");
            DialogueUI.Instance.SkipTypewriter(dialogueData.lines[_currentLineIndex]);
            return;
        }

        // Если есть активные выборы - ждём
        if (DialogueUI.Instance.HasChoices) return;

        // Переходим к следующей реплике
        _currentLineIndex++;
        Debug.Log($"[DialogueTrigger] Переход к реплике {_currentLineIndex} из {dialogueData.lines.Length}");

        if (_currentLineIndex < dialogueData.lines.Length)
        {
            ShowCurrentLine();
        }
        else
        {
            Debug.Log("[DialogueTrigger] Все реплики показаны → EndDialogue");
            EndDialogue();
        }
    }

    private void HandleRepeatInteraction()
    {
        Debug.Log($"[DialogueTrigger] HandleRepeatInteraction | endBehaviour={endBehaviour}");

        switch (endBehaviour)
        {
            case DialogueTriggerEndBehaviour.DoNothing:
                break;

            case DialogueTriggerEndBehaviour.RestartDialogue:
                _dialogueCompleted = false;
                StartDialogue();
                break;

            case DialogueTriggerEndBehaviour.ShowRepeatPrompt:
                ShowRepeatPrompt();
                break;

            case DialogueTriggerEndBehaviour.DisableTrigger:
                gameObject.SetActive(false);
                break;
        }
    }

    private void ShowRepeatPrompt()
    {
        if (dialogueData.repeatPromptLine == null)
        {
            Debug.LogWarning("[DialogueTrigger] repeatPromptLine не назначен → перезапускаем диалог");
            _dialogueCompleted = false;
            StartDialogue();
            return;
        }

        _dialogueActive = true;
        _waitingForChoice = true;

        if (interactHint != null)
            interactHint.SetActive(false);

        // Создаём варианты выбора
        var choices = new DialogueChoice[]
        {
            new DialogueChoice
            {
                choiceText = dialogueData.repeatChoiceText,
                isCorrect = true,
                correctResponse = null
            },
            new DialogueChoice
            {
                choiceText = dialogueData.skipChoiceText,
                isCorrect = false,
                wrongResponse = null
            }
        };

        var promptLine = dialogueData.repeatPromptLine;
        var originalChoices = promptLine.choices;
        promptLine.choices = choices;

        DialogueUI.Instance.ShowLine(promptLine, (isCorrect) =>
        {
            promptLine.choices = originalChoices;
            _waitingForChoice = false;

            Debug.Log($"[DialogueTrigger] Выбор в RepeatPrompt: isCorrect={isCorrect}");

            if (isCorrect)
            {
                Debug.Log("[DialogueTrigger] Повторяем диалог");
                _dialogueCompleted = false;
                _dialogueActive = false;
                StartDialogue();
            }
            else
            {
                Debug.Log("[DialogueTrigger] Закрываем без повтора");
                CloseDialogue();
            }
        });
    }

    private void StartDialogue()
    {
        if (dialogueData == null)
        {
            Debug.LogError("[DialogueTrigger] dialogueData не назначен!");
            return;
        }

        if (dialogueData.lines == null || dialogueData.lines.Length == 0)
        {
            Debug.LogError("[DialogueTrigger] dialogueData.lines пустой!");
            return;
        }

        _dialogueActive = true;
        _currentLineIndex = 0;

        if (interactHint != null)
            interactHint.SetActive(false);

        Debug.Log($"[DialogueTrigger] StartDialogue | реплик: {dialogueData.lines.Length} | finalLine: {dialogueData.finalLine != null}");

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        var line = dialogueData.lines[_currentLineIndex];
        bool hasChoices = line.choices != null && line.choices.Length > 0;
        _waitingForChoice = hasChoices;

        Debug.Log($"[DialogueTrigger] ShowCurrentLine [{_currentLineIndex}]: \"{line.text}\" | hasChoices={hasChoices}");

        DialogueUI.Instance.ShowLine(line, (isCorrect) =>
        {
            Debug.Log($"[DialogueTrigger] Выбор на [{_currentLineIndex}]: isCorrect={isCorrect}");
            _waitingForChoice = false;
        });
    }

    private void EndDialogue()
    {
        Debug.Log($"[DialogueTrigger] EndDialogue | finalLine: {dialogueData.finalLine != null} | _dialogueCompleted={_dialogueCompleted}");

        // Показываем финальную реплику, если она есть и диалог ещё не завершён
        if (dialogueData.finalLine != null && !_dialogueCompleted)
        {
            Debug.Log($"[DialogueTrigger] Показываем финальную реплику: \"{dialogueData.finalLine.text}\"");
            _waitingForChoice = false;

            if (_waitAndCloseCoroutine != null)
                StopCoroutine(_waitAndCloseCoroutine);

            DialogueUI.Instance.ShowLine(dialogueData.finalLine, null);
            _waitingForClose = true;
            _waitAndCloseCoroutine = StartCoroutine(WaitAndClose());
            return;
        }

        CloseDialogue();
    }

    private IEnumerator WaitAndClose()
    {
        Debug.Log("[DialogueTrigger] WaitAndClose — ждём конца печати");
        yield return new WaitUntil(() => !DialogueUI.Instance.IsTyping);

        Debug.Log("[DialogueTrigger] WaitAndClose — ждём отпускания кнопки");
        yield return new WaitUntil(() => !_playerControls.Player.Interact.IsPressed());

        Debug.Log("[DialogueTrigger] WaitAndClose — ждём нажатия Interact");
        yield return new WaitUntil(() => _playerControls.Player.Interact.WasPressedThisFrame());

        Debug.Log("[DialogueTrigger] WaitAndClose — закрываем");
        _waitAndCloseCoroutine = null;
        CloseDialogue();
    }

    private void CloseDialogue()
    {
        Debug.Log("[DialogueTrigger] CloseDialogue → _dialogueCompleted = true");
        _dialogueActive = false;
        _currentLineIndex = 0;
        _waitingForChoice = false;
        _waitingForClose = false;
        _dialogueCompleted = true;

        if (_waitAndCloseCoroutine != null)
        {
            StopCoroutine(_waitAndCloseCoroutine);
            _waitAndCloseCoroutine = null;
        }

        DialogueUI.Instance.ClosePanel();

        if (interactHint != null && _playerInRange)
            interactHint.SetActive(true);
    }

    public void TriggerDialogue()
    {
        StartDialogue();
    }

    public void ResetDialogue()
    {
        _dialogueActive = false;
        _currentLineIndex = 0;
        _waitingForChoice = false;
        _waitingForClose = false;
        _dialogueCompleted = false;

        if (_waitAndCloseCoroutine != null)
        {
            StopCoroutine(_waitAndCloseCoroutine);
            _waitAndCloseCoroutine = null;
        }

        DialogueUI.Instance.ClosePanel();

        if (interactHint != null && _playerInRange)
            interactHint.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log($"[DialogueTrigger] Игрок вошёл в зону | _dialogueCompleted={_dialogueCompleted}");

        _playerInRange = true;

        if (interactHint != null)
            interactHint.SetActive(true);

        if (triggerOnEnter)
        {
            if (!_dialogueCompleted)
            {
                StartDialogue();
            }
            else
            {
                HandleRepeatInteraction();
            }
        }
    }

        public void SetDialogueData(DialogueData newData)
    {
        dialogueData = newData;
        _dialogueCompleted = false; // чтобы можно было начать заново
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log($"[DialogueTrigger] Игрок вышел из зоны | _dialogueActive={_dialogueActive} | _dialogueCompleted={_dialogueCompleted}");

        _playerInRange = false;

        if (interactHint != null)
            interactHint.SetActive(false);

        // Если диалог активен - он был прерван до завершения
        if (_dialogueActive)
        {
            Debug.Log("[DialogueTrigger] Диалог прерван выходом из зоны");

            _dialogueActive = false;
            _currentLineIndex = 0;
            _waitingForChoice = false;
            _waitingForClose = false;

            // Прерванный диалог не считается завершённым
            // _dialogueCompleted НЕ СБРАСЫВАЕМ, если диалог был прерван - он не завершён,
            // поэтому _dialogueCompleted остаётся false (или true, если был завершён, 
            // но в этом случае _dialogueActive = false, и мы сюда не попадём)

            if (_waitAndCloseCoroutine != null)
            {
                StopCoroutine(_waitAndCloseCoroutine);
                _waitAndCloseCoroutine = null;
            }

            DialogueUI.Instance.ClosePanel();
        }
        // Если диалог не активен, то ничего не сбрасываем
        // Состояние _dialogueCompleted остаётся неизменным
    }
}

public enum DialogueTriggerEndBehaviour
{
    DoNothing,
    RestartDialogue,
    ShowRepeatPrompt,
    DisableTrigger
}