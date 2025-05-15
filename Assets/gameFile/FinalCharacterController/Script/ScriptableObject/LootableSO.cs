using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LootableSO", menuName = "Scriptable Objects/LootableSO")]
public class LootableSO : ScriptableObject
{
    [System.Serializable]
    public class WeightedItem
    {
        public InventoryItemData itemData;
        public float weight;
        public Rarity rarity;
    }

    public List<WeightedItem> items;

    public InventoryItemData GetRandomItem()
    {
        float totalWeight = 0f;

        foreach (var entry in items)
        {
            float weight = entry.weight > 0 ? entry.weight : GetRarityWeight(entry.rarity);
            totalWeight += weight;
        }

        float roll = Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var entry in items)
        {
            float weight = entry.weight > 0 ? entry.weight : GetRarityWeight(entry.rarity);
            cumulative += weight;
            if (cumulative >= roll)
            {
                return entry.itemData;
            }
        }

        return null;
    }

    private float GetRarityWeight(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 60f,
            Rarity.Uncommon => 25f,
            Rarity.Rare => 10f,
            Rarity.Epic => 4f,
            Rarity.Legendary => 1f,
            _ => 0f
        };
    }
}
