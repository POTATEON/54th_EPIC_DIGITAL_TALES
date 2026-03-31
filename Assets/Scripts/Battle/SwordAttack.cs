using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    [Header("Настройки атаки")]
    [SerializeField] private float damage = 1f;

    [Header("Хитбокс меча")]
    [SerializeField] private GameObject swordHitbox;

    [Header("Поворот меча")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector3 hitboxOffsetRight = new Vector3(0.5f, 0f, 0f);
    [SerializeField] private Vector3 hitboxOffsetLeft = new Vector3(-0.5f, 0f, 0f);
    [SerializeField] private float hitboxRotationRight = 0f;
    [SerializeField] private float hitboxRotationLeft = 180f;

    private Animator _animator;
    private PlayerControls _playerControls;
    private bool _isAttacking;
    private SpriteRenderer _playerSprite;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _playerControls = new PlayerControls();

        if (swordHitbox != null)
            swordHitbox.SetActive(false);

        if (playerTransform == null)
            playerTransform = transform;

        _playerSprite = playerTransform.GetComponent<SpriteRenderer>();
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
        {
            UpdateHitboxDirection();
            swordHitbox.SetActive(true);
        }

        DealDamageImmediate();

        Invoke(nameof(EndAttack), 0.3f);
    }

    private void UpdateHitboxDirection()
    {
        if (swordHitbox == null) return;

        bool facingRight = true;

        if (_playerSprite != null)
            facingRight = !_playerSprite.flipX;
        else
            facingRight = playerTransform.localScale.x > 0;

        // Меняем позицию
        if (facingRight)
        {
            swordHitbox.transform.localPosition = hitboxOffsetRight;
            swordHitbox.transform.localRotation = Quaternion.Euler(0, 0, hitboxRotationRight);
        }
        else
        {
            swordHitbox.transform.localPosition = hitboxOffsetLeft;
            swordHitbox.transform.localRotation = Quaternion.Euler(0, 0, hitboxRotationLeft);
        }

        // Scale НЕ ТРОГАЕМ
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

        for (int i = 0; i < count; i++)
        {
            var enemy = results[i].GetComponent<MathEnemy>();
            if (enemy == null) continue;

            if (!enemy.IsMathSolved)
            {
                Debug.Log("[SwordAttack] Математика не решена — урон не наносится");
                continue;
            }

            bool died = enemy.TakeDamage(damage);

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

        if (died)
            EndAttack();
    }
}