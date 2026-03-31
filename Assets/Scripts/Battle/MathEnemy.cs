using UnityEngine;

/// <summary>
/// Визуальный компонент врага. Не хранит MathEnemyData —
/// данные передаются снаружи через Init() из BattleSetup.
/// Префаб содержит только визуал, HP бар и эффекты.
/// </summary>
public class MathEnemy : MonoBehaviour
{
    [Header("Визуал")]
    [SerializeField] private GameObject enemyVisual;
    [SerializeField] private ParticleSystem deathEffect;

    [Header("HP бар над головой")]
    [SerializeField] private WorldSpaceHpBar hpBar;

    private MathEnemyData _data;
    private float _currentHp;
    private float _maxHp;
    private bool _isMathSolved;
    private bool _isDead;

    public MathEnemyData Data => _data;
    public float CurrentHp => _currentHp;
    public bool IsMathSolved => _isMathSolved;
    public bool IsDead => _isDead;
    public string EnemyName => _data != null ? _data.enemyName : "???";

    public event System.Action<MathEnemy> OnDied;

    // ---------------------------------------------------------------
    // Инициализация (вызывается из BattleSetup сразу после Instantiate)
    // ---------------------------------------------------------------

    /// <summary>
    /// Передаёт данные боя в экземпляр врага.
    /// Должен вызываться до того как BattleManager.StartBattle() получит этот объект.
    /// </summary>
    public void Init(MathEnemyData data)
    {
        if (data == null)
        {
            Debug.LogError($"[MathEnemy] Init: data == null на {gameObject.name}!");
            return;
        }

        _data = data;
        _isMathSolved = false;
        _isDead = false;
        _currentHp = 0f;

        // Показываем первое выражение как HP-текст сразу при инициализации
        if (hpBar != null && data.operations.Count > 0)
            hpBar.SetText(data.operations[0].expression);
    }

    // ---------------------------------------------------------------
    // Публичный API (вызывается из BattleManager)
    // ---------------------------------------------------------------

    public void ShowHpBar() => hpBar?.Show();

    public void SetHpBarText(string text) => hpBar?.SetText(text);

    /// <summary>Вызывается когда математика решена. Устанавливает числовое HP.</summary>
    public void SetHpAfterMath(float hp)
    {
        _currentHp = hp;
        _maxHp = hp;
        _isMathSolved = true;
        hpBar?.SetValue(hp, hp);
        Debug.Log($"[MathEnemy] HP установлен: {_currentHp}");
    }

    /// <summary>Возвращает true если враг умер.</summary>
    public bool TakeDamage(float damage)
    {
        if (!_isMathSolved) { Debug.Log("[MathEnemy] Математика не решена — урон не наносится"); return false; }
        if (_isDead) return true;

        _currentHp -= damage;
        hpBar?.SetValue(Mathf.Max(0f, _currentHp), _maxHp);   // используем сохранённый _maxHp
        Debug.Log($"[MathEnemy] Урон {damage} → HP: {_currentHp}");

        if (_currentHp <= 0f) { Die(); return true; }
        return false;
    }

    // ---------------------------------------------------------------
    // Приватное
    // ---------------------------------------------------------------

    private void Die()
    {
        _isDead = true;
        Debug.Log($"[MathEnemy] {EnemyName} умер");

        if (deathEffect != null)
        {
            deathEffect.transform.SetParent(null);
            deathEffect.Play();
        }

        OnDied?.Invoke(this);
        Destroy(gameObject);
    }
}