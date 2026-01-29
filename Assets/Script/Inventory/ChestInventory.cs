using UnityEngine;

public class ChestInventory : InventoryBase
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
            Debug.Log($"ChestInventory initialized with {maxSlots} empty slots");
        }
        else
        {
            Debug.Log($"ChestInventory already has {items.Count} slots - skipping init");
        }
    }

    // Chest-specific: just override if you need custom behavior
    public override bool AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0) return false;

        // Try to stack if stackable
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

        // Find empty slot
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

        Debug.LogWarning("Chest is full!");
        return false;
    }
}