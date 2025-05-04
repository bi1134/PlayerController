using UnityEngine;

public class ItemEffectBehaviour : MonoBehaviour
{
    public ItemEffectConfig config;
    private PlayerStats player;
    private float lastTriggerTime;

    public void Initialize(PlayerStats pStats)
    {
        player = pStats;
        if (config.trigger == EffectTrigger.OnPickup)
            TryTrigger(null, null);
    }

    public void TryTrigger(EnemyHealth enemy, GameObject context)
    {
        if (Time.time - lastTriggerTime < config.cooldown || Random.value > config.chanceToProc) return;
        
        int stacks = player.GetItemStackForEffect(config);
        if (stacks <= 0) return;

        float scaledValue = config.value * stacks;

        switch (config.action)
        {
            case EffectAction.DealDamage:
                if (enemy != null)
                {
                    float bonusDamage = player.baseStats.baseDamage;
                    enemy.TakeDamage(scaledValue + bonusDamage, Vector3.up);
                }
                break;

            case EffectAction.Heal:
                player.Heal(scaledValue);
                break;

            case EffectAction.SpawnPrefab:
                if (config.spawnPrefab != null)
                    Instantiate(config.spawnPrefab, player.transform.position + Vector3.up, Quaternion.identity);
                break;

            case EffectAction.ModifyStat:
                player.ModifyStat(config.targetStat, scaledValue);
                break;

            case EffectAction.ExecuteEnemy:
                if (enemy != null && enemy.GetHealthPercent() <= config.maxTriggerHealthThreshold)
                    enemy.TakeDamage(enemy.currentHealth, Vector3.up);
                break;
        }

        lastTriggerTime = Time.time;
    }

}
