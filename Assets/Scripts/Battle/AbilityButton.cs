using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Вешается на каждую кнопку-способность в Canvas.
/// Регистрирует себя в AbilityRegistry при старте.
/// BattleManager подписывается на OnPressed чтобы проверить правильность выбора.
/// </summary>
public class AbilityButton : MonoBehaviour
{
    [Header("Идентификатор способности")]
    [Tooltip("Уникальное имя способности. Именно это имя пишется в MathOperation.correctAbilityId")]
    [SerializeField] private string abilityId;

    [Header("UI")]
    [SerializeField] private TMP_Text label;
    [SerializeField] private Button button;

    public string AbilityId => abilityId;

    /// <summary>Срабатывает когда игрок нажимает кнопку. Передаёт себя.</summary>
    public event System.Action<AbilityButton> OnPressed;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (label == null)  label  = GetComponentInChildren<TMP_Text>();

        button.onClick.AddListener(() => OnPressed?.Invoke(this));
    }

    private void Start()
    {
        // Регистрируемся в реестре чтобы BattleManager мог найти нас по abilityId
        AbilityRegistry.Instance?.Register(this);
    }

    private void OnDestroy()
    {
        AbilityRegistry.Instance?.Unregister(this);
    }

    public void SetInteractable(bool value) => button.interactable = value;
}
