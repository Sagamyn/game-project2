using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropZone : MonoBehaviour, IDropHandler
{
    public InventoryOwner owner;

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI draggedSlot =
            eventData.pointerDrag?.GetComponent<InventorySlotUI>();

        if (draggedSlot == null)
            return;

        // Same inventory? Do nothing
        if (draggedSlot.owner == owner)
            return;

        InventoryTransferManager.Instance.Transfer(
            draggedSlot.owner,
            draggedSlot.GetSlotIndex(),
            owner,
            -1 // -1 means "any free slot"
        );
    }
}
