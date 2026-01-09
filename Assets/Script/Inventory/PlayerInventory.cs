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

    public int maxSlots = 28;
    public List<InventorySlot> items = new List<InventorySlot>();
    public event Action OnInventoryChanged;

    void Awake()
    {
        // Initialize with empty slots at the very start
        InitializeSlots();
    }

    void InitializeSlots()
    {
        // Only initialize if empty
        if (items.Count == 0)
        {
            for (int i = 0; i < maxSlots; i++)
            {
                items.Add(new InventorySlot { item = null, amount = 0 });
            }
        }
    }

    public bool HasItem(ItemData item)
    {
        if (item == null)
            return false;

        foreach (var slot in items)
        {
            if (slot.item == item && slot.amount > 0)
                return true;
        }
        return false;
    }

    // Amount-based check (used by quests, crafting, farming, etc.)
    public bool HasItem(ItemData item, int requiredAmount)
    {
        if (item == null)
            return false;

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

    public bool AddItem(ItemData item, int amount)
    {
        // Try to stack with existing item first
        if (item.stackable)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].item == item && items[i].amount > 0)
                {
                    items[i].amount += amount;
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        // Find first empty slot
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].item == null || items[i].amount <= 0)
            {
                items[i].item = item;
                items[i].amount = amount;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }
         Debug.Log($"Trying to add {item.itemName} x{amount}");
        Debug.LogWarning("Inventory is full!");
        return false;
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
                    // Clear the slot but keep it in the list
                    items[i].item = null;
                    items[i].amount = 0;

                    Hotbar hotbar = FindObjectOfType<Hotbar>();
                    if (hotbar != null)
                        hotbar.ClearSlotsContaining(item);
                }

                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }

    // NEW: Swap two inventory slots by index
    public void SwapSlots(int index1, int index2)
    {
        if (index1 < 0 || index1 >= items.Count || index2 < 0 || index2 >= items.Count)
        {
            Debug.LogError($"Invalid swap indices: {index1}, {index2}");
            return;
        }

        // Swap the actual slot data
        InventorySlot temp = items[index1];
        items[index1] = items[index2];
        items[index2] = temp;

        OnInventoryChanged?.Invoke();
    }
}