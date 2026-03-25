using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    [Header("Настройки атаки")]
    [SerializeField] private float damage = 1f;

    [Header("Хитбокс меча")]
    [SerializeField] private GameObject swordHitbox;

    private Animator _animator;
    private PlayerControls _playerControls;
    private bool _isAttacking;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _playerControls = new PlayerControls();

        if (swordHitbox != null)
            swordHitbox.SetActive(false);
    }

    private void OnEnable() => _playerControls.Player.Enable();
    private void OnDisable() => _playerControls.Player.Disable();

    private void Update()
    {
        if (_playerControls.Player.Attack.WasPressedThisFrame())
            TriggerAttack();
    }

    private void TriggerAttack()
    {
        if (_isAttacking) return;

        _isAttacking = true;

        if (_animator != null)
            _animator.SetTrigger("Attack");

        if (swordHitbox != null)
            swordHitbox.SetActive(true);

        DealDamageImmediate();

        Invoke(nameof(EndAttack), 0.3f);
    }

    private void DealDamageImmediate()
    {
        if (swordHitbox == null) return;

        var hitboxCollider = swordHitbox.GetComponent<Collider2D>();
        if (hitboxCollider == null)
        {
            Debug.LogError("[SwordAttack] На swordHitbox нет Collider2D!");
            return;
        }

        var results = new Collider2D[10];
        int count = hitboxCollider.Overlap(new ContactFilter2D().NoFilter(), results);

        Debug.Log($"[SwordAttack] DealDamageImmediate: найдено коллайдеров = {count}");

        for (int i = 0; i < count; i++)
        {
            var enemy = results[i].GetComponent<MathEnemy>();
            if (enemy == null) continue;

            if (!enemy.IsMathSolved)
            {
                Debug.Log("[SwordAttack] Математика не решена — урон не наносится");
                continue;
            }

            // HP бар обновляется внутри MathEnemy.TakeDamage → WorldSpaceHpBar
            bool died = enemy.TakeDamage(damage);
            Debug.Log($"[SwordAttack] Удар! Урон={damage}, HP врага={enemy.CurrentHp}");

            if (died)
            {
                EndAttack();
                return;
            }
        }
    }

    public void EndAttack()
    {
        _isAttacking = false;
        if (swordHitbox != null)
            swordHitbox.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isAttacking) return;

        var enemy = other.GetComponent<MathEnemy>();
        if (enemy == null || !enemy.IsMathSolved) return;

        bool died = enemy.TakeDamage(damage);
        Debug.Log($"[SwordAttack] Удар (Enter)! Урон={damage}, HP врага={enemy.CurrentHp}");

        if (died)
            EndAttack();
    }
}