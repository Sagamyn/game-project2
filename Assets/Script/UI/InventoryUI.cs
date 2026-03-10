using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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
    public UIAnimator uiAnimator;

    private List<InventorySlotUI> slotUIList = new List<InventorySlotUI>();
    private bool isOpen;
    private bool isRefreshing = false;
    private bool slotsCreated = false;

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
        slotsCreated = true;

        // Close inventory by default
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
        // Check for Tab input
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // CRITICAL: Check if chest is open
            ChestUIController chestUI = FindObjectOfType<ChestUIController>();
            if (chestUI != null && chestUI.IsOpen())
            {
                Debug.Log("Cannot open inventory - chest is already showing inventory");
                return;
            }

            // Check if other blocking UIs are open
            if (!isOpen && IsBlockingUIOpen())
            {
                Debug.Log("Cannot open inventory - another UI is blocking");
                return;
            }

            if (isOpen)
                CloseInventory();
            else
                OpenInventory();
        }
    }

    bool IsBlockingUIOpen()
    {
        CookingUI cookingUI = FindObjectOfType<CookingUI>();
        if (cookingUI != null && cookingUI.IsOpen())
            return true;

        ShopUIManager shopUI = FindObjectOfType<ShopUIManager>();
        if (shopUI != null && shopUI.IsOpen())
            return true;

        RestaurantUI restaurantUI = FindObjectOfType<RestaurantUI>();
        if (restaurantUI != null && restaurantUI.IsOpen())
            return true;

        return false;
    }

    void OpenInventory()
    {
        isOpen = true;

        // FORCE: Show panel and force all children visible
        ForceShowPanel();

        if (playerMovement != null)
            playerMovement.LockMovement(true);

        if (HotbarVisibilityManager.Instance != null)
            HotbarVisibilityManager.Instance.OnInventoryOpen();

        // Force refresh after frame
        StartCoroutine(RefreshAfterFrame());
        
        Debug.Log("✓ Inventory opened");
    }

    void ForceShowPanel()
    {
        // Show panel
        if (uiAnimator != null)
        {
            panel.SetActive(true);
            uiAnimator.Show();
        }
        else
        {
            panel.SetActive(true);
        }

        // FORCE: Enable all children of panel
        ForceEnableChildren(panel.transform);

        // FORCE: Enable grid specifically
        if (gridParent != null)
        {
            gridParent.gameObject.SetActive(true);
        }
    }

    void ForceEnableChildren(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).gameObject.SetActive(true);
        }
    }

    void CloseInventory()
    {
        isOpen = false;

        if (uiAnimator != null)
            uiAnimator.Hide();
        else
            panel.SetActive(false);

        if (playerMovement != null)
            playerMovement.LockMovement(false);

        if (HotbarVisibilityManager.Instance != null)
            HotbarVisibilityManager.Instance.OnInventoryClose();
        
        Debug.Log("✓ Inventory closed");
    }

    public void ForceClose()
    {
        if (isOpen)
            CloseInventory();
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    void CreateSlots()
    {
        // CRITICAL: Don't destroy if slots already exist
        if (slotsCreated && slotUIList.Count > 0)
        {
            Debug.Log("InventoryUI slots already created, skipping recreation...");
            return;
        }

        // Clear any existing slots (only on first creation)
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        slotUIList.Clear();

        if (slotPrefab == null || gridParent == null)
        {
            Debug.LogError("Slot Prefab or Grid Parent is NULL!");
            return;
        }

        if (inventory == null)
        {
            Debug.LogError("PlayerInventory is NULL!");
            return;
        }

        Debug.Log($"Creating {inventory.maxSlots} InventoryUI slots...");

        // FORCE: Make sure parent is active before creating children
        gridParent.gameObject.SetActive(true);

        for (int i = 0; i < inventory.maxSlots; i++)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, gridParent);
            slot.transform.SetParent(gridParent, false);
            
            // FORCE: Everything visible
            slot.gameObject.SetActive(true);
            slot.enabled = true;
            
            slot.name = $"InventoryUISlot_{i}";
            slot.owner = InventoryOwner.Player;
            slot.Initialize(i);
            
            // FORCE: Canvas group visible
            CanvasGroup cg = slot.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            
            slotUIList.Add(slot);
            
            // Add click listener for hotbar assignment
            int index = i;
            var button = slot.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => AssignToHotbar(index));
        }

        // Force canvas updates
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridParent.GetComponent<RectTransform>());
        
        Debug.Log($"✓ Created {slotUIList.Count} InventoryUI slots");
    }

    public void Refresh()
    {
        if (isRefreshing) return;
        isRefreshing = true;

        if (inventory == null || inventory.items == null)
        {
            isRefreshing = false;
            return;
        }

        // Remove null slots
        slotUIList.RemoveAll(slot => slot == null);

        // Recreate if all slots were destroyed
        if (slotUIList.Count == 0)
        {
            Debug.LogWarning("All InventoryUI slots were destroyed! Recreating...");
            slotsCreated = false;
            CreateSlots();
            slotsCreated = true;
        }

        // Update each slot
        for (int i = 0; i < slotUIList.Count && i < inventory.items.Count; i++)
        {
            if (slotUIList[i] != null)
            {
                // FORCE: Make sure slot is visible
                slotUIList[i].gameObject.SetActive(true);
                
                // Set data
                var itemSlot = inventory.items[i];
                slotUIList[i].Set(itemSlot.item, itemSlot.amount);
            }
        }

        isRefreshing = false;
    }

    // Force refresh after frame to ensure everything is initialized
    IEnumerator RefreshAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        
        // Force panel and grid visible again
        if (panel != null) panel.SetActive(true);
        if (gridParent != null) gridParent.gameObject.SetActive(true);
        
        // Force all slots active
        foreach (var slot in slotUIList)
        {
            if (slot != null) slot.gameObject.SetActive(true);
        }
        
        // Refresh inventory
        Refresh();
        
        Canvas.ForceUpdateCanvases();
        
        Debug.Log("✓ InventoryUI post-frame refresh complete");
    }

    void AssignToHotbar(int inventorySlotIndex)
    {
        if (hotbar == null || inventorySlotIndex < 0 || inventorySlotIndex >= inventory.items.Count)
            return;

        ItemData item = inventory.items[inventorySlotIndex].item;
        if (item == null)
            return;

        int hotbarIndex = hotbar.selectedIndex;
        hotbar.SetSlot(hotbarIndex, item);

        if (playerFarming != null)
            playerFarming.selectedItem = item;
    }

    void UpdateMoneyDisplay(int money)
    {
        if (moneyText != null)
            moneyText.text = money.ToString();
    }
}