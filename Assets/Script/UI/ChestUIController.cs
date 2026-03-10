using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ChestUIController : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory playerInventory;
    public InventorySlotUI slotPrefab;

    [Header("UI Panels")]
    public GameObject panel;               // The whole ChestUI panel
    public GameObject chestPanelUI;        // ChestPanelUI (chest slots)
    public GameObject playerInventoryPanel; // PlayerInventoryPanel (player slots)
    public UIAnimator uiAnimator;

    [Header("Grids")]
    public Transform playerGrid;   // PlayerInventoryPanel → Grid
    public Transform chestGrid;    // ChestPanelUI → Grid

    private ChestInventory currentChest;
    private List<InventorySlotUI> playerSlots = new List<InventorySlotUI>();
    private List<InventorySlotUI> chestSlots = new List<InventorySlotUI>();

    private bool isOpen;
    private bool isRefreshing;
    private bool playerSlotsCreated = false;

    void Awake()
    {
        // Hide everything at start
        if (uiAnimator != null)
            uiAnimator.HideInstant();
        else if (panel != null)
            panel.SetActive(false);

        // Make sure both sub-panels are hidden too
        if (chestPanelUI != null)
            chestPanelUI.SetActive(false);

        if (playerInventoryPanel != null)
            playerInventoryPanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (playerInventory != null)
            playerInventory.OnInventoryChanged -= RefreshPlayerInventory;

        if (currentChest != null)
            currentChest.OnInventoryChanged -= RefreshChestInventory;
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    // =======================
    // OPEN / CLOSE
    // =======================

    public void Open(ChestInventory chest)
    {
        if (chest == null)
        {
            Debug.LogError("ChestUIController.Open: ChestInventory is NULL");
            return;
        }

        currentChest = chest;
        isOpen = true;

        // FORCE: Show panels FIRST before creating slots
        ForceShowAllPanels();

        // Subscribe to inventory change events
        playerInventory.OnInventoryChanged += RefreshPlayerInventory;
        currentChest.OnInventoryChanged += RefreshChestInventory;

        // Create player slots if needed (first time only)
        if (!playerSlotsCreated || playerSlots.Count == 0)
        {
            CreatePlayerSlots();
            playerSlotsCreated = true;
        }

        // Build chest slots
        CreateChestSlots();

        // Force refresh after a frame delay
        StartCoroutine(RefreshAfterFrame());
    }

    void ForceShowAllPanels()
    {
        // Show root panel
        if (uiAnimator != null)
        {
            panel.SetActive(true);
            uiAnimator.Show();
        }
        else if (panel != null)
        {
            panel.SetActive(true);
        }

        // FORCE: Show both sub-panels explicitly
        if (chestPanelUI != null)
        {
            chestPanelUI.SetActive(true);
            ForceEnableChildren(chestPanelUI.transform);
        }

        if (playerInventoryPanel != null)
        {
            playerInventoryPanel.SetActive(true);
            ForceEnableChildren(playerInventoryPanel.transform);
        }

        // FORCE: Enable grids
        if (playerGrid != null)
            playerGrid.gameObject.SetActive(true);

        if (chestGrid != null)
            chestGrid.gameObject.SetActive(true);

        Debug.Log("✓ Chest opened — showing ChestPanelUI + PlayerInventoryPanel");
    }

    void ForceEnableChildren(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).gameObject.SetActive(true);
        }
    }

    public void Close()
    {
        if (!isOpen) return;

        isOpen = false;

        // Hide root panel
        if (uiAnimator != null)
            uiAnimator.Hide();
        else if (panel != null)
            panel.SetActive(false);

        // Hide both sub-panels
        if (chestPanelUI != null)
            chestPanelUI.SetActive(false);

        if (playerInventoryPanel != null)
            playerInventoryPanel.SetActive(false);

        // Unsubscribe events
        if (playerInventory != null)
            playerInventory.OnInventoryChanged -= RefreshPlayerInventory;

        if (currentChest != null)
            currentChest.OnInventoryChanged -= RefreshChestInventory;

        // Clean up chest-specific slots
        DestroyChestSlots();

        currentChest = null;

        Debug.Log("✓ Chest UI closed");
    }

    // =======================
    // SLOT CREATION
    // =======================

    void CreatePlayerSlots()
    {
        foreach (var slot in playerSlots)
            if (slot != null) Destroy(slot.gameObject);
        playerSlots.Clear();

        if (playerGrid == null)
        {
            Debug.LogError("PlayerGrid is NULL! Assign it in the Inspector.");
            return;
        }

        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory is NULL! Assign it in the Inspector.");
            return;
        }

        if (playerInventory.items == null || playerInventory.items.Count == 0)
        {
            Debug.LogError("PlayerInventory.items is NULL or empty!");
            return;
        }

        Debug.Log($"Creating {playerInventory.items.Count} player slots...");

        // FORCE: Make sure parent is active before creating children
        playerGrid.gameObject.SetActive(true);
        if (playerInventoryPanel != null)
            playerInventoryPanel.SetActive(true);

        for (int i = 0; i < playerInventory.items.Count; i++)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, playerGrid);
            slot.transform.SetParent(playerGrid, false);
            
            // FORCE: Everything visible
            slot.gameObject.SetActive(true);
            slot.enabled = true;
            
            slot.name = $"PlayerSlot_{i}";
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
            
            playerSlots.Add(slot);
        }

        // Force canvas updates
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(playerGrid.GetComponent<RectTransform>());
        
        Debug.Log($"✓ Created {playerSlots.Count} player slots");
    }

    void CreateChestSlots()
    {
        DestroyChestSlots();

        if (currentChest == null || chestGrid == null) return;

        // FORCE: Make sure parent is active before creating children
        chestGrid.gameObject.SetActive(true);
        if (chestPanelUI != null)
            chestPanelUI.SetActive(true);

        for (int i = 0; i < currentChest.items.Count; i++)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, chestGrid);
            slot.transform.SetParent(chestGrid, false);
            
            // FORCE: Everything visible
            slot.gameObject.SetActive(true);
            slot.enabled = true;
            
            slot.name = $"ChestSlot_{i}";
            slot.owner = InventoryOwner.Chest;
            slot.Initialize(i);
            
            // FORCE: Canvas group visible
            CanvasGroup cg = slot.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            
            chestSlots.Add(slot);
        }

        // Force canvas updates
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(chestGrid.GetComponent<RectTransform>());
        
        Debug.Log($"✓ Created {chestSlots.Count} chest slots");
    }

    void DestroyChestSlots()
    {
        foreach (var slot in chestSlots)
            if (slot != null) Destroy(slot.gameObject);
        chestSlots.Clear();
    }

    // =======================
    // REFRESH
    // =======================

    void RefreshPlayerInventory()
    {
        if (isRefreshing || !isOpen) return;
        isRefreshing = true;

        // Remove any destroyed slots
        playerSlots.RemoveAll(slot => slot == null);

        // Recreate if all slots were destroyed
        if (playerSlots.Count == 0)
        {
            Debug.LogWarning("Player slots were destroyed! Recreating...");
            CreatePlayerSlots();
        }

        for (int i = 0; i < playerSlots.Count && i < playerInventory.items.Count; i++)
        {
            if (playerSlots[i] != null)
            {
                var data = playerInventory.items[i];
                
                // FORCE: Make sure slot is visible
                playerSlots[i].gameObject.SetActive(true);
                
                // Set data
                playerSlots[i].Set(data.item, data.amount);
            }
        }

        isRefreshing = false;
    }

    void RefreshChestInventory()
    {
        if (isRefreshing || !isOpen || currentChest == null) return;
        isRefreshing = true;

        // Remove any destroyed slots
        chestSlots.RemoveAll(slot => slot == null);

        for (int i = 0; i < chestSlots.Count && i < currentChest.items.Count; i++)
        {
            if (chestSlots[i] != null)
            {
                var data = currentChest.items[i];
                
                // FORCE: Make sure slot is visible
                chestSlots[i].gameObject.SetActive(true);
                
                // Set data
                chestSlots[i].Set(data.item, data.amount);
            }
        }

        isRefreshing = false;
    }

    // Refresh after frame to ensure everything is initialized
    IEnumerator RefreshAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        
        // Force panels visible again
        if (playerInventoryPanel != null) playerInventoryPanel.SetActive(true);
        if (chestPanelUI != null) chestPanelUI.SetActive(true);
        if (playerGrid != null) playerGrid.gameObject.SetActive(true);
        if (chestGrid != null) chestGrid.gameObject.SetActive(true);
        
        // Force all slots active
        foreach (var slot in playerSlots)
            if (slot != null) slot.gameObject.SetActive(true);
        
        foreach (var slot in chestSlots)
            if (slot != null) slot.gameObject.SetActive(true);
        
        // Refresh both inventories
        RefreshPlayerInventory();
        RefreshChestInventory();
        
        Canvas.ForceUpdateCanvases();
        
        Debug.Log("✓ Post-frame refresh complete");
    }
}