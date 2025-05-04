using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public BaseStatsSO baseStats;

    private Dictionary<string, float> currentStats = new();

    private void Start()
    {
        var health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.maxHealth = baseStats.maxHealth;
            health.currentHealth = baseStats.maxHealth; // full heal on start
        }
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
}
