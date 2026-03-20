using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueLine", menuName = "Dialogue/DialogueLine")]
public class DialogueLine : ScriptableObject
{
    [TextArea(2, 5)]
    public string text;
    public string speakerName;
    public Sprite speakerPortrait;

    [Header("Варианты ответа (оставь пустым если выборов нет)")]
    public DialogueChoice[] choices;
}

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public DialogueLine correctResponse;    // реплика если ответ правильный
    public DialogueLine wrongResponse;      // реплика если ответ неправильный
    public bool isCorrect;
}