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
    }

    public List<WeightedItem> items;

    public InventoryItemData GetRandomItem()
    {
        float totalWeight = 0f;
        foreach (var entry in items)
        {
            totalWeight += entry.weight;
        }

        float roll = Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var entry in items)
        {
            cumulative += entry.weight;
            if (cumulative >= roll)
            {
                return entry.itemData;
            }
        }

        return null;
    }
}
