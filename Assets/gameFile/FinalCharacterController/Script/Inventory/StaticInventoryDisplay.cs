using UnityEngine;
using System.Collections.Generic;

public class StaticInventoryDisplay : InventoryDisplay
{
    [SerializeField] private InventoryHolder inventoryHolder;
    [SerializeField] private InventorySlot_UI[] inventoryPassiveItemSlots;

    protected override void Start()
    {
        base.Start();

        if (inventoryHolder != null)
        {
            inventorySystem = inventoryHolder.PassiveItemInventory;
            inventorySystem.OnInventorySlotChanged += UpdateSlot;
        }
        else { Debug.LogWarning($"No inventory assigned to {this.gameObject}"); }

        AssignSlot(inventorySystem);
    }

    public override void AssignSlot(InventorySystem invToDisplay)
    {
        slotDictionary = new Dictionary<InventorySlot_UI, InventorySlot>();

        int hotbarSize = Mathf.Min(inventoryPassiveItemSlots.Length, 10); // Show up to 10

        for (int i = 0; i < hotbarSize; i++)
        {
            slotDictionary.Add(inventoryPassiveItemSlots[i], invToDisplay.InventorySlots[i]);
            inventoryPassiveItemSlots[i].Init(invToDisplay.InventorySlots[i]);
        }
    }
}
