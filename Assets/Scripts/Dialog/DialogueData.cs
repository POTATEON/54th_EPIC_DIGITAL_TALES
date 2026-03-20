using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueData", menuName = "Dialogue/DialogueData")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;

    [Header("Конечная реплика")]
    [Tooltip("Если указана — диалог считается завершённым когда игрок её видит")]
    public DialogueLine finalLine;

    [Header("Повторное взаимодействие")]
    public DialogueLine repeatPromptLine;
    public string repeatChoiceText = "Повторить";
    public string skipChoiceText = "Всё понял, спасибо";
}