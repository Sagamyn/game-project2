using UnityEngine;

public class PlayerInventory : InventoryBase
{
    public int maxSlots = 20;

    void Awake()
    {
        Initialize();
    }

    void Initialize()
    {
        // CRITICAL: Only initialize if empty
        if (items.Count == 0)
        {
            for (int i = 0; i < maxSlots; i++)
            {
                items.Add(new InventorySlot());
            }
            Debug.Log($"PlayerInventory initialized with {maxSlots} empty slots");
        }
        else
        {
            Debug.Log($"PlayerInventory already has {items.Count} slots - skipping init");
        }
    }

    public override bool AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return false;

        if (item.stackable)
        {
            foreach (var slot in items)
            {
                if (slot.item == item)
                {
                    slot.amount += amount;
                    NotifyChanged();
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
                NotifyChanged();
                return true;
            }
        }

        Debug.LogWarning("Player inventory is full!");
        return false;
    }   

    public override void ConsumeItem(ItemData item, int amount)
    {
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

                NotifyChanged();
                return;
            }
        }
    }
}