using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Калькулятор с экранной клавиатурой и системой кнопок-способностей (масок ввода).
/// Режим sub/sup: все последующие цифры и точки входят в один индекс.
/// Поддержка нескольких способностей подряд.
/// Поддержка режимов: Normal, Equation, Inequality (определяется по имени сцены).
/// Поддержка многострочного ввода (системы уравнений/неравенств).
/// </summary>
public class Calculator : MonoBehaviour
{
    // ── Дисплей ─────────────────────────────────────────────────────
    [Header("Дисплей")]
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private TMP_Text modeIndicator;

    // ── Способности (маски) ─────────────────────────────────────────
    [Header("Способности (маски)")]
    [SerializeField] private Transform abilitiesContainer;
    [SerializeField] private Button abilityButtonPrefab;
    [SerializeField] private Button nextSlotButton;

    // ── Цифры ───────────────────────────────────────────────────────
    [Header("Цифры")]
    [SerializeField] private Button btn0, btn1, btn2, btn3, btn4;
    [SerializeField] private Button btn5, btn6, btn7, btn8, btn9;

    // ── Точка/запятая ───────────────────────────────────────────────
    [Header("Точка/запятая")]
    [SerializeField] private Button btnDot;

    // ── Операторы ───────────────────────────────────────────────────
    [Header("Операторы")]
    [SerializeField] private Button btnPlus, btnMinus, btnMul, btnDiv;
    [SerializeField] private Button btnLog;
    [SerializeField] private Button btnLParen, btnRParen;

    // ── Индексы ─────────────────────────────────────────────────────
    [Header("Индексы")]
    [SerializeField] private Button btnSub;
    [SerializeField] private Button btnSup;

    // ── Специальные символы ─────────────────────────────────────────
    [Header("Специальные символы")]
    [SerializeField] private Button btnEquals;      // =
    [SerializeField] private Button btnX;           // x (переменная)
    [SerializeField] private Button btnGreater;     // >
    [SerializeField] private Button btnLess;        // <
    [SerializeField] private Button btnGreaterEqual;// ≥
    [SerializeField] private Button btnLessEqual;   // ≤

    // ── Служебные ───────────────────────────────────────────────────
    [Header("Служебные")]
    [SerializeField] private Button btnBackspace;
    [SerializeField] private Button btnClear;
    [SerializeField] private Button btnApply;

    // ── Системы уравнений ───────────────────────────────────────────
    [Header("Системы уравнений")]
    [SerializeField] private Button btnAddLine;

    // ── Цвета ───────────────────────────────────────────────────────
    [Header("Цвета режимов")]
    [SerializeField] private Color subActiveColor = new Color(0.30f, 0.60f, 1.00f);
    [SerializeField] private Color supActiveColor = new Color(1.00f, 0.60f, 0.20f);
    [SerializeField] private Color defaultBtnColor = Color.white;

    // ── Режимы калькулятора ─────────────────────────────────────────
    public enum CalculatorMode
    {
        Normal,      // обычный режим (только цифры, операции, log)
        Equation,    // уравнения (показывает = и x)
        Inequality   // неравенства (показывает =, x, >, <, ≥, ≤)
    }
    public static Calculator Instance { get; private set; }

    // ── Событие ─────────────────────────────────────────────────────
    public event System.Action<string> OnSubmit;

    // ── Многострочный ввод ─────────────────────────────────────────
    private List<StringBuilder> _lines = new List<StringBuilder> { new StringBuilder() };
    private int _currentLine = 0;

    private enum IndexMode { None, Sub, Sup }
    private IndexMode _mode = IndexMode.None;
    private bool _inIndex = false;
    private CalculatorMode _currentMode = CalculatorMode.Normal;

    // ── Режим маски ─────────────────────────────────────────────────
    private struct Segment
    {
        public bool isSlot;
        public string fixedText;
        public bool isSubSlot;
        public bool isSupSlot;
        public StringBuilder slotValue;
    }

    private List<Segment> _segments;
    private int _currentSlot = -1;
    private bool _maskActive = false;
    private int _filledSlotsCount = 0;

    private readonly List<Button> _spawnedAbilityButtons = new();

