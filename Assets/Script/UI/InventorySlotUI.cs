using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public enum InventoryOwner
{
    Player,
    Chest
}

public class InventorySlotUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI References - MUST BE ASSIGNED")]
    public Image icon;
    public TextMeshProUGUI amountText;
    public Image background;

    [Header("Info (Don't Change)")]
    public InventoryOwner owner;
    
    private ItemData item;
    private int amount;
    private int slotIndex;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    // Static reference to track what's being dragged
    private static InventorySlotUI draggedSlot;
    private static GameObject dragVisualObject;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Validate references
        if (icon == null)
            Debug.LogError($"Icon is not assigned on {gameObject.name}!");
        if (amountText == null)
            Debug.LogError($"AmountText is not assigned on {gameObject.name}!");
        if (background == null)
            Debug.LogError($"Background is not assigned on {gameObject.name}!");

        // Make sure the slot can receive drops
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    public void Initialize(int index)
    {
        slotIndex = index;
    }

    public void Set(ItemData newItem, int newAmount)
    {
        item = newItem;
        amount = newAmount;

        if (item == null || amount <= 0)
        {
            Clear();
            return;
        }

        // Set icon
        if (icon != null)
        {
            icon.sprite = item.icon;
            icon.enabled = true;
            icon.color = Color.white;
        }

        // Set amount text
        if (amountText != null)
        {
            amountText.text = amount > 1 ? amount.ToString() : "";
            amountText.enabled = amount > 1;
        }

        // Set background
        if (background != null)
        {
            Color filledColor = background.color;
            filledColor.a = 1f;
            background.color = filledColor;
        }
    }

    void Clear()
    {
        item = null;
        amount = 0;

        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }

        if (amountText != null)
        {
            amountText.text = "";
            amountText.enabled = false;
        }

        if (background != null)
        {
            Color emptyColor = background.color;
            emptyColor.a = 0.5f;
            background.color = emptyColor;
        }
    }

    public ItemData GetItem() => item;
    public int GetAmount() => amount;
    public int GetSlotIndex() => slotIndex;

    // ================= DRAG & DROP =================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null) return;

        draggedSlot = this;

        // Create drag visual
        CreateDragVisual();

        // Make original transparent
        canvasGroup.alpha = 0.4f;

        Debug.Log($"✓ Started dragging: {item.itemName} from {owner} slot {slotIndex}");
    }

    void CreateDragVisual()
    {
        // Clean up any existing drag visual
        if (dragVisualObject != null)
        {
            Destroy(dragVisualObject);
        }

        dragVisualObject = new GameObject("DragVisual");
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        dragVisualObject.transform.SetParent(rootCanvas.transform);
        dragVisualObject.transform.SetAsLastSibling(); // Render on top

        RectTransform dragRect = dragVisualObject.AddComponent<RectTransform>();
        dragRect.sizeDelta = rectTransform.sizeDelta;
        dragRect.localScale = Vector3.one; // IMPORTANT!
        
        // Set initial position to mouse
        dragRect.position = Input.mousePosition;

        CanvasGroup dragCG = dragVisualObject.AddComponent<CanvasGroup>();
        dragCG.blocksRaycasts = false; // Don't block raycasts
        dragCG.alpha = 0.9f;

        // Background (makes it visible!)
        Image bgImage = dragVisualObject.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark background
        bgImage.raycastTarget = false;

        // Icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(dragVisualObject.transform, false); // worldPositionStays = false!
        Image dragIcon = iconObj.AddComponent<Image>();
        dragIcon.sprite = item.icon;
        dragIcon.color = Color.white;
        dragIcon.raycastTarget = false;
        
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = new Vector2(-10, -10); // Padding from edges
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.localScale = Vector3.one;

        // Amount text
        if (amount > 1)
        {
            GameObject textObj = new GameObject("Amount");
            textObj.transform.SetParent(dragVisualObject.transform, false); // worldPositionStays = false!
            TextMeshProUGUI dragText = textObj.AddComponent<TextMeshProUGUI>();
            dragText.text = amount.ToString();
            dragText.fontSize = amountText != null ? amountText.fontSize : 16;
            dragText.color = Color.white;
            dragText.alignment = TextAlignmentOptions.BottomRight;
            dragText.raycastTarget = false;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = new Vector2(-5, -5); // Small padding
            textRect.anchoredPosition = Vector2.zero;
            textRect.localScale = Vector3.one;
        }

        Debug.Log($"✓ Created drag visual at position {dragRect.position}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedSlot != this || dragVisualObject == null) return;
        
        // Update position to follow mouse
        RectTransform dragRect = dragVisualObject.GetComponent<RectTransform>();
        if (dragRect != null)
        {
            dragRect.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedSlot != this) return;

        if (dragVisualObject != null)
        {
            Destroy(dragVisualObject);
            dragVisualObject = null;
        }

        canvasGroup.alpha = 1f;
        draggedSlot = null;

        Debug.Log("✓ Ended dragging");
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"✓ OnDrop called on {owner} slot {slotIndex}");

        // Check if dragging from hotbar
        HotbarSlotUI hotbarSlot = eventData.pointerDrag?.GetComponent<HotbarSlotUI>();
        if (hotbarSlot != null)
        {
            HandleHotbarDrop(hotbarSlot);
            return;
        }

        // Check if dragging from inventory slot
        if (draggedSlot == null)
        {
            Debug.LogWarning("No inventory slot is being dragged");
            return;
        }

        InventorySlotUI draggedInventorySlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (draggedInventorySlot != null && draggedInventorySlot != this)
        {
            HandleInventoryDrop(draggedInventorySlot);
        }
        else
        {
            Debug.LogWarning("Dragged object is not an InventorySlotUI or is the same slot");
        }
    }

    void HandleHotbarDrop(HotbarSlotUI hotbarSlot)
    {
        ItemData hotbarItem = hotbarSlot.GetItem();
        if (hotbarItem == null)
        {
            Debug.LogWarning("Hotbar slot is empty");
            return;
        }

        Debug.Log($"🎯 Hotbar → {owner}: {hotbarItem.itemName} to slot {slotIndex}");

        // Get the player inventory (source of the item)
        PlayerInventory playerInv = FindObjectOfType<PlayerInventory>();
        if (playerInv == null)
        {
            Debug.LogError("PlayerInventory not found!");
            return;
        }

        // Get the amount from player inventory
        int itemAmount = playerInv.GetAmount(hotbarItem);
        if (itemAmount <= 0)
        {
            Debug.LogWarning($"No {hotbarItem.itemName} in player inventory!");
            return;
        }

        // Get the target inventory
        InventoryBase targetInv = null;
        if (owner == InventoryOwner.Player)
        {
            // Dragging from hotbar back to player inventory - just clear hotbar reference
            hotbarSlot.ClearFromHotbar();
            Debug.Log("✓ Cleared hotbar slot (item stays in player inventory)");
            return;
        }
        else if (owner == InventoryOwner.Chest)
        {
            targetInv = InventoryTransferManager.Instance?.currentChest;
        }

        if (targetInv == null)
        {
            Debug.LogError($"Target inventory not found for {owner}");
            return;
        }

        // Transfer from player inventory to target (chest)
        bool success = false;

        // Check if target slot is empty
        if (this.item == null || slotIndex >= targetInv.items.Count)
        {
            // Add to specific slot
            if (slotIndex < targetInv.items.Count)
            {
                targetInv.items[slotIndex].item = hotbarItem;
                targetInv.items[slotIndex].amount = itemAmount;
                success = true;
            }
        }
        else if (this.item == hotbarItem)
        {
            // Same item - stack it
            targetInv.items[slotIndex].amount += itemAmount;
            success = true;
        }
        else
        {
            Debug.LogWarning($"Slot {slotIndex} already has {this.item.itemName}");
            return;
        }

        if (success)
        {
            // Remove from player inventory
            playerInv.ConsumeItem(hotbarItem, itemAmount);

            // Clear hotbar slot
            hotbarSlot.ClearFromHotbar();

            // Notify both inventories
            targetInv.NotifyChanged();

            Debug.Log($"✓ Transferred {itemAmount}x {hotbarItem.itemName} from Player to {owner}");
        }
    }

    void HandleInventoryDrop(InventorySlotUI draggedSlot)
    {
        Debug.Log($"🎯 Drop: {draggedSlot.owner} slot {draggedSlot.slotIndex} ({draggedSlot.item?.itemName}) → {this.owner} slot {this.slotIndex}");

        // Same inventory - swap slots
        if (draggedSlot.owner == this.owner)
        {
            if (owner == InventoryOwner.Player)
            {
                PlayerInventory inv = FindObjectOfType<PlayerInventory>();
                if (inv != null)
                {
                    inv.SwapSlots(draggedSlot.slotIndex, this.slotIndex);
                    Debug.Log("✓ Swapped within Player inventory");
                }
                else
                {
                    Debug.LogError("PlayerInventory not found!");
                }
            }
            else if (owner == InventoryOwner.Chest)
            {
                ChestInventory inv = InventoryTransferManager.Instance?.currentChest;
                if (inv != null)
                {
                    inv.SwapSlots(draggedSlot.slotIndex, this.slotIndex);
                    Debug.Log("✓ Swapped within Chest inventory");
                }
                else
                {
                    Debug.LogError("Current chest inventory not found!");
                }
            }
        }
        // Different inventory - transfer
        else
        {
            if (InventoryTransferManager.Instance == null)
            {
                Debug.LogError("❌ InventoryTransferManager.Instance is null!");
                return;
            }

            Debug.Log($"🔄 Transferring from {draggedSlot.owner} to {this.owner}");
            InventoryTransferManager.Instance.Transfer(
                draggedSlot.owner,
                draggedSlot.slotIndex,
                this.owner,
                this.slotIndex
            );
        }
    }

    // Visual feedback
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (draggedSlot != null && draggedSlot != this && background != null)
        {
            Color highlightColor = background.color;
            highlightColor.a = 1f;
            background.color = highlightColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (background != null && item == null)
        {
            Color normalColor = background.color;
            normalColor.a = 0.5f;
            background.color = normalColor;
        }
    }
}