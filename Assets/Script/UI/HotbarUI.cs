using UnityEngine;

public class HotbarUI : MonoBehaviour
{
    public Hotbar hotbar;
    public HotbarSlotUI[] slots;
    public PlayerInventory inventory;

    void Start()
    {
        if (inventory != null)
            inventory.OnInventoryChanged += Refresh;

        Refresh();
    }

    void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= Refresh;
    }

    public void Refresh()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            ItemData item = hotbar.GetSlot(i);
            int amount = item != null ? inventory.GetAmount(item) : 0;

            slots[i].Set(item, amount);
        }

        RefreshSelection();
    }

    public void RefreshSelection()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].SetSelected(i == hotbar.selectedIndex);
    }
}
