using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BattleSetup : MonoBehaviour
{
    [Header("Враг")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Настройки")]
    [SerializeField] private bool autoDestroyOnWin = true;

    private MathEnemy _spawnedEnemy;
    private PlayerController2D _player;
    private bool _battleStarted;

    private void Start()
    {
        var col = GetComponent<Collider2D>();
        if (!col.isTrigger) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_battleStarted) return;
        if (!other.CompareTag("Player")) return;

        _player = other.GetComponent<PlayerController2D>();
        if (_player == null) return;

        StartBattle();
    }

    private void StartBattle()
    {
        if (enemyPrefab == null) { Debug.LogError("[BattleSetup] enemyPrefab не назначен!"); return; }

        _battleStarted = true;

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        var go = Instantiate(enemyPrefab, pos, Quaternion.identity);
        _spawnedEnemy = go.GetComponent<MathEnemy>();

        if (_spawnedEnemy == null) { Debug.LogError("[BattleSetup] MathEnemy не найден на префабе!"); Destroy(go); return; }

        _spawnedEnemy.OnDied += OnEnemyDied;

        _player.LockMovement();
        BattleManager.Instance.StartBattle(_spawnedEnemy);
    }

    private void OnEnemyDied(MathEnemy enemy)
    {
        enemy.OnDied -= OnEnemyDied;
        BattleManager.Instance.EndBattle();

        if (autoDestroyOnWin)
            Destroy(gameObject);
    }
}