using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory inventory;
    public PlayerMovement playerMovement;
    public PlayerFarming playerFarming;

    [Header("UI Setup")]
    public InventorySlotUI slotPrefab;
    public Transform gridParent;
    public Hotbar hotbar;
    public GameObject panel;

    private List<InventorySlotUI> slotUIList = new List<InventorySlotUI>();
    private bool isOpen;
    private bool isRefreshing = false; // Prevent refresh loops

    void Start()
    {
        // Subscribe to inventory changes
        if (inventory != null)
        {
            inventory.OnInventoryChanged += Refresh;
        }

        // Create all slots once at start
        CreateSlots();

        // Close inventory by default
        panel.SetActive(false);
        isOpen = false;

        // Initial refresh
        Refresh();
    }

    void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= Refresh;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isOpen)
                CloseInventory();
            else
                OpenInventory();
        }
    }

    void OpenInventory()
    {
        isOpen = true;
        panel.SetActive(true);

        if (playerMovement != null)
            playerMovement.LockMovement(true);

        Refresh();
    }

    void CloseInventory()
    {
        isOpen = false;
        panel.SetActive(false);

        if (playerMovement != null)
            playerMovement.LockMovement(false);
    }

    // Create slots ONCE at the start - they never get destroyed
    void CreateSlots()
    {
        // Clear any existing slots
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        slotUIList.Clear();

        // Create slots for all inventory positions
        for (int i = 0; i < inventory.maxSlots; i++)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, gridParent);
            slot.name = $"InventorySlot_{i}"; // Give it a clear name for debugging
            slot.Initialize(i); // Pass the index
            slotUIList.Add(slot);

            // Store the original position after layout
            Canvas.ForceUpdateCanvases();
            
            // Add click listener for hotbar assignment
            int index = i; // Capture for lambda
            slot.GetComponent<UnityEngine.UI.Button>()
                ?.onClick.AddListener(() => AssignToHotbar(index));
        }

        // Force layout update
        Canvas.ForceUpdateCanvases();

        Debug.Log($"Created {slotUIList.Count} inventory slots");
    }

    // Update slot visuals without destroying them
    public void Refresh()
    {
        // Prevent recursive refresh calls
        if (isRefreshing) return;
        isRefreshing = true;

        if (inventory == null || inventory.items == null)
        {
            Debug.LogWarning("Inventory or items list is null!");
            isRefreshing = false;
            return;
        }

        // Make sure we have enough slots
        if (slotUIList.Count == 0)
        {
            CreateSlots();
        }

        // Update each slot's display WITHOUT changing their position
        for (int i = 0; i < slotUIList.Count && i < inventory.items.Count; i++)
        {
            if (slotUIList[i] != null)
            {
                var itemSlot = inventory.items[i];
                slotUIList[i].Set(itemSlot.item, itemSlot.amount);
            }
        }

        isRefreshing = false;
        Debug.Log("Inventory UI refreshed - slots updated in place");
    }

    void AssignToHotbar(int inventorySlotIndex)
    {
        if (hotbar == null)
        {
            Debug.LogError("Hotbar reference is NULL!");
            return;
        }

        if (inventorySlotIndex < 0 || inventorySlotIndex >= inventory.items.Count)
        {
            Debug.LogError($"Invalid slot index: {inventorySlotIndex}");
            return;
        }

        ItemData item = inventory.items[inventorySlotIndex].item;
        
        if (item == null)
        {
            Debug.LogWarning("Selected slot is empty");
            return;
        }

        int hotbarIndex = hotbar.selectedIndex;
        Debug.Log($"Assigning {item.itemName} to hotbar slot {hotbarIndex}");

        hotbar.SetSlot(hotbarIndex, item);

        if (playerFarming != null)
            playerFarming.selectedItem = item;
    }
}