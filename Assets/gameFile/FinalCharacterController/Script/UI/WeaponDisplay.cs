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

        for (int i = 0; i < invToDisplay.InventorySize; i++)
        {
            var slot = invToDisplay.InventorySlots[i];
            if (slot.ItemData != null && slot.ItemData.ItemType == ItemType.Weapon)
            {
                int index = slotDictionary.Count;
                if (index >= weaponSlots.Length) break;

                slotDictionary.Add(weaponSlots[index], slot);
                weaponSlots[index].Init(slot);
            }
        }
    }
}
