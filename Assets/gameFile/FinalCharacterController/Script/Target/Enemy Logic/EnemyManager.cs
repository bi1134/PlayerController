using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject enemyPrefab;

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


        if (ObjectPooler.PoolExists("Enemy"))
        {
            SpawnNewEnemy();
        }
        else
        {
            Debug.LogError("Enemy pool not initialized yet!");
        }
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f && aliveEnemies.Count < maxEnemies)
        {
            SpawnNewEnemy();
            cooldownTimer = spawnCooldown;
        }

        // Cleanup destroyed enemies
        aliveEnemies.RemoveAll(enemy => enemy == null || !enemy.activeInHierarchy);
    }

    void SpawnNewEnemy()
    {
        Vector3 spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;

        // Sample NavMesh to find closest valid point
        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            GameObject newEnemy = ObjectPooler.SpawnFromPool("Enemy", hit.position, Quaternion.identity);
            aliveEnemies.Add(newEnemy);
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
