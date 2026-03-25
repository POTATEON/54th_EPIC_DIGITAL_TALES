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
    [Tooltip("Выражение на этом шаге. Для финального шага — это число, оно станет HP врага")]
    public string expression;

    [Tooltip("ID способности которую нужно нажать")]
    public string correctAbilityId;

    [Tooltip("Если true — expression парсится как число и становится HP врага")]
    public bool isFinal;
}