using System;
using UnityEngine;
using TMPro;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Battle Panel (скрывается только после смерти врага)")]
    [SerializeField] private GameObject battlePanel;

    [Header("UI внутри панели")]
    [SerializeField] private TMP_Text expressionText;
    [SerializeField] private TMP_Text enemyNameText;

    [Header("Калькулятор")]
    [SerializeField] private Calculator calculator;

    // Кэшированная ссылка на игрока — устанавливается из BattleSetup через SetPlayer()
    private PlayerController2D _player;

    private MathEnemy _currentEnemy;
    private MathEnemyData _data;
    private int _stepIndex;
    private bool _mathSolved;
    private bool _battleActive;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        battlePanel?.SetActive(false);
    }

    private void Start()
    {
        if (calculator != null)
            calculator.OnSubmit += OnCalculatorSubmit;
        else
            Debug.LogError("[BattleManager] Calculator не назначен!");
    }

    private void OnDestroy()
    {
        if (calculator != null)
            calculator.OnSubmit -= OnCalculatorSubmit;
    }

    // ---------------------------------------------------------------
    // Публичный API
    // ---------------------------------------------------------------

    /// <summary>Вызывается из BattleSetup чтобы передать ссылку на игрока.</summary>
    public void SetPlayer(PlayerController2D player) => _player = player;

    public void StartBattle(MathEnemy enemy)
    {
        _currentEnemy = enemy;
        _data = enemy.Data;
        _stepIndex = 0;
        _mathSolved = false;
        _battleActive = true;

        battlePanel?.SetActive(true);

        PlayerHealth.Instance?.ShowHpBar();
        enemy.ShowHpBar();

        Debug.Log($"[BattleManager] Бой начат: {_data.enemyName}");

        // Если первый шаг сразу финальный — математики нет
        if (_data.operations.Count > 0 && _data.operations[0].isFinal)
        {
            ResolveFinalStep(_data.operations[0]);
            return;
        }

        ShowCurrentStep();
    }

    public void EndBattle()
    {
        _battleActive = false;
        calculator?.Hide();
        battlePanel?.SetActive(false);
        PlayerHealth.Instance?.HideHpBar();
        _currentEnemy = null;
        Debug.Log("[BattleManager] Враг убит — панель скрыта");
    }

    // ---------------------------------------------------------------
    // Обработка ввода из калькулятора
    // ---------------------------------------------------------------

    private void OnCalculatorSubmit(string rawInput)
    {
        if (!_battleActive || _mathSolved) return;

        var step = _data.operations[_stepIndex];
        string normalized = NormalizeInput(rawInput);

        if (IsCorrectInput(normalized, step))
        {
            OnCorrectAnswer();
        }
        else
        {
            OnWrongAnswer(rawInput, step);
        }
    }

    /// <summary>
    /// Приводит ввод к единому формату для сравнения.
    /// Правила:
    ///   • нижний регистр
    ///   • убираем пробелы
    ///   • заменяем unicode-подстрочные цифры (₀–₉) на _X
    ///   • заменяем unicode-надстрочные цифры (⁰–⁹, ²³) на ^X
    /// Это позволяет сравнивать "log_28" и "log₂8" как одно и то же.
    /// </summary>
    private static string NormalizeInput(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        var sb = new System.Text.StringBuilder();
        string lower = input.ToLowerInvariant().Replace(" ", "");

        foreach (char c in lower)
        {
            // Unicode subscript digits ₀₁₂₃₄₅₆₇₈₉
            if (c >= '₀' && c <= '₉')
            {
                sb.Append('_');
                sb.Append((char)('0' + (c - '₀')));
                continue;
            }
            // Unicode superscript digits ⁰¹²³⁴⁵⁶⁷⁸⁹
            switch (c)
            {
                case '⁰': sb.Append("^0"); continue;
                case '¹': sb.Append("^1"); continue;
                case '²': sb.Append("^2"); continue;
                case '³': sb.Append("^3"); continue;
                case '⁴': sb.Append("^4"); continue;
                case '⁵': sb.Append("^5"); continue;
                case '⁶': sb.Append("^6"); continue;
                case '⁷': sb.Append("^7"); continue;
                case '⁸': sb.Append("^8"); continue;
                case '⁹': sb.Append("^9"); continue;
            }
            sb.Append(c);
        }

        return sb.ToString();
    }

    private static bool IsCorrectInput(string normalizedInput, MathOperation step)
    {
        if (step.expectedInputs == null || step.expectedInputs.Length == 0)
        {
            Debug.LogWarning("[BattleManager] У шага нет expectedInputs!");
            return false;
        }

        foreach (var expected in step.expectedInputs)
        {
            if (NormalizeInput(expected) == normalizedInput)
                return true;
        }

        return false;
    }

    // ---------------------------------------------------------------
    // Логика правильного / неправильного ответа
    // ---------------------------------------------------------------

    private void OnCorrectAnswer()
    {
        Debug.Log($"[BattleManager] Правильный ответ на шаге {_stepIndex}");

        // Блокируем ввод на момент перехода
        calculator?.SetInteractable(false);

        _stepIndex++;

        while (_stepIndex < _data.operations.Count)
        {
            var next = _data.operations[_stepIndex];

            if (next.isFinal)
            {
                ResolveFinalStep(next);
                return;
            }
            else
            {
                ShowCurrentStep();
                return;
            }
        }

        Debug.LogWarning("[BattleManager] Шаги закончились, но isFinal не встретился!");
    }

    private void OnWrongAnswer(string input, MathOperation step)
    {
        Debug.Log($"[BattleManager] Неправильно! Введено: '{input}'");
        PlayerHealth.Instance?.TakeDamage(_data.wrongAnswerDamage);

        // Очищаем поле, фокус остаётся
        calculator?.Clear();
    }

    // ---------------------------------------------------------------
    // Вспомогательные методы
    // ---------------------------------------------------------------

    private void ShowCurrentStep()
    {
        var step = _data.operations[_stepIndex];

        if (enemyNameText != null) enemyNameText.text = _data.enemyName;
        if (expressionText != null) expressionText.text = step.expression;

        _currentEnemy?.SetHpBarText(step.expression);

        // Показываем калькулятор с подсказкой-выражением
        calculator?.Show(step.expression, step.abilities);
        calculator?.SetInteractable(true);

        Debug.Log($"[BattleManager] Шаг {_stepIndex}: '{step.expression}'");
    }

    private void ResolveFinalStep(MathOperation finalStep)
    {
        if (!float.TryParse(finalStep.expression, out float hp))
        {
            Debug.LogError($"[BattleManager] isFinal=true, но expression='{finalStep.expression}' не является числом!");
            return;
        }

        _mathSolved = true;
        _currentEnemy.SetHpAfterMath(hp);

        if (expressionText != null) expressionText.text = finalStep.expression;
        if (enemyNameText != null) enemyNameText.text = _data.enemyName;

        calculator?.Hide();

        _player?.UnlockMovement();

        Debug.Log($"[BattleManager] Математика решена! HP = {hp}. Калькулятор скрыт.");
    }
}