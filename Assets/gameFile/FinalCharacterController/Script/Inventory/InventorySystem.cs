using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[System.Serializable]
public class InventorySystem
{
    [SerializeField] private List<InventorySlot> inventorySlots;
    public List<InventorySlot> InventorySlots => inventorySlots;
    public int InventorySize => inventorySlots.Count;

    public UnityAction<InventorySlot> OnInventorySlotChanged;

    public InventorySystem(int size)
    {
        inventorySlots = new List<InventorySlot>(size);

        for (int i = 0; i < size; i++)
        {
            // Add empty slots (null and stack size -1)
            inventorySlots.Add(new InventorySlot());
        }
    }

    public bool AddToInventory(InventoryItemData itemToAdd, int amountToAdd)
    {
        if (ContainsItem(itemToAdd, out List<InventorySlot> inventorySlot)) //check whether the item is already in the inventory
        {
            foreach (var slot in inventorySlot)
            {
                if (slot.RoomLeftInStack(amountToAdd))
                {
                    slot.AddToStack(amountToAdd);
                    OnInventorySlotChanged?.Invoke(slot);
                    return true;
                }
            }
        }

        if (HasFreeSlot(out InventorySlot freeSlot)) //gets the first available slot
        {
            freeSlot.UpdateInventorySlot(itemToAdd, amountToAdd);
            OnInventorySlotChanged?.Invoke(freeSlot);
            return true;
        }
        return false;
    }

    public bool RemoveToInventory(InventoryItemData itemToRemove, int amountToRemove)
    {
        if (ContainsItem(itemToRemove, out List<InventorySlot> inventorySlots))
        {
            foreach (var slot in inventorySlots)
            {
                if (slot.StackSize >= amountToRemove)
                {
                    slot.RemoveFromStack(amountToRemove);
                    if (slot.StackSize <= 0)
                    {
                        slot.ClearSlot();
                    }

                    OnInventorySlotChanged?.Invoke(slot);
                    return true; 
                }
            }
        }

        return false;
    }

    public List<InventorySlot> GetAllFilledSlots()
    {
        return inventorySlots.Where(slot => slot.ItemData != null).ToList();
    }

    public bool ContainsItem(InventoryItemData itemToAdd, out List<InventorySlot> inventorySlot)
    {
        inventorySlot = InventorySlots.Where(i => i.ItemData == itemToAdd).ToList();
        return inventorySlot == null ? false : true;
    }

    public bool HasFreeSlot(out InventorySlot freeSlot)
    {
        freeSlot = InventorySlots.FirstOrDefault(i => i.ItemData == null);
        return freeSlot == null ? false : true;
    }

    public int GetItemStack(InventoryItemData itemData)
    {
        foreach (var slot in inventorySlots)
        {
            if (slot.ItemData == itemData)
                return slot.StackSize;
        }
        return 0;
    }
}
