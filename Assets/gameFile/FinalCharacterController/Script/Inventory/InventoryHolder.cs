using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InventoryHolder : MonoBehaviour
{
    [Header("General Inventory")]
    [SerializeField] private int inventorySize;
    [SerializeField] protected InventorySystem inventorySystem;

    [Header("Passive Items (Buffs)")]
    [SerializeField] private int passiveInventorySize;
    [SerializeField] private InventorySystem passiveItemInventory;

    [Header("Active Items (Equipment)")]
    [SerializeField] private InventoryItemData activeItem; //only holds 1 item at a time

    [Header("Weapon Slots")]
    public InventoryItemData weaponSlot1;
    public InventoryItemData weaponSlot2;

    public InventorySystem InventorySystem => inventorySystem;
    public InventorySystem PassiveItemInventory => passiveItemInventory;

    public static UnityAction<InventorySystem> OnDynamicInventoryDisplayRequested;

    private void Awake()
    {
        inventorySystem = new InventorySystem(inventorySize);
        passiveItemInventory = new InventorySystem(passiveInventorySize);
    }

    public bool PickUpItem(InventoryItemData itemData)
    {
        switch (itemData.ItemType)
        {
            case ItemType.Passive:
                return passiveItemInventory.AddToInventory(itemData, 1);

            case ItemType.Active:
                activeItem = itemData;
                return true;

            case ItemType.Weapon:
                if (weaponSlot1 == null)
                {
                    weaponSlot1 = itemData;
                    EquipWeaponImmediately(weaponSlot1);
                    return true;
                }
                else if (weaponSlot2 == null)
                {
                    weaponSlot2 = itemData;
                    return true;
                }
                return false; // Can't carry more than two weapons

            default:
                return inventorySystem.AddToInventory(itemData, 1);
        }
    }

    private void EquipWeaponImmediately(InventoryItemData weaponData)
    {
        ActiveWeapon activeWeapon = GetComponent<ActiveWeapon>();
        if (activeWeapon != null)
        {
            activeWeapon.EquipWeaponFromInventory(weaponData);
        }
    }
}
