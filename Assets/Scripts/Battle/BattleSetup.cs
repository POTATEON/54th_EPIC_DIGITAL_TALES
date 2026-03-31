using UnityEngine;

/// <summary>
/// Триггер боя. Размещается в сцене — у каждого триггера свои данные врага.
/// Префаб врага не содержит MathEnemyData: данные передаются через MathEnemy.Init().
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BattleSetup : MonoBehaviour
{
    [Header("Префаб врага (только визуал — без данных)")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Данные боя для этого триггера")]
    [SerializeField] private MathEnemyData battleData;

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
        if (battleData == null) { Debug.LogError("[BattleSetup] battleData не назначена!"); return; }

        _battleStarted = true;

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        var go = Instantiate(enemyPrefab, pos, Quaternion.identity);

        _spawnedEnemy = go.GetComponent<MathEnemy>();
        if (_spawnedEnemy == null)
        {
            Debug.LogError("[BattleSetup] MathEnemy не найден на префабе!");
            Destroy(go);
            return;
        }

        // Передаём данные боя в экземпляр врага — до StartBattle
        _spawnedEnemy.Init(battleData);
        _spawnedEnemy.OnDied += OnEnemyDied;

        _player.LockMovement();
        BattleManager.Instance.SetPlayer(_player);
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