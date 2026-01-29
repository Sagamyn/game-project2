using UnityEngine;
using System.Collections.Generic;

public class ChestUIController : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory playerInventory;
    public InventorySlotUI slotPrefab;

    [Header("UI")]
    public Transform playerGrid;
    public Transform chestGrid;
    public GameObject panel;
    public UIAnimator uiAnimator; // Optional: for animations

    private ChestInventory currentChest;
    private List<InventorySlotUI> playerSlots = new List<InventorySlotUI>();
    private List<InventorySlotUI> chestSlots = new List<InventorySlotUI>();

    private bool isOpen;
    private bool isRefreshing;

    void Awake()
    {
        if (uiAnimator != null)
            uiAnimator.HideInstant();
        else
            panel.SetActive(false);
        
        // Create player slots ONCE at start (they're always the same)
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
        
        // Use animator if available
        if (uiAnimator != null)
            uiAnimator.Show();
        else
            panel.SetActive(true);

        isOpen = true;

        Debug.Log($"Opening chest with {chest.items.Count} slots");

        // Subscribe to events
        playerInventory.OnInventoryChanged += RefreshPlayerInventory;
        currentChest.OnInventoryChanged += RefreshChestInventory;

        // Create chest slots for THIS specific chest
        CreateChestSlots();

        // Refresh both
        RefreshPlayerInventory();
        RefreshChestInventory();
    }

    public void Close()
    {
        if (!isOpen) return;

        isOpen = false;

        // Use animator if available
        if (uiAnimator != null)
            uiAnimator.Hide();
        else
            panel.SetActive(false);

        // Unsubscribe
        if (playerInventory != null)
            playerInventory.OnInventoryChanged -= RefreshPlayerInventory;

        if (currentChest != null)
            currentChest.OnInventoryChanged -= RefreshChestInventory;

        // Destroy chest slots (they're chest-specific)
        DestroyChestSlots();
        
        currentChest = null;
    }

    // =======================
    // SLOT CREATION
    // =======================

    void CreatePlayerSlots()
    {
        // Clear existing
        foreach (var slot in playerSlots)
            if (slot != null) Destroy(slot.gameObject);
        playerSlots.Clear();

        if (playerGrid == null)
        {
            Debug.LogError("PlayerGrid is NULL! Please assign it in the Inspector.");
            return;
        }

        Debug.Log($"Creating {playerInventory.items.Count} player slots...");
        Debug.Log($"PlayerGrid: {playerGrid.name}");

        // Create new slots
        for (int i = 0; i < playerInventory.items.Count; i++)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, playerGrid);
            slot.transform.SetParent(playerGrid, false); // Force set parent
            slot.name = $"PlayerSlot_{i}";
            slot.owner = InventoryOwner.Player;
            slot.Initialize(i);
            playerSlots.Add(slot);
        }

        Canvas.ForceUpdateCanvases();
        Debug.Log($"✓ Created {playerSlots.Count} player slots in playerGrid");
    }

    void CreateChestSlots()
    {
        // Clear existing chest slots
        DestroyChestSlots();

        if (currentChest == null)
        {
            Debug.LogError("Cannot create chest slots - currentChest is null!");
            return;
        }

        if (chestGrid == null)
        {
            Debug.LogError("ChestGrid is NULL! Please assign it in the Inspector.");
            return;
        }

        Debug.Log($"Creating {currentChest.items.Count} chest slots...");
        Debug.Log($"ChestGrid: {chestGrid.name} (Path: {GetGameObjectPath(chestGrid.gameObject)})");

        // Create new slots for current chest
        for (int i = 0; i < currentChest.items.Count; i++)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, chestGrid);
            slot.transform.SetParent(chestGrid, false); // Force set parent
            slot.name = $"ChestSlot_{i}";
            slot.owner = InventoryOwner.Chest;
            slot.Initialize(i);
            chestSlots.Add(slot);

            Debug.Log($"Created {slot.name} under {slot.transform.parent.name}");
        }

        Canvas.ForceUpdateCanvases();
        Debug.Log($"✓ Created {chestSlots.Count} chest slots in chestGrid");
    }

    // Helper to debug hierarchy paths
    string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform current = obj.transform.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        return path;
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
            var slotUI = playerSlots[i];
            var data = playerInventory.items[i];
            slotUI.Set(data.item, data.amount);
        }

        isRefreshing = false;
    }

    void RefreshChestInventory()
    {
        if (isRefreshing || !isOpen || currentChest == null) return;
        isRefreshing = true;

        for (int i = 0; i < chestSlots.Count && i < currentChest.items.Count; i++)
        {
            var slotUI = chestSlots[i];
            var data = currentChest.items[i];
            slotUI.Set(data.item, data.amount);
        }

        isRefreshing = false;
    }
}