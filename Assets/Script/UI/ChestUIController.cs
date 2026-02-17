using UnityEngine;
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

        // Pre-create player slots since player inventory never changes
        CreatePlayerSlots();
    }

    void OnDestroy()
    {
        if (playerInventory != null)
            playerInventory.OnInventoryChanged -= RefreshPlayerInventory;

        if (currentChest != null)
            currentChest.OnInventoryChanged -= RefreshChestInventory;
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

        // Show the root panel
        if (uiAnimator != null)
        {
            panel.SetActive(true);
            uiAnimator.Show();
        }
        else if (panel != null)
        {
            panel.SetActive(true);
        }

        // Show both sub-panels
        if (chestPanelUI != null)
            chestPanelUI.SetActive(true);

        if (playerInventoryPanel != null)
            playerInventoryPanel.SetActive(true);

        Debug.Log($"✓ Chest opened — showing ChestPanelUI + PlayerInventoryPanel");

        // Subscribe to inventory change events
        playerInventory.OnInventoryChanged += RefreshPlayerInventory;
        currentChest.OnInventoryChanged += RefreshChestInventory;

        // Build chest slots and refresh both
        CreateChestSlots();
        RefreshPlayerInventory();
        RefreshChestInventory();
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

        for (int i = 0; i < playerInventory.items.Count; i++)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, playerGrid);
            slot.transform.SetParent(playerGrid, false);
            slot.name = $"PlayerSlot_{i}";
            slot.owner = InventoryOwner.Player;
            slot.Initialize(i);
            playerSlots.Add(slot);
        }

        Canvas.ForceUpdateCanvases();
        Debug.Log($"✓ Created {playerSlots.Count} player slots");
    }

    void CreateChestSlots()
    {
        DestroyChestSlots();

        if (currentChest == null || chestGrid == null) return;

        for (int i = 0; i < currentChest.items.Count; i++)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, chestGrid);
            slot.transform.SetParent(chestGrid, false);
            slot.name = $"ChestSlot_{i}";
            slot.owner = InventoryOwner.Chest;
            slot.Initialize(i);
            chestSlots.Add(slot);
        }

        Canvas.ForceUpdateCanvases();
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

        for (int i = 0; i < playerSlots.Count && i < playerInventory.items.Count; i++)
        {
            var data = playerInventory.items[i];
            playerSlots[i].Set(data.item, data.amount);
        }

        isRefreshing = false;
    }

    void RefreshChestInventory()
    {
        if (isRefreshing || !isOpen || currentChest == null) return;
        isRefreshing = true;

        for (int i = 0; i < chestSlots.Count && i < currentChest.items.Count; i++)
        {
            var data = currentChest.items[i];
            chestSlots[i].Set(data.item, data.amount);
        }

        isRefreshing = false;
    }
}