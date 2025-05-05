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
    private PlayerStats stats;
    private void Awake()
    {
        activeWeapon = GetComponent<ActiveWeapon>();
        stats = GetComponent<PlayerStats>();

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
                if (passiveItemInventory.AddToInventory(itemData, 1))
                {
                    var prefab = itemData.Prefab;
                    if (prefab != null)
                    {
                        var effect = prefab.GetComponent<ItemEffectBehaviour>();
                        if (effect != null && effect.config != null)
                        {
                            stats.TryAddEffectFromItem(effect.config, prefab);
                        }
                    }
                    return true; //return true when successfully added
                }
                return false;

            case ItemType.Active:
                activeItem = itemData;
                return true;

            case ItemType.Weapon:
                // Prevent picking up the same weapon twice
                if ((weaponSlot1 != null && weaponSlot1.ID == itemData.ID) ||
                    (weaponSlot2 != null && weaponSlot2.ID == itemData.ID))
                {
                    return false;
                }


                if (weaponSlot1 == null)
                {
                    weaponSlot1 = itemData;
                    EquipWeaponImmediately(weaponSlot1);
                    activeWeapon.SyncEquippedWeaponsWithInventory();
                    return true;
                }
                else if (weaponSlot2 == null)
                {
                    weaponSlot2 = itemData;
                    activeWeapon.SyncEquippedWeaponsWithInventory();
                    return true;
                }

                return false; // Can't carry more than two weapons

            default:
                return inventorySystem.AddToInventory(itemData, 1);
        }
    }

    private void EquipWeaponImmediately(InventoryItemData weaponData)
    {
        if (activeWeapon == null)
        {
            Debug.LogError("InventoryHolder: activeWeapon is NULL!"); // <== THIS should show up
            return;
        }
            activeWeapon.EquipWeaponFromInventory(weaponData);
    }
}
