using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class RadioVolume : MonoBehaviour
{
    [Header("Радио")]
    [SerializeField] private AudioSource radioSource;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text volumeText;
    [SerializeField] private CanvasGroup radioPanel;
    
    [Header("NPC диалог")]
    [SerializeField] private DialogueTrigger npcDialogue;
    [SerializeField] private DialogueData solvedDialogue;
    
    [Header("Уравнение")]
    [SerializeField] private string equation = "L = 10·log₁₀(I/I₀)";
    [SerializeField] private float intensity = 1000f;
    [SerializeField] private float correctVolume = 30f;
    
    [Header("Подсказка")]
    [SerializeField] private GameObject interactHint;
    
    private PlayerControls _controls;
    private bool _playerInRange = false;
    private bool _puzzleSolved = false;
    
    private void Awake()
    {
        _controls = new PlayerControls();
        
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            volumeSlider.interactable = false;
            volumeSlider.value = 0;
        }
        
        if (radioSource != null)
            radioSource.volume = 0;
        
        if (radioPanel != null)
            radioPanel.alpha = 0;
    }
    
    private void OnEnable() => _controls.Player.Enable();
    private void OnDisable() => _controls.Player.Disable();
    
    private void Update()
    {
        if (_playerInRange && !_puzzleSolved && _controls.Player.Interact.WasPressedThisFrame())
        {
            ShowCalculator();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;
        if (interactHint != null) interactHint.SetActive(true);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;
        if (interactHint != null) interactHint.SetActive(false);
       if (radioPanel != null)
            radioPanel.alpha = 0;
    }
    
    private void ShowCalculator()
    {
        if (Calculator.Instance == null)
        {
            Debug.LogError("[RadioVolume] Calculator.Instance не найден!");
            return;
        }
        
        // Временно активируем калькулятор
        Calculator.Instance.gameObject.SetActive(true);
        
        string hint = $"Реши уравнение: {equation}\nI/I₀ = {intensity}";
        Calculator.Instance.Show(hint);
        Calculator.Instance.OnSubmit += OnCalculatorSubmit;
    }

    private void OnCalculatorSubmit(string input)
    {
        if (Calculator.Instance != null)
            Calculator.Instance.OnSubmit -= OnCalculatorSubmit;
        
        if (float.TryParse(input, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float result))
        {
            if (Mathf.Abs(result - correctVolume) < 0.1f)
            {
                SolvePuzzle();
            }
            else
            {
                Debug.Log($"Неправильно. Ожидалось {correctVolume}, получено {result}");
            }
        }
    }
    
    private void SolvePuzzle()
    {
        _puzzleSolved = true;
        
        if (volumeSlider != null)
            volumeSlider.interactable = true;
        
        if (radioPanel != null)
        {
            radioPanel.alpha = 1;
            radioPanel.interactable = true;
        }
        
        if (npcDialogue != null && solvedDialogue != null)
        {
            npcDialogue.SetDialogueData(solvedDialogue);
        }
        
        if (interactHint != null)
            interactHint.SetActive(false);
        
        if (npcDialogue != null)
            npcDialogue.TriggerDialogue();
        
        
        Calculator.Instance.gameObject.SetActive(false);

        Debug.Log("Радио настроено! Диалог деда изменён.");
    }
    
    private void OnVolumeChanged(float value)
    {
        float normalizedVolume = value / 30f;
        if (radioSource != null)
            radioSource.volume = Mathf.Clamp01(normalizedVolume);
        
        if (volumeText != null)
            volumeText.text = $"{value:F0} дБ";
    }
}