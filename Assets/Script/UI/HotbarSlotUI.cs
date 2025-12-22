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

    ItemData item;
    CanvasGroup canvasGroup;
    Vector3 startPosition;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    // 🔹 PURE UI UPDATE
    public void Set(ItemData newItem, int amount)
    {
        item = newItem;

        if (item == null)
        {
            ClearVisual();
            return;
        }

        icon.sprite = item.icon;
        icon.enabled = true;
        icon.color = Color.white;

        amountText.text = amount > 0 ? "x" + amount : "";
        amountText.enabled = amount > 0;
    }

    void ClearVisual()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = true;                  // keep raycast
        icon.color = new Color(1, 1, 1, 0);
        amountText.text = "";
    }

    public ItemData GetItem() => item;

    public void SetSelected(bool selected)
    {
        if (selectionBorder != null)
            selectionBorder.enabled = selected;
    }

    // ================= DRAG =================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null) return;

        startPosition = transform.position;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        transform.position = startPosition;
    }

    // 🔹 INVENTORY → HOTBAR
    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI invSlot =
            eventData.pointerDrag?.GetComponent<InventorySlotUI>();

        if (invSlot == null) return;

        ItemData droppedItem = invSlot.GetItem();
        if (droppedItem == null) return;

        hotbar.SetSlot(slotIndex, droppedItem);
    }

    // 🔹 CALLED BY INVENTORY DROP
    public void ClearFromHotbar()
    {
        hotbar.ClearSlot(slotIndex);
        ClearVisual();
    }
}