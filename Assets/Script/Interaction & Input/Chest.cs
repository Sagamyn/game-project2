using UnityEngine;

public class Chest : Interactable
{
    public ChestInventory chestInventory;
    public ChestUIController chestUI;

    bool isOpen;

    protected override void Awake()
    {
        base.Awake();

        if (chestInventory == null)
            chestInventory = GetComponent<ChestInventory>();
    }

    public override void Interact()
    {
        if (isOpen)
            CloseChest();
        else
            OpenChest();
    }

    void OpenChest()
    {
        if (chestUI == null)
        {
            Debug.LogError("ChestUIController NOT FOUND");
            return;
        }

        if (chestInventory == null)
        {
            Debug.LogError("ChestInventory NOT FOUND");
            return;
        }

        // Tell the transfer manager which chest is active
        InventoryTransferManager.Instance?.SetCurrentChest(chestInventory);

        isOpen = true;
        chestUI.Open(chestInventory);

        PlayerMovement.Instance?.LockMovement(true);

        // Hide hotbar when chest opens
        if (HotbarVisibilityManager.Instance != null)
        {
            HotbarVisibilityManager.Instance.OnChestOpen();
        }
    }

    void CloseChest()
    {
        if (chestUI == null)
        {
            Debug.LogError("ChestUIController NOT FOUND");
            return;
        }

        isOpen = false;
        chestUI.Close();

        // Clear current chest in transfer manager
        if (InventoryTransferManager.Instance != null)
            InventoryTransferManager.Instance.SetCurrentChest(null);

        PlayerMovement.Instance?.LockMovement(false);

        // Show hotbar when chest closes
        if (HotbarVisibilityManager.Instance != null)
        {
            HotbarVisibilityManager.Instance.OnChestClose();
        }
    }
}