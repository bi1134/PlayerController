using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject enemyPrefab;

    public int maxEnemies = 10;
    public float spawnCooldown = 3f;

    private float cooldownTimer;
    private List<GameObject> aliveEnemies = new List<GameObject>();

    private void Start()
    {
        cooldownTimer = 0f;
        SpawnNewEnemy();
    }

    private void OnEnable()
    {
        EnemyDeathState.OnDeath += HandleEnemyDeath;
    }

    private void OnDisable()
    {
        EnemyDeathState.OnDeath -= HandleEnemyDeath;
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
        aliveEnemies.RemoveAll(enemy => enemy == null);
    }

    void SpawnNewEnemy()
    {
        int index = Random.Range(0, spawnPoints.Length - 1);
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPoints[index].position, Quaternion.identity);
        aliveEnemies.Add(newEnemy);
    }

    void HandleEnemyDeath()
    {
        // Don't spawn here — just allow cooldown to trigger next one
        // Logic is handled in Update
    }
}
