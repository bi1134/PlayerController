using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public BaseStatsSO baseStats;

    private Dictionary<string, float> currentStats = new();
    private EnemyHealth health;


    private void Start()
    {
        health = GetComponent<EnemyHealth>();
    }

    public float GetStat(string statName)
    {
        return currentStats.TryGetValue(statName, out var value) ? value : 0f;
    }

    public void ModifyStat(string statName, float amount)
    {
        if (!currentStats.ContainsKey(statName)) currentStats[statName] = 0f;
        currentStats[statName] += amount;
    }

    public void ResetStats()
    {
        if (health != null)
        {
            health.maxHealth = baseStats.maxHealth;
            health.currentHealth = baseStats.maxHealth; // full heal on start
        }
    }

    public float GetHealthPercent()
    {
        if (health == null || health.maxHealth <= 0f) return 0f;
        return health.currentHealth / health.maxHealth;
    }

    public float CurrentHPPercent =>
   (health != null && health.maxHealth > 0f)
       ? health.currentHealth / health.maxHealth
       : 0f;
}
