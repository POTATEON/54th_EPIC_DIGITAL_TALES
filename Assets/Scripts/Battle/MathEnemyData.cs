using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMathEnemy", menuName = "Battle/MathEnemyData")]
public class MathEnemyData : ScriptableObject
{
    [Header("Основное")]
    public string enemyName = "Враг";

    [Header("Урон врага при ошибке игрока")]
    public float wrongAnswerDamage = 10f;

    [Header("Шаги решения")]
    public List<MathOperation> operations = new();
}

[System.Serializable]
public class MathOperation
{
    [Tooltip("Выражение, которое видно над врагом на этом шаге")]
    public string expression;

    [Tooltip("Допустимые варианты ввода (регистр и пробелы игнорируются)")]
    public string[] expectedInputs;

    [Tooltip("Если true — expression парсится как число и становится HP врага")]
    public bool isFinal;

    [Tooltip("Кнопки-способности (маски ввода) доступные на этом шаге")]
    public List<AbilityTemplate> abilities = new();

    // ===== НОВЫЕ ПОЛЯ ДЛЯ СИСТЕМ =====
    [Tooltip("Если true — ответом является система уравнений/неравенств")]
    public bool isSystem;

    [Tooltip("Ожидаемые уравнения/неравенства системы (каждое отдельно)")]
    public string[] expectedSystemInputs;
}