    // ════════════════════════════════════════════════════════════════
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        BindAll();
        SetModeByScene();
        if (abilityButtonPrefab != null)
            abilityButtonPrefab.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;
        if (_maskActive) return; // в режиме маски не переключаем строки

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Навигация стрелками между строками
        if (keyboard.upArrowKey.wasPressedThisFrame && _currentLine > 0)
        {
            _currentLine--;
            RefreshDisplay();
        }
        else if (keyboard.downArrowKey.wasPressedThisFrame && _currentLine < _lines.Count - 1)
        {
            _currentLine++;
            RefreshDisplay();
        }
    }

    // ════════════════════════════════════════════════════════════════
    // ОПРЕДЕЛЕНИЕ РЕЖИМА ПО ИМЕНИ СЦЕНЫ
    // ════════════════════════════════════════════════════════════════

    private void SetModeByScene()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (sceneName.Contains("Equations") || sceneName.Contains("уравнение") || sceneName.Contains("Equation"))
        {
            SetMode(CalculatorMode.Equation);
            Debug.Log("[Calculator] Режим: Уравнения");
        }
        else if (sceneName.Contains("Inequalities") || sceneName.Contains("неравенство") || sceneName.Contains("Inequality"))
        {
            SetMode(CalculatorMode.Inequality);
            Debug.Log("[Calculator] Режим: Неравенства");
        }
        else
        {
            SetMode(CalculatorMode.Normal);
            Debug.Log("[Calculator] Режим: Обычный");
        }
    }

    // ════════════════════════════════════════════════════════════════
    // ПУБЛИЧНЫЙ API
    // ════════════════════════════════════════════════════════════════

    public void Show(string hint = "", List<AbilityTemplate> abilities = null)
    {
        gameObject.SetActive(true);
        ExitMask();
        ClearFree();

        if (hintText != null)
        {
            hintText.text = hint;
            hintText.gameObject.SetActive(!string.IsNullOrEmpty(hint));
        }

        SpawnAbilityButtons(abilities);
        RefreshDisplay();
    }

    public void Hide()
    {
        ClearAbilityButtons();
        gameObject.SetActive(false);
    }

    public void SetInteractable(bool value)
    {
        foreach (var btn in AllButtons())
            if (btn != null) btn.interactable = value;
        foreach (var btn in _spawnedAbilityButtons)
            if (btn != null) btn.interactable = value;
        if (nextSlotButton != null)
            nextSlotButton.interactable = value;
        if (btnAddLine != null)
            btnAddLine.interactable = value;
    }

    public void Clear()
    {
        ExitMask();
        ClearFree();
        RefreshDisplay();
    }

    public void SetMode(CalculatorMode mode)
    {
        _currentMode = mode;

        if (btnEquals != null)
            btnEquals.gameObject.SetActive(mode != CalculatorMode.Normal);

        if (btnX != null)
            btnX.gameObject.SetActive(mode == CalculatorMode.Equation || mode == CalculatorMode.Inequality);

        if (btnGreater != null)
            btnGreater.gameObject.SetActive(mode == CalculatorMode.Inequality);

        if (btnLess != null)
            btnLess.gameObject.SetActive(mode == CalculatorMode.Inequality);

        if (btnGreaterEqual != null)
            btnGreaterEqual.gameObject.SetActive(mode == CalculatorMode.Inequality);

        if (btnLessEqual != null)
            btnLessEqual.gameObject.SetActive(mode == CalculatorMode.Inequality);
    }

    public void AddNewLine()
    {
        if (_maskActive) return;

        if (_lines.Count > 0 && _lines[_lines.Count - 1].Length == 0)
            return;

        _lines.Add(new StringBuilder());
        _currentLine = _lines.Count - 1;
        RefreshDisplay();
    }

    // ════════════════════════════════════════════════════════════════
    // КНОПКИ СПОСОБНОСТЕЙ
    // ════════════════════════════════════════════════════════════════

    private void SpawnAbilityButtons(List<AbilityTemplate> abilities)
    {
        ClearAbilityButtons();

        bool hasAbilities = abilities != null && abilities.Count > 0;
        if (abilitiesContainer != null)
            abilitiesContainer.gameObject.SetActive(hasAbilities);

        if (!hasAbilities || abilityButtonPrefab == null) return;

        foreach (var template in abilities)
        {
            if (template == null) continue;

            var go = Instantiate(abilityButtonPrefab.gameObject, abilitiesContainer);
            go.SetActive(true);

            var btn = go.GetComponent<Button>();
            var label = go.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = template.label;

            var t = template;
            btn.onClick.AddListener(() => ActivateMask(t));

            _spawnedAbilityButtons.Add(btn);
        }
    }

    private void ClearAbilityButtons()
    {
        foreach (var btn in _spawnedAbilityButtons)
            if (btn != null) Destroy(btn.gameObject);
        _spawnedAbilityButtons.Clear();
    }

    // ════════════════════════════════════════════════════════════════
    // РЕЖИМ МАСКИ
    // ════════════════════════════════════════════════════════════════

    private void ActivateMask(AbilityTemplate template)
    {
        ExitMask();
        _segments = ParseTemplate(template.template);
        _maskActive = true;
        _currentSlot = FindNextSlot(-1);
        UpdateFilledSlotsCount();

        if (nextSlotButton != null)
            nextSlotButton.gameObject.SetActive(true);

        UpdateNextSlotButtonText();
        SetIndexMode(IndexMode.None);
        RefreshDisplay();
    }

    private void ExitMask()
    {
        _maskActive = false;
        _segments = null;
        _currentSlot = -1;
        if (nextSlotButton != null)
            nextSlotButton.gameObject.SetActive(false);
    }

    private static List<Segment> ParseTemplate(string template)
    {
        var result = new List<Segment>();
        if (string.IsNullOrEmpty(template)) return result;

        var buf = new StringBuilder();

        for (int i = 0; i < template.Length; i++)
        {
            char c = template[i];

            if (c == '_')
            {
                if (buf.Length > 0)
                {
                    result.Add(new Segment { isSlot = false, fixedText = buf.ToString() });
                    buf.Clear();
                }
                result.Add(new Segment
                {
                    isSlot = true,
                    isSubSlot = false,
                    isSupSlot = false,
                    slotValue = new StringBuilder()
                });
            }
            else if (c == '^')
            {
                if (buf.Length > 0)
                {
                    result.Add(new Segment { isSlot = false, fixedText = buf.ToString() });
                    buf.Clear();
                }
                result.Add(new Segment
                {
                    isSlot = true,
                    isSubSlot = false,
                    isSupSlot = false,
                    slotValue = new StringBuilder()
                });
            }
            else
            {
                buf.Append(c);
            }
        }

        if (buf.Length > 0)
            result.Add(new Segment { isSlot = false, fixedText = buf.ToString() });

        return result;
    }

    private int FindNextSlot(int fromIndex)
    {
        if (_segments == null) return -1;
        for (int i = fromIndex + 1; i < _segments.Count; i++)
            if (_segments[i].isSlot) return i;
        return -1;
    }

    private int FindPrevSlot(int fromIndex)
    {
        if (_segments == null) return -1;
        for (int i = fromIndex - 1; i >= 0; i--)
            if (_segments[i].isSlot) return i;
        return -1;
    }

    private int SlotCount()
    {
        int n = 0;
        if (_segments == null) return 0;
        foreach (var s in _segments) if (s.isSlot) n++;
        return n;
    }

    private void MoveToNextSlot()
    {
        if (_segments == null) return;

        int next = FindNextSlot(_currentSlot);

        if (next >= 0)
        {
            _currentSlot = next;
            RefreshDisplay();
        }
        else
        {
            string currentMaskResult = BuildMaskResult();
            ExitMask();

            if (!string.IsNullOrEmpty(currentMaskResult))
            {
                _lines[_currentLine].Append(currentMaskResult);
            }

            RefreshDisplay();
        }
    }

    private void UpdateFilledSlotsCount()
    {
        if (_segments == null)
        {
            _filledSlotsCount = 0;
            return;
        }

        _filledSlotsCount = 0;
        foreach (var seg in _segments)
        {
            if (seg.isSlot && seg.slotValue.Length > 0)
                _filledSlotsCount++;
        }
    }

    private void UpdateNextSlotButtonText()
    {
        if (nextSlotButton == null) return;

        var label = nextSlotButton.GetComponentInChildren<TMP_Text>();
        if (label == null) return;

        int totalSlots = SlotCount();
        if (_filledSlotsCount >= totalSlots && totalSlots > 0)
        {
            label.text = "✓";
        }
        else
        {
            label.text = "→";
        }
    }

    private string BuildMaskResult()
    {
        var sb = new StringBuilder();
        if (_segments == null) return string.Empty;

        foreach (var seg in _segments)
        {
            if (!seg.isSlot)
            {
                sb.Append(seg.fixedText);
            }
            else
            {
                sb.Append(seg.slotValue);
            }
        }
        return sb.ToString();
    }

    // ════════════════════════════════════════════════════════════════
    // ЛОГИКА ВВОДА
    // ════════════════════════════════════════════════════════════════

    private void AppendValue(string value)
    {
        bool isDigit = char.IsDigit(value[0]);
        bool isDot = (value == ",");
        bool isParen = (value == "(" || value == ")");
        bool keepsIndex = isDigit || isDot;

        if (!keepsIndex || isParen)
        {
            if (_inIndex) _inIndex = false;
        }

        if (isParen && _mode != IndexMode.None)
        {
            SetIndexMode(IndexMode.None);
            _inIndex = false;
        }

        if (_maskActive && _currentSlot >= 0)
        {
            var seg = _segments[_currentSlot];

            if (_mode == IndexMode.Sub)
            {
                if (!_inIndex)
                {
                    seg.slotValue.Append('_');
                    _inIndex = true;
                }
                seg.slotValue.Append(value);
            }
            else if (_mode == IndexMode.Sup)
            {
                if (!_inIndex)
                {
                    seg.slotValue.Append('^');
                    _inIndex = true;
                }
                seg.slotValue.Append(value);
            }
            else
            {
                seg.slotValue.Append(value);
            }

            UpdateFilledSlotsCount();
            UpdateNextSlotButtonText();
        }
        else
        {
            var currentLineBuilder = _lines[_currentLine];

            if (_mode == IndexMode.Sub)
            {
                if (!_inIndex)
                {
                    currentLineBuilder.Append('_');
                    _inIndex = true;
                }
                currentLineBuilder.Append(value);
            }
            else if (_mode == IndexMode.Sup)
            {
                if (!_inIndex)
                {
                    currentLineBuilder.Append('^');
                    _inIndex = true;
                }
                currentLineBuilder.Append(value);
            }
            else
            {
                currentLineBuilder.Append(value);
            }
        }

        RefreshDisplay();
    }

    private void Backspace()
    {
        if (_maskActive && _currentSlot >= 0)
        {
            var val = _segments[_currentSlot].slotValue;
            if (val.Length == 0)
            {
                int prev = FindPrevSlot(_currentSlot);
                if (prev >= 0) _currentSlot = prev;
            }
            else if (val.Length >= 2 && (val[val.Length - 2] == '_' || val[val.Length - 2] == '^'))
            {
                val.Remove(val.Length - 2, 2);
                _inIndex = false;
            }
            else
            {
                val.Remove(val.Length - 1, 1);
            }

            UpdateFilledSlotsCount();
            UpdateNextSlotButtonText();
            RefreshDisplay();
            return;
        }

        var currentLineBuilder = _lines[_currentLine];
        if (currentLineBuilder.Length == 0) return;

        if (currentLineBuilder.Length >= 2 &&
            (currentLineBuilder[currentLineBuilder.Length - 2] == '_' ||
             currentLineBuilder[currentLineBuilder.Length - 2] == '^'))
        {
            currentLineBuilder.Remove(currentLineBuilder.Length - 2, 2);
            _inIndex = false;
        }
        else if (currentLineBuilder.Length >= 3 &&
                 currentLineBuilder.ToString(currentLineBuilder.Length - 3, 3) == "log")
        {
            currentLineBuilder.Remove(currentLineBuilder.Length - 3, 3);
        }
        else
        {
            currentLineBuilder.Remove(currentLineBuilder.Length - 1, 1);
        }

        if (_lines[_currentLine].Length == 0 && _lines.Count > 1)
        {
            _lines.RemoveAt(_currentLine);
            if (_currentLine >= _lines.Count)
                _currentLine = _lines.Count - 1;
        }

        RefreshDisplay();
    }

    private void ClearFree()
    {
        _lines.Clear();
        _lines.Add(new StringBuilder());
        _currentLine = 0;
        _inIndex = false;
        SetIndexMode(IndexMode.None);
    }

    private void Submit()
    {
        string result;

        if (_maskActive)
        {
            result = BuildMaskResult();
            ExitMask();
            ClearFree();
        }
        else
        {
            result = string.Join(";", _lines.Select(l => l.ToString()));
            if (string.IsNullOrEmpty(result)) return;
            ClearFree();
        }

        result = NormalizeForBattle(result);
        RefreshDisplay();
        OnSubmit?.Invoke(result);
    }

    private string NormalizeForBattle(string input)
    {
        input = input.Replace(" ", "");
        input = input.Replace("log_10", "log₁₀");
        input = input.Replace("log10", "log₁₀");
        input = input.Replace("log_2", "log₂");
        input = input.Replace("log2", "log₂");
        input = input.Replace("log_3", "log₃");
        input = input.Replace("log3", "log₃");
        input = input.Replace("log_5", "log₅");
        input = input.Replace("log5", "log₅");
        input = input.Replace("log_e", "ln");
        return input;
    }

    // ════════════════════════════════════════════════════════════════
    // SUB / SUP РЕЖИМ
    // ════════════════════════════════════════════════════════════════

    private void ToggleSub()
    {
        if (_mode == IndexMode.Sub)
        {
            SetIndexMode(IndexMode.None);
            _inIndex = false;
        }
        else
        {
            SetIndexMode(IndexMode.Sub);
            _inIndex = false;
        }
    }

    private void ToggleSup()
    {
        if (_mode == IndexMode.Sup)
        {
            SetIndexMode(IndexMode.None);
            _inIndex = false;
        }
        else
        {
            SetIndexMode(IndexMode.Sup);
            _inIndex = false;
        }
    }

    private void SetIndexMode(IndexMode mode)
    {
        _mode = mode;
        if (_mode == IndexMode.None)
            _inIndex = false;

        SetButtonTint(btnSub, defaultBtnColor);
        SetButtonTint(btnSup, defaultBtnColor);

        switch (_mode)
        {
            case IndexMode.Sub:
                SetButtonTint(btnSub, subActiveColor);
                ShowModeIndicator("sub");
                break;
            case IndexMode.Sup:
                SetButtonTint(btnSup, supActiveColor);
                ShowModeIndicator("sup");
                break;
            default:
                if (modeIndicator != null) modeIndicator.gameObject.SetActive(false);
                break;
        }
    }

    private void ShowModeIndicator(string text)
    {
        if (modeIndicator == null) return;
        modeIndicator.text = text;
        modeIndicator.gameObject.SetActive(true);
    }

    private static void SetButtonTint(Button btn, Color color)
    {
        if (btn == null) return;
        var c = btn.colors;
        c.normalColor = color;
        btn.colors = c;
    }

    // ════════════════════════════════════════════════════════════════
    // ОТОБРАЖЕНИЕ
    // ════════════════════════════════════════════════════════════════

    private void RefreshDisplay()
    {
        if (displayText == null) return;

        if (_maskActive)
        {
            displayText.text = BuildMaskDisplay();
            return;
        }

        var sb = new StringBuilder();
        for (int i = 0; i < _lines.Count; i++)
        {
            string rawContent = _lines[i].ToString();
            string formattedContent = string.IsNullOrEmpty(rawContent) ? "□" : RenderRaw(rawContent);

            if (i == _currentLine)
                sb.Append("> ").Append(formattedContent);
            else
                sb.Append("  ").Append(formattedContent);

            if (i != _lines.Count - 1)
                sb.AppendLine();
        }

        displayText.text = sb.ToString();
    }

    private string BuildMaskDisplay()
    {
        var sb = new StringBuilder();
        if (_segments == null) return string.Empty;

        for (int i = 0; i < _segments.Count; i++)
        {
            var seg = _segments[i];
            if (!seg.isSlot)
            {
                sb.Append(RenderRaw(seg.fixedText));
                continue;
            }

            bool isCurrent = (i == _currentSlot);
            string slotContent;

            if (seg.slotValue.Length > 0)
            {
                slotContent = RenderRaw(seg.slotValue.ToString());
            }
            else
            {
                slotContent = isCurrent ? "<color=#888888>□</color>" : "<color=#AAAAAA>□</color>";
            }

            sb.Append(slotContent);

            if (isCurrent)
                sb.Append("<color=#888888>|</color>");
        }

        return sb.ToString();
    }

    private static string RenderRaw(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;

        var sb = new StringBuilder();
        int i = 0;
        while (i < raw.Length)
        {
            if (raw[i] == '_' && i + 1 < raw.Length)
            {
                sb.Append("<sub>");
                i++;
                while (i < raw.Length && raw[i] != '_' && raw[i] != '^')
                {
                    sb.Append(raw[i]);
                    i++;
                }
                sb.Append("</sub>");
            }
            else if (raw[i] == '^' && i + 1 < raw.Length)
            {
                sb.Append("<sup>");
                i++;
                while (i < raw.Length && raw[i] != '_' && raw[i] != '^')
                {
                    sb.Append(raw[i]);
                    i++;
                }
                sb.Append("</sup>");
            }
            else
            {
                sb.Append(raw[i]);
                i++;
            }
        }
        return sb.ToString();
    }

    // ════════════════════════════════════════════════════════════════
    // ПРИВЯЗКА КНОПОК
    // ════════════════════════════════════════════════════════════════

    private void BindAll()
    {
        Bind(btn0, "0"); Bind(btn1, "1"); Bind(btn2, "2");
        Bind(btn3, "3"); Bind(btn4, "4"); Bind(btn5, "5");
        Bind(btn6, "6"); Bind(btn7, "7"); Bind(btn8, "8");
        Bind(btn9, "9");

        if (btnDot != null)
            btnDot.onClick.AddListener(() => AppendValue(","));

        Bind(btnPlus, "+"); Bind(btnMinus, "-");
        Bind(btnMul, "*"); Bind(btnDiv, "/");
        Bind(btnLog, "log");
        Bind(btnLParen, "("); Bind(btnRParen, ")");

        btnSub?.onClick.AddListener(ToggleSub);
        btnSup?.onClick.AddListener(ToggleSup);

        if (btnEquals != null)
            btnEquals.onClick.AddListener(() => AppendValue("="));
        if (btnX != null)
            btnX.onClick.AddListener(() => AppendValue("x"));
        if (btnGreater != null)
            btnGreater.onClick.AddListener(() => AppendValue(">"));
        if (btnLess != null)
            btnLess.onClick.AddListener(() => AppendValue("<"));
        if (btnGreaterEqual != null)
            btnGreaterEqual.onClick.AddListener(() => AppendValue("≥"));
        if (btnLessEqual != null)
            btnLessEqual.onClick.AddListener(() => AppendValue("≤"));

        btnBackspace?.onClick.AddListener(Backspace);
        btnClear?.onClick.AddListener(() => { ExitMask(); ClearFree(); RefreshDisplay(); });
        btnApply?.onClick.AddListener(Submit);

        if (btnAddLine != null)
            btnAddLine.onClick.AddListener(AddNewLine);

        if (nextSlotButton != null)
        {
            nextSlotButton.onClick.AddListener(MoveToNextSlot);
            nextSlotButton.gameObject.SetActive(false);
        }
    }

    private void Bind(Button btn, string value)
    {
        if (btn == null) return;
        btn.onClick.AddListener(() => AppendValue(value));
    }

    private IEnumerable<Button> AllButtons()
    {
        yield return btn0; yield return btn1; yield return btn2;
        yield return btn3; yield return btn4; yield return btn5;
        yield return btn6; yield return btn7; yield return btn8; yield return btn9;
        if (btnDot != null) yield return btnDot;
        yield return btnPlus; yield return btnMinus;
        yield return btnMul; yield return btnDiv;
        yield return btnLog; yield return btnLParen; yield return btnRParen;
        yield return btnSub; yield return btnSup;
        yield return btnEquals; yield return btnX;
        yield return btnGreater; yield return btnLess;
        yield return btnGreaterEqual; yield return btnLessEqual;
        yield return btnBackspace; yield return btnClear; yield return btnApply;
        if (btnAddLine != null) yield return btnAddLine;
    }
}