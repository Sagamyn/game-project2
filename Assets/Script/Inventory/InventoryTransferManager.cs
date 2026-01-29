using UnityEngine;

public class InventoryTransferManager : MonoBehaviour
{
    public static InventoryTransferManager Instance;

    public PlayerInventory playerInventory;
    public ChestInventory currentChest;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Transfer(
        InventoryOwner fromOwner, int fromIndex,
        InventoryOwner toOwner, int toIndex
    )
    {
        Debug.Log($"Transfer: {fromOwner}[{fromIndex}] → {toOwner}[{toIndex}]");

        var fromInv = GetInventory(fromOwner);
        var toInv = GetInventory(toOwner);

        if (fromInv == null)
        {
            Debug.LogError($"Source inventory is null for {fromOwner}");
            return;
        }

        if (toInv == null)
        {
            Debug.LogError($"Target inventory is null for {toOwner}");
            return;
        }

        if (fromIndex < 0 || fromIndex >= fromInv.items.Count)
        {
            Debug.LogError($"Invalid source index: {fromIndex}");
            return;
        }

        var fromSlot = fromInv.items[fromIndex];
        if (fromSlot.item == null || fromSlot.amount <= 0)
        {
            Debug.LogWarning("Source slot is empty");
            return;
        }

        // If target slot is specified and not empty, try to swap
        if (toIndex >= 0 && toIndex < toInv.items.Count)
        {
            var toSlot = toInv.items[toIndex];
            
            // If target is empty, just move
            if (toSlot.IsEmpty)
            {
                toSlot.item = fromSlot.item;
                toSlot.amount = fromSlot.amount;
                fromSlot.item = null;
                fromSlot.amount = 0;
            }
            // If target has item, swap
            else
            {
                var tempItem = toSlot.item;
                var tempAmount = toSlot.amount;

                toSlot.item = fromSlot.item;
                toSlot.amount = fromSlot.amount;

                fromSlot.item = tempItem;
                fromSlot.amount = tempAmount;
            }
        }
        // If no target slot specified, find empty slot
        else
        {
            bool success = toInv.AddItem(fromSlot.item, fromSlot.amount);
            if (!success)
            {
                Debug.LogWarning("Target inventory is full!");
                return;
            }

            fromSlot.item = null;
            fromSlot.amount = 0;
        }

        // Notify both inventories
        fromInv.NotifyChanged();
        toInv.NotifyChanged();

        Debug.Log("Transfer successful!");
    }

    InventoryBase GetInventory(InventoryOwner owner)
    {
        return owner switch
        {
            InventoryOwner.Player => playerInventory,
            InventoryOwner.Chest => currentChest,
            _ => null
        };
    }

    public void SetCurrentChest(ChestInventory chest)
    {
        currentChest = chest;
        Debug.Log($"Current chest set: {(chest != null ? "Active" : "Null")}");
    }
}