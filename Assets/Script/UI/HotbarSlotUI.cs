using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class HotbarSlotUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI")]
    public Image icon;
    public Image selectionBorder;
    public TextMeshProUGUI amountText;

    [Header("Hotbar")]
    public Hotbar hotbar;
    public int slotIndex;

    private ItemData item;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 startPosition;

    // Static for drag tracking
    private static HotbarSlotUI draggedHotbarSlot;
    private static GameObject dragVisualObject;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    // UI UPDATE
    public void Set(ItemData newItem, int amount)
    {
        item = newItem;

        if (item == null)
        {
            ClearVisual();
            return;
        }

        if (icon != null)
        {
            icon.sprite = item.icon;
            icon.enabled = true;
            icon.color = Color.white;
        }

        if (amountText != null)
        {
            amountText.text = amount > 0 ? "x" + amount : "";
            amountText.enabled = amount > 0;
        }
    }

    void ClearVisual()
    {
        item = null;
        
        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = true; // keep raycast
            icon.color = new Color(1, 1, 1, 0);
        }

        if (amountText != null)
        {
            amountText.text = "";
            amountText.enabled = false;
        }
    }

    public ItemData GetItem() => item;
    public int GetSlotIndex() => slotIndex;

    public void SetSelected(bool selected)
    {
        if (selectionBorder != null)
            selectionBorder.enabled = selected;
    }

    // ================= DRAG =================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null) return;

        draggedHotbarSlot = this;
        startPosition = transform.position;

        // Create drag visual
        CreateDragVisual();

        // Make original transparent
        canvasGroup.alpha = 0.6f;

        Debug.Log($"Started dragging: {item.itemName} from Hotbar slot {slotIndex}");
    }

    void CreateDragVisual()
    {
        dragVisualObject = new GameObject("HotbarDragVisual");
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        dragVisualObject.transform.SetParent(rootCanvas.transform);
        dragVisualObject.transform.SetAsLastSibling();

        RectTransform dragRect = dragVisualObject.AddComponent<RectTransform>();
        dragRect.sizeDelta = rectTransform.sizeDelta;

        CanvasGroup dragCG = dragVisualObject.AddComponent<CanvasGroup>();
        dragCG.blocksRaycasts = false;
        dragCG.alpha = 0.8f;

        // Icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(dragVisualObject.transform);
        Image dragIcon = iconObj.AddComponent<Image>();
        dragIcon.sprite = item.icon;
        dragIcon.color = Color.white;
        
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = icon.rectTransform.sizeDelta;
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.localScale = Vector3.one;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedHotbarSlot != this || dragVisualObject == null) return;
        dragVisualObject.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedHotbarSlot != this) return;

        // Destroy drag visual
        if (dragVisualObject != null)
        {
            Destroy(dragVisualObject);
            dragVisualObject = null;
        }

        // Restore original
        canvasGroup.alpha = 1f;
        transform.position = startPosition;

        draggedHotbarSlot = null;

        Debug.Log("Ended hotbar drag");
    }

    // INVENTORY/CHEST → HOTBAR
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"Drop on Hotbar slot {slotIndex}");

        // Check if dragging from inventory
        InventorySlotUI invSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (invSlot != null)
        {
            ItemData droppedItem = invSlot.GetItem();
            if (droppedItem == null)
            {
                Debug.LogWarning("Dropped item is null");
                return;
            }

            // CRITICAL CHECK: Only allow items from PLAYER inventory to hotbar
            if (invSlot.owner != InventoryOwner.Player)
            {
                Debug.LogWarning($"❌ Cannot add {droppedItem.itemName} to hotbar - item is in {invSlot.owner} inventory, not Player inventory!");
                return;
            }

            // Verify item actually exists in player inventory
            PlayerInventory playerInv = FindObjectOfType<PlayerInventory>();
            if (playerInv != null && !playerInv.HasItem(droppedItem))
            {
                Debug.LogWarning($"❌ Cannot add {droppedItem.itemName} to hotbar - not in player inventory!");
                return;
            }

            // Assign to hotbar
            hotbar.SetSlot(slotIndex, droppedItem);

            Debug.Log($"✓ Assigned {droppedItem.itemName} to hotbar slot {slotIndex}");

            // Refresh inventory UI
            InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
            if (inventoryUI != null)
                inventoryUI.Refresh();

            return;
        }

        // Check if swapping between hotbar slots
        HotbarSlotUI otherHotbarSlot = eventData.pointerDrag?.GetComponent<HotbarSlotUI>();
        if (otherHotbarSlot != null && otherHotbarSlot != this)
        {
            // Swap hotbar slots
            ItemData tempItem = this.item;
            this.item = otherHotbarSlot.item;
            otherHotbarSlot.item = tempItem;

            // Update hotbar data
            hotbar.SetSlot(this.slotIndex, this.item);
            hotbar.SetSlot(otherHotbarSlot.slotIndex, otherHotbarSlot.item);

            Debug.Log($"✓ Swapped hotbar slots {otherHotbarSlot.slotIndex} and {this.slotIndex}");
        }
    }

    // CALLED BY INVENTORY WHEN DRAGGING HOTBAR → INVENTORY
    public void ClearFromHotbar()
    {
        hotbar.ClearSlot(slotIndex);
        ClearVisual();
        Debug.Log($"Cleared hotbar slot {slotIndex}");
    }

    // Public method to check if this slot is being dragged (for InventorySlotUI)
    public static HotbarSlotUI GetDraggedSlot() => draggedHotbarSlot;
}