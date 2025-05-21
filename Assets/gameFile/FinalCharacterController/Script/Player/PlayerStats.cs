using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public BaseStatsSO baseStats;
    public PlayerHealth playerHealth;
    public BaseStatsSO playerDefaultStats;
    public TextMeshProUGUI moneyCount;

    private Dictionary<StatType, float> currentStats = new();

    public List<ItemEffectBehaviour> activeEffects = new();

    private InventoryHolder inventoryHolder;


    //bool
    public bool allowRetriggerOnDuplicatePickup = false;

    private void Awake()
    {
        inventoryHolder = GetComponent<InventoryHolder>();
        baseStats.maxHealth = playerDefaultStats.maxHealth;
        baseStats.baseDamage = playerDefaultStats.baseDamage;
        baseStats.moveSpeed = playerDefaultStats.moveSpeed;
        baseStats.critChance = playerDefaultStats.critChance;
        baseStats.level = playerDefaultStats.level;
        baseStats.exp = playerDefaultStats.exp;
        baseStats.money = playerDefaultStats.money;
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

    public void AddMoney(int amount)
    {
        baseStats.money += amount;
        Debug.Log("Money: " + baseStats.money);
        moneyCount.text = baseStats.money.ToString();
    }

    public void OnLevelUp(int newLevel, int lastLevel)
    {
        int levelDelta = newLevel - lastLevel;
        if (levelDelta <= 0) return;

        float healthBonus = levelDelta * 10f;
        float damageBonus = levelDelta * 2f;
        float critBonus = levelDelta * 0.01f;

        ModifyStat(StatType.MaxHealth, healthBonus);
        ModifyStat(StatType.BonusDamage, damageBonus);
        ModifyStat(StatType.CritChance, critBonus);

        if (playerHealth != null)
        {
            playerHealth.currentHealth = playerHealth.maxHealth;
            playerHealth.TriggerHealthChanged();
        }

        lastLevel = newLevel;

        Debug.Log($"[PlayerStats] Level Up! New level {newLevel}. Stats scaled for +{levelDelta} levels.");
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
