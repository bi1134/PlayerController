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


    private ActiveWeapon activeWeapon;
    private void Awake()
    {
        activeWeapon = GetComponent<ActiveWeapon>();

        if (activeWeapon == null)
        {
            Debug.LogError("InventoryHolder: activeWeapon is NULL during Awake!");
        }
        else
        {
            Debug.Log("InventoryHolder: activeWeapon is set to " + activeWeapon.name);
        }
        inventorySystem = new InventorySystem(inventorySize);
        passiveItemInventory = new InventorySystem(passiveInventorySize);
    }

    public bool PickUpItem(InventoryItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogError("InventoryHolder: Tried to pick up a NULL item!");
            return false;
        }

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
        Debug.Log("EquipWeaponImmediately: Called with " + weaponData);
        if (activeWeapon == null)
        {
            Debug.LogError("InventoryHolder: activeWeapon is NULL!"); // <== THIS should show up
            return;
        }
            activeWeapon.EquipWeaponFromInventory(weaponData);
    }
}
