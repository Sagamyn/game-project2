using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image icon;
    public TextMeshProUGUI amountText;
    public Image background;

    ItemData item;
    int amount;

    CanvasGroup canvasGroup;
    Transform originalParent;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
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

        amountText.text = amount > 1 ? "x" + amount : "";
        amountText.enabled = amount > 1;
    }

    void Clear()
    {
        item = null;
        amount = 0;
        icon.enabled = false;
        amountText.text = "";
    }

    public ItemData GetItem() => item;

    // ================= DRAG =================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null) return;

        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(originalParent);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }

    // 🔥 ACCEPT DROP FROM HOTBAR
    public void OnDrop(PointerEventData eventData)
    {
        HotbarSlotUI hotbarSlot =
            eventData.pointerDrag?.GetComponent<HotbarSlotUI>();

        if (hotbarSlot == null)
            return;

        hotbarSlot.ClearFromHotbar();
        Debug.Log("Returned hotbar item to inventory");
    }
}
