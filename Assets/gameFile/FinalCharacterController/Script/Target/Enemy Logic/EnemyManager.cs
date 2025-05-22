using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnEntry
    {
        public EnemyConfig config;
        public string poolTag;
    }

    public Transform[] spawnPoints;
    public List<EnemySpawnEntry> enemySpawnEntries;

    public int maxEnemies = 10;
    public float spawnCooldown = 3f;

    public int enemiesKilled = 0;
    public int maxCorpses = 35;

    private float cooldownTimer;
    private List<GameObject> aliveEnemies = new List<GameObject>();

    private void OnEnable()
    {
        EnemyDeathState.OnDeath += HandleEnemyDeath;
    }

    private void OnDisable()
    {
        EnemyDeathState.OnDeath -= HandleEnemyDeath;
    }

    private IEnumerator Start()
    {
        EnemyCorpseTracker.MaxAllowedCorpses = maxCorpses;
        yield return Helpers.GetWaitForSecond(0.1f);
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f && aliveEnemies.Count < maxEnemies)
        {
            SpawnNewEnemy();
            cooldownTimer = spawnCooldown;
        }

        aliveEnemies.RemoveAll(enemy => enemy == null || !enemy.activeInHierarchy);
    }

    void SpawnNewEnemy()
    {
        if (enemySpawnEntries == null || enemySpawnEntries.Count == 0) return;

        Vector3 spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;

        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            EnemySpawnEntry entry = enemySpawnEntries[Random.Range(0, enemySpawnEntries.Count)];

            GameObject newEnemy = ObjectPooler.SpawnFromPool(entry.poolTag, hit.position, Quaternion.identity);
            aliveEnemies.Add(newEnemy);

            if (newEnemy.TryGetComponent(out Enemy enemy))
            {
                enemy.config = entry.config;
                // Optional: reinit state machine if needed
            }
        }
        else
        {
            Debug.LogWarning("Spawn position not on NavMesh. Skipping spawn.");
        }
    }

    void HandleEnemyDeath()
    {
        enemiesKilled++;
    }
}
