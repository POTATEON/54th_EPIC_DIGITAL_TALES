using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueChoiceButton : MonoBehaviour
{
    [SerializeField] private TMP_Text choiceText;
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void Setup(string text, System.Action onClickCallback)
    {
        choiceText.text = text;
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => onClickCallback());
    }
}