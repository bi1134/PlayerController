using UnityEngine;

public enum ItemType
{
    Passive,
    Active,
    Weapon
}

public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

[CreateAssetMenu(fileName = "InventoryItemData", menuName = "Scriptable Objects/InventoryItemData")]
public class InventoryItemData : ScriptableObject
{
    public Rarity rarity;

    public int ID;
    public ItemType ItemType; //determines the type of item
    public string ItemName;
    [TextArea(3, 10)]
    public string Description;
    public Sprite Icon;
    public int MaxStackSize;

    public GameObject Prefab; //use for item or active item
}