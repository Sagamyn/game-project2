using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class InventoryBase : MonoBehaviour
{
    [Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int amount;

        public bool IsEmpty => item == null || amount <= 0;
    }

    public List<InventorySlot> items = new List<InventorySlot>();
    public event Action OnInventoryChanged;

    // ================= QUERY =================

    public bool HasItem(ItemData item)
    {
        if (item == null) return false;

        foreach (var slot in items)
            if (slot.item == item && slot.amount > 0)
                return true;

        return false;
    }

    public bool HasItem(ItemData item, int requiredAmount)
    {
        if (item == null) return false;

        int total = 0;
        foreach (var slot in items)
        {
            if (slot.item == item)
            {
                total += slot.amount;
                if (total >= requiredAmount)
                    return true;
            }
        }
        return false;
    }

    public int GetAmount(ItemData item)
    {
        int total = 0;
        foreach (var slot in items)
            if (slot.item == item)
                total += slot.amount;
        return total;
    }

    // ================= ADD / REMOVE =================

    public virtual bool AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return false;

        if (item.stackable)
        {
            foreach (var slot in items)
            {
                if (slot.item == item)
                {
                    slot.amount += amount;
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        foreach (var slot in items)
        {
            if (slot.IsEmpty)
            {
                slot.item = item;
                slot.amount = amount;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        return false;
    }
        public void NotifyChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    public virtual void ConsumeItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0) return;

        foreach (var slot in items)
        {
            if (slot.item == item)
            {
                slot.amount -= amount;

                if (slot.amount <= 0)
                {
                    slot.item = null;
                    slot.amount = 0;
                }

                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }

    // ================= UTIL =================

    public void SwapSlots(int a, int b)
    {
        if (a < 0 || b < 0 || a >= items.Count || b >= items.Count)
            return;

        var temp = items[a];
        items[a] = items[b];
        items[b] = temp;

        OnInventoryChanged?.Invoke();
    }

public void ClearSlot(int index)
{
    if (index < 0 || index >= items.Count)
        return;

    items[index].item = null;
    items[index].amount = 0;

    OnInventoryChanged?.Invoke();
}
}
    