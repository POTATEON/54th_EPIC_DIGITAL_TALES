using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Battle Panel (скрывается только после смерти врага)")]
    [SerializeField] private GameObject battlePanel;

    [Header("UI внутри панели")]
    [SerializeField] private TMP_Text expressionText;
    [SerializeField] private TMP_Text enemyNameText;

    [Header("Контейнер кнопок-заклинаний (скрывается после математики)")]
    [SerializeField] private GameObject spellButtonsContainer;

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
        if (AbilityRegistry.Instance != null)
        {
            AbilityRegistry.Instance.OnButtonRegistered += OnNewButtonRegistered;
            foreach (var btn in AbilityRegistry.Instance.All)
                SubscribeButton(btn);
        }
        else
        {
            Debug.LogError("[BattleManager] AbilityRegistry не найден!");
        }
    }

    private void OnDestroy()
    {
        if (AbilityRegistry.Instance != null)
            AbilityRegistry.Instance.OnButtonRegistered -= OnNewButtonRegistered;
    }

    // ---------------------------------------------------------------
    // Публичный API
    // ---------------------------------------------------------------

    public void StartBattle(MathEnemy enemy)
    {
        _currentEnemy = enemy;
        _data = enemy.Data;
        _stepIndex = 0;
        _mathSolved = false;
        _battleActive = true;

        battlePanel?.SetActive(true);
        spellButtonsContainer?.SetActive(true);

        // Показываем HP бары только во время боя
        PlayerHealth.Instance?.ShowHpBar();
        enemy.ShowHpBar();

        Debug.Log($"[BattleManager] Бой начат: {_data.enemyName}");

        // Если первый (и единственный) шаг сразу финальный — математики нет,
        // сразу выставляем числовое HP и разрешаем атаку
        if (_data.operations.Count > 0 && _data.operations[0].isFinal)
        {
            var finalStep = _data.operations[0];
            if (!float.TryParse(finalStep.expression, out float hp))
            {
                Debug.LogError($"[BattleManager] isFinal=true на шаге 0, но expression='{finalStep.expression}' не число!");
                return;
            }

            _mathSolved = true;
            _currentEnemy.SetHpAfterMath(hp);
            spellButtonsContainer?.SetActive(false);
            FindFirstObjectByType<PlayerController2D>()?.UnlockMovement();
            Debug.Log($"[BattleManager] Шаг 0 сразу финальный — HP={hp}, математика пропущена.");
            return;
        }

        ShowCurrentStep();
    }

    public void EndBattle()
    {
        _battleActive = false;

        battlePanel?.SetActive(false);

        // Скрываем HP бар игрока — бой закончен
        // HP бар врага исчезнет вместе с его GO (Destroy в MathEnemy.Die)
        PlayerHealth.Instance?.HideHpBar();

        _currentEnemy = null;
        Debug.Log("[BattleManager] Враг убит — панель и HP бары скрыты");
    }

    // ---------------------------------------------------------------
    // Подписка на кнопки
    // ---------------------------------------------------------------

    private void OnNewButtonRegistered(AbilityButton btn) => SubscribeButton(btn);

    private void SubscribeButton(AbilityButton btn)
    {
        btn.OnPressed -= OnAbilityPressed;
        btn.OnPressed += OnAbilityPressed;
    }

    // ---------------------------------------------------------------
    // Логика нажатия
    // ---------------------------------------------------------------

    private void OnAbilityPressed(AbilityButton pressed)
    {
        if (!_battleActive || _mathSolved) return;

        var step = _data.operations[_stepIndex];

        if (pressed.AbilityId == step.correctAbilityId)
            OnCorrectAnswer();
        else
            OnWrongAnswer(pressed, step);
    }

    private void OnCorrectAnswer()
    {
        Debug.Log($"[BattleManager] Правильный ответ на шаге {_stepIndex}");
        _stepIndex++;

        while (_stepIndex < _data.operations.Count)
        {
            var next = _data.operations[_stepIndex];

            if (next.isFinal)
            {
                if (!float.TryParse(next.expression, out float hp))
                {
                    Debug.LogError($"[BattleManager] isFinal=true, но expression='{next.expression}' не является числом!");
                    return;
                }

                _mathSolved = true;
                _currentEnemy.SetHpAfterMath(hp);

                // Обновляем текст выражения в панели
                if (expressionText != null) expressionText.text = next.expression;
                if (enemyNameText != null) enemyNameText.text = _data.enemyName;

                // Скрываем кнопки заклинаний — математика решена
                spellButtonsContainer?.SetActive(false);

                FindFirstObjectByType<PlayerController2D>()?.UnlockMovement();

                Debug.Log($"[BattleManager] Математика решена! HP = {hp}. Заклинания скрыты.");
                return;
            }
            else
            {
                ShowCurrentStep();
                return;
            }
        }

        Debug.LogWarning("[BattleManager] Шаги закончились но isFinal не встретился!");
    }

    private void OnWrongAnswer(AbilityButton pressed, MathOperation step)
    {
        Debug.Log($"[BattleManager] Неправильно! '{pressed.AbilityId}' вместо '{step.correctAbilityId}'");
        PlayerHealth.Instance?.TakeDamage(_data.wrongAnswerDamage);
    }

    private void ShowCurrentStep()
    {
        var step = _data.operations[_stepIndex];

        if (enemyNameText != null) enemyNameText.text = _data.enemyName;
        if (expressionText != null) expressionText.text = step.expression;

        // Синхронизируем HP бар над врагом с текущим выражением
        _currentEnemy?.SetHpBarText(step.expression);

        Debug.Log($"[BattleManager] Шаг {_stepIndex}: '{step.expression}' | кнопка: '{step.correctAbilityId}'");
    }
}