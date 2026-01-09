using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI References")]
    public Image icon;
    public TextMeshProUGUI amountText;
    public Image background;

    [Header("Drag Visual")]
    public GameObject dragVisual; // Optional: separate visual for dragging

    private ItemData item;
    private int amount;
    private int slotIndex;

    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 originalPosition;
    private RectTransform rectTransform;

    // Static reference to track what's being dragged
    private static InventorySlotUI draggedSlot;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
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

        icon.sprite = item.icon;
        icon.enabled = true;
        icon.color = Color.white;

        amountText.text = amount > 1 ? amount.ToString() : "";
        amountText.enabled = amount > 1;

        // Make filled slot fully visible
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
        icon.sprite = null;
        icon.enabled = false;
        amountText.text = "";

        // Make empty slot slightly transparent
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
        // Don't allow dragging empty slots
        if (item == null) return;

        draggedSlot = this;

        // Store original position
        originalParent = transform.parent;
        originalPosition = transform.position;

        // Move to root so it renders on top
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        // Make it semi-transparent and disable raycast blocking
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        Debug.Log($"Started dragging: {item.itemName} from slot {slotIndex}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedSlot != this) return;

        // Follow mouse/touch position
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedSlot != this) return;

        // Reset visual state
        transform.SetParent(originalParent);
        transform.position = originalPosition;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        draggedSlot = null;

        Debug.Log($"Ended dragging: {item?.itemName}");
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Check if something is being dragged
        if (draggedSlot == null) return;

        // Check if dragging from hotbar
        HotbarSlotUI hotbarSlot = eventData.pointerDrag?.GetComponent<HotbarSlotUI>();
        if (hotbarSlot != null)
        {
            // Remove from hotbar
            hotbarSlot.ClearFromHotbar();
            return;
        }

        // Check if dragging from another inventory slot
        InventorySlotUI draggedInventorySlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        if (draggedInventorySlot != null && draggedInventorySlot != this)
        {
            // Perform the swap
            SwapSlots(draggedInventorySlot);
        }
    }

    private void SwapSlots(InventorySlotUI otherSlot)
    {
        // Get inventory reference
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
        if (inventory == null)
        {
            Debug.LogError("PlayerInventory not found!");
            return;
        }

        int thisIndex = this.slotIndex;
        int otherIndex = otherSlot.slotIndex;

        Debug.Log($"Swapping slot {otherIndex} ({otherSlot.item?.itemName}) with slot {thisIndex} ({this.item?.itemName})");

        // Swap in the inventory data (this will trigger OnInventoryChanged)
        inventory.SwapSlots(thisIndex, otherIndex);

        // The InventoryUI will refresh and update all slots automatically
        // No need to manually update visuals here
    }

    // Optional: Visual feedback when hovering
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (draggedSlot != null && draggedSlot != this && background != null)
        {
            // Highlight as drop target
            Color highlightColor = background.color;
            highlightColor.a = 1f;
            background.color = highlightColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (background != null && item == null)
        {
            // Return to normal empty state
            Color normalColor = background.color;
            normalColor.a = 0.5f;
            background.color = normalColor;
        }
    }
}