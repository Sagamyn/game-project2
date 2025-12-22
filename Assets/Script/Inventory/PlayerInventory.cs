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
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].item == item)
            {
                items[i].amount -= amount;

                if (items[i].amount <= 0)
                {
                    items.RemoveAt(i);
                }

                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }

}
