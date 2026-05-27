using UnityEngine;

[CreateAssetMenu(fileName = "TutorialHint", menuName = "Battle/Tutorial Hint Data")]
public class TutorialHintData : ScriptableObject
{
    [Header("Целевая способность")]
    [Tooltip("AbilityId кнопки, на которую указывает стрелка")]
    public string targetAbilityId;

    [Header("Текст подсказки")]
    [TextArea(3, 6)]
    public string message;

    [Header("Позиционирование")]
    public PointerDirection direction = PointerDirection.Top;
    public float offset = 20f;

    [Header("Тайминг")]
    public float showDelay = 0.3f;
}

public enum PointerDirection
{
    Top,
    Bottom,
    Left,
    Right
}