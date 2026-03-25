using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Синглтон-реестр всех AbilityButton в сцене.
/// Когда новая кнопка регистрируется — стреляет событием OnButtonRegistered.
/// BattleManager подписывается на это событие и сразу добавляет кнопку в слушатели.
/// </summary>
public class AbilityRegistry : MonoBehaviour
{
    public static AbilityRegistry Instance { get; private set; }

    private readonly Dictionary<string, AbilityButton> _buttons = new();

    /// <summary>
    /// Срабатывает каждый раз когда новая AbilityButton появляется в сцене.
    /// BattleManager подписывается сюда чтобы автоматически слушать новые кнопки.
    /// </summary>
    public event System.Action<AbilityButton> OnButtonRegistered;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Register(AbilityButton btn)
    {
        if (string.IsNullOrEmpty(btn.AbilityId))
        {
            Debug.LogWarning($"[AbilityRegistry] Кнопка {btn.gameObject.name} не имеет abilityId!");
            return;
        }

        if (_buttons.ContainsKey(btn.AbilityId))
        {
            Debug.LogWarning($"[AbilityRegistry] Дублирующийся abilityId '{btn.AbilityId}' — пропускаем");
            return;
        }

        _buttons[btn.AbilityId] = btn;
        Debug.Log($"[AbilityRegistry] Зарегистрирована способность '{btn.AbilityId}'");

        // Уведомляем всех подписчиков (в первую очередь BattleManager)
        OnButtonRegistered?.Invoke(btn);
    }

    public void Unregister(AbilityButton btn)
    {
        if (_buttons.Remove(btn.AbilityId))
            Debug.Log($"[AbilityRegistry] Удалена способность '{btn.AbilityId}'");
    }

    public AbilityButton Get(string abilityId)
    {
        _buttons.TryGetValue(abilityId, out var btn);
        return btn;
    }

    public IEnumerable<AbilityButton> All => _buttons.Values;
}