using UnityEngine;

public class MathEnemy : MonoBehaviour
{
    [Header("Данные врага")]
    [SerializeField] private MathEnemyData data;

    [Header("Визуал")]
    [SerializeField] private GameObject enemyVisual;
    [SerializeField] private ParticleSystem deathEffect;

    [Header("HP бар над головой")]
    [SerializeField] private WorldSpaceHpBar hpBar;

    private float _currentHp;
    private bool _isMathSolved;
    private bool _isDead;

    public MathEnemyData Data => data;
    public float CurrentHp => _currentHp;
    public bool IsMathSolved => _isMathSolved;
    public bool IsDead => _isDead;
    public string EnemyName => data != null ? data.enemyName : "???";

    private void Awake()
    {
        if (data == null) { Debug.LogError($"[MathEnemy] data не назначена на {gameObject.name}!"); return; }
        _isMathSolved = false;
        _isDead = false;
        _currentHp = 0f;

        // Показываем первое выражение из данных как HP текст сразу при старте
        if (hpBar != null && data.operations.Count > 0)
            hpBar.SetText(data.operations[0].expression);
    }

    /// <summary>
    /// Вызывается BattleManager когда математика решена.
    /// Устанавливает реальное числовое HP.
    /// </summary>
    public void SetHpAfterMath(float hp)
    {
        _currentHp = hp;
        _isMathSolved = true;
        hpBar?.SetValue(hp, hp);
        Debug.Log($"[MathEnemy] HP установлен: {_currentHp}");
    }

    /// <summary>
    /// Вызывается BattleManager при каждом шаге математики
    /// чтобы обновить текст HP бара над врагом.
    /// </summary>
    public void ShowHpBar() => hpBar?.Show();

    public void SetHpBarText(string text)
    {
        hpBar?.SetText(text);
    }

    public bool TakeDamage(float damage)
    {
        if (!_isMathSolved) { Debug.Log("[MathEnemy] Математика не решена — урон не наносится"); return false; }
        if (_isDead) { return true; }

        _currentHp -= damage;
        hpBar?.SetValue(Mathf.Max(0f, _currentHp), _currentHp + damage); // max = hp до удара
        Debug.Log($"[MathEnemy] Урон {damage} → HP: {_currentHp}");

        if (_currentHp <= 0f) { Die(); return true; }
        return false;
    }

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

    public event System.Action<MathEnemy> OnDied;
}