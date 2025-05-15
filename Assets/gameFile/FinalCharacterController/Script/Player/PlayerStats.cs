using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public BaseStatsSO baseStats;
    public PlayerHealth playerHealth;

    private Dictionary<StatType, float> currentStats = new();

    public List<ItemEffectBehaviour> activeEffects = new();

    private InventoryHolder inventoryHolder;


    //bool
    public bool allowRetriggerOnDuplicatePickup = false;

    private void Awake()
    {
        inventoryHolder = GetComponent<InventoryHolder>();
    }

    private void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.maxHealth = baseStats.maxHealth;
            playerHealth.currentHealth = baseStats.maxHealth; // full heal on start
            playerHealth.TriggerHealthChanged();
        }
    }

    private void Update()
    {
        TriggerEffects(EffectTrigger.PassiveTick);
    }

    public void ModifyStat(StatType stat, float amount)
    {
        if (!currentStats.ContainsKey(stat)) currentStats[stat] = 0f;
        currentStats[stat] += amount;

        if (stat == StatType.MaxHealth && playerHealth != null)
        {
            playerHealth.maxHealth += amount;

            playerHealth.currentHealth += amount;

            playerHealth.TriggerHealthChanged();
        }
    }

    public float GetStat(StatType statName)
    {
        return currentStats.ContainsKey(statName) ? currentStats[statName] : 0f;
    }

    public void Heal(float amount)
    {
        playerHealth.Heal(amount);
    }

    public int GetItemStackForEffect(ItemEffectConfig config)
    {
        if (inventoryHolder == null) return 0;

        var passiveSlots = inventoryHolder.PassiveItemInventory.GetAllFilledSlots();
        foreach (var slot in passiveSlots)
        {
            if (slot.ItemData != null && slot.ItemData.Prefab != null)
            {
                var itemEffect = slot.ItemData.Prefab.GetComponent<ItemEffectBehaviour>();
                if (itemEffect != null && itemEffect.config == config)
                    return slot.StackSize;
            }
        }
        return 0;
    }

    public void TriggerEffects(EffectTrigger trigger, EnemyHealth enemy = null)
    {
        foreach (var effect in activeEffects)
        {
            if (effect.config.trigger == trigger)
            {
                effect.TryTrigger(enemy, null);
            }
        }
    }

    public void TryAddEffectFromItem(ItemEffectConfig config, GameObject prefab)
    {
        foreach (var effect in activeEffects)
        {
            if (effect.config == config)
            {
                // Always retrigger for OnPickup to update stat scaling
                if (config.trigger == EffectTrigger.OnPickup)
                {
                    effect.TryTrigger(null, null);
                }
                return; // Still don't instantiate another one
            }
        }

        // First-time pickup
        GameObject effectObj = Instantiate(prefab, transform);
        effectObj.name = config.effectName + "_Effect";

        var itemEffect = effectObj.GetComponent<ItemEffectBehaviour>();
        if (itemEffect != null)
        {
            activeEffects.Add(itemEffect);
            itemEffect.Initialize(this);
        }
    }

}
