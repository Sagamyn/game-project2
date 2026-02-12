using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory inventory;
    public PlayerMovement playerMovement;
    public PlayerFarming playerFarming;
    public PlayerMoney playerMoney;

    [Header("UI Setup")]
    public InventorySlotUI slotPrefab;
    public Transform gridParent;
    public Hotbar hotbar;
    public GameObject panel;
    public TMP_Text moneyText;
    public UIAnimator uiAnimator; // Optional: for animations

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

        if (playerMoney != null)
        playerMoney.OnMoneyChanged += UpdateMoneyDisplay;

        // Create all slots once at start
        CreateSlots();

        // Close inventory by default - but keep InventoryUI active!
        if (uiAnimator != null)
        {
            uiAnimator.HideInstant();
        }
        else
        {
            panel.SetActive(false);
        }

        isOpen = false;

        // Initial refresh
        Refresh();

        if (playerMoney != null)
        UpdateMoneyDisplay(playerMoney.CurrentMoney);
    }

    void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= Refresh;
        }

        if (playerMoney != null)
        {
            playerMoney.OnMoneyChanged -= UpdateMoneyDisplay;
        }   
    }

    void Update()
    {
        // Always check for Tab input, even when panel is hidden
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

        // Use animator if available
        if (uiAnimator != null)
        {
            panel.SetActive(true); // Activate first
            uiAnimator.Show();
        }
        else
        {
            panel.SetActive(true);
        }

        if (playerMovement != null)
            playerMovement.LockMovement(true);

        // Optional: Hide hotbar when inventory opens
        if (HotbarVisibilityManager.Instance != null)
        {
            HotbarVisibilityManager.Instance.OnInventoryOpen();
        }

        Refresh();
    }

    void CloseInventory()
    {
        isOpen = false;

        // Use animator if available, otherwise just hide
        if (uiAnimator != null)
            uiAnimator.Hide();
        else
            panel.SetActive(false);

        if (playerMovement != null)
            playerMovement.LockMovement(false);

        // Optional: Show hotbar when inventory closes
        if (HotbarVisibilityManager.Instance != null)
        {
            HotbarVisibilityManager.Instance.OnInventoryClose();
        }
    }

    // Create slots ONCE at the start - they never get destroyed
    void CreateSlots()
    {
        // Clear any existing slots
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        slotUIList.Clear();

        if (slotPrefab == null)
        {
            Debug.LogError("Slot Prefab is NULL! Please assign it in the Inspector.");
            return;
        }

        if (gridParent == null)
        {
            Debug.LogError("Grid Parent is NULL! Please assign it in the Inspector.");
            return;
        }

        Debug.Log($"Creating {inventory.maxSlots} slots...");

        // Create slots for all inventory positions
        for (int i = 0; i < inventory.maxSlots; i++)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, gridParent);
            slot.name = $"InventorySlot_{i}"; // Give it a clear name for debugging
            slot.Initialize(i); // Pass the index
            slotUIList.Add(slot);

            // Make sure the slot is active and visible
            slot.gameObject.SetActive(true);
            
            // Add click listener for hotbar assignment
            int index = i; // Capture for lambda
            var button = slot.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => AssignToHotbar(index));
            }

            Debug.Log($"Created slot {i}: {slot.name}");
        }

        // Force layout update
        Canvas.ForceUpdateCanvases();

        Debug.Log($"✓ Successfully created {slotUIList.Count} inventory slots");
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
            Debug.LogWarning("No slots found! Creating slots...");
            CreateSlots();
        }

        Debug.Log($"Refreshing {slotUIList.Count} slots with {inventory.items.Count} items");

        // Update each slot's display WITHOUT changing their position
        for (int i = 0; i < slotUIList.Count && i < inventory.items.Count; i++)
        {
            if (slotUIList[i] != null)
            {
                var itemSlot = inventory.items[i];
                slotUIList[i].Set(itemSlot.item, itemSlot.amount);
                
                // Debug each slot
                if (itemSlot.item != null)
                {
                    Debug.Log($"Slot {i}: {itemSlot.item.itemName} x{itemSlot.amount}");
                }
            }
            else
            {
                Debug.LogError($"Slot {i} is NULL!");
            }
        }

        isRefreshing = false;
        Debug.Log("✓ Inventory UI refreshed - slots updated in place");
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

    void UpdateMoneyDisplay(int money)
    {
        if (moneyText != null)
        {
            moneyText.text = money.ToString();
        }
    }
}