using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Здоровье")]
    [SerializeField] private float maxHp = 100f;

    [Header("HP бар над головой (скрыт вне боя)")]
    [SerializeField] private WorldSpaceHpBar hpBar;

    private float _currentHp;

    public float CurrentHp => _currentHp;
    public float MaxHp => maxHp;
    public bool IsDead => _currentHp <= 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _currentHp = maxHp;
        // hpBar скрыт — он сам делает SetActive(false) в своём Awake
    }

    public void ShowHpBar()
    {
        hpBar?.Setup(maxHp);
        hpBar?.SetValue(_currentHp, maxHp);
        hpBar?.Show();
    }

    public void HideHpBar() => hpBar?.Hide();

    public void TakeDamage(float damage)
    {
        if (IsDead) return;
        _currentHp = Mathf.Max(0f, _currentHp - damage);
        hpBar?.SetValue(_currentHp, maxHp);
        Debug.Log($"[PlayerHealth] Урон {damage} → HP: {_currentHp}");
        if (IsDead) Debug.Log("[PlayerHealth] Игрок умер");
    }

    public void Heal(float amount)
    {
        _currentHp = Mathf.Min(maxHp, _currentHp + amount);
        hpBar?.SetValue(_currentHp, maxHp);
    }
}