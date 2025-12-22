using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    [Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int amount;
    }

    public List<InventorySlot> items = new List<InventorySlot>();

    public event Action OnInventoryChanged;

    public bool HasItem(ItemData item)
    {
        foreach (var slot in items)
            if (slot.item == item && slot.amount > 0)
                return true;
        return false;
    }

    public int GetAmount(ItemData item)
    {
        foreach (var slot in items)
            if (slot.item == item)
                return slot.amount;
        return 0;
    }

    public bool AddItem(ItemData item, int amount)
    {
        foreach (var slot in items)
        {
            if (slot.item == item && item.stackable)
            {
                slot.amount += amount;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        items.Add(new InventorySlot
        {
            item = item,
            amount = amount
        });

        OnInventoryChanged?.Invoke();
        return true;
    }


    public void ConsumeItem(ItemData item, int amount = 1)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].item == item)
            {
                items[i].amount -= amount;

                if (items[i].amount <= 0)
                {
                    items[i].amount = 0;

                    // 🔥 NOTIFY HOTBAR THAT THIS ITEM IS GONE
                    Hotbar hotbar = FindObjectOfType<Hotbar>();
                    if (hotbar != null)
                        hotbar.ClearSlotsContaining(item);
                }

                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }


}