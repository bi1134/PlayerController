using System.Collections.Generic;
using UnityEngine;

public class WeaponDisplay : InventoryDisplay
{
    [SerializeField] private InventoryHolder inventoryHolder;
    [SerializeField] private InventorySlot_UI[] weaponSlots;

    protected override void Start()
    {
        base.Start();

        if (inventoryHolder == null)
        {
            Debug.LogWarning("WeaponSlotDisplay: No InventoryHolder assigned!");
            return;
        }

        inventorySystem = inventoryHolder.WeaponInventory;

        if (inventorySystem != null)
            inventorySystem.OnInventorySlotChanged += UpdateSlot;

        AssignSlot(inventorySystem);
    }

    public override void AssignSlot(InventorySystem invToDisplay)
    {
        slotDictionary = new Dictionary<InventorySlot_UI, InventorySlot>();

        for (int i = 0; i < invToDisplay.InventorySize && i < weaponSlots.Length; i++)
        {
            InventorySlot inventorySlot = invToDisplay.InventorySlots[i];
            if (inventorySlot.ItemData != null && inventorySlot.ItemData.ItemType == ItemType.Weapon)
            {
                slotDictionary.Add(weaponSlots[i], inventorySlot);
                weaponSlots[i].Init(inventorySlot);
            }
        }
    }
}
