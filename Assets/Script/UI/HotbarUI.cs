using UnityEngine;

public class HotbarUI : MonoBehaviour
{
    public Hotbar hotbar;
    public HotbarSlotUI[] slots;
    public PlayerInventory inventory;
    // return UI from hotbar.cs
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
        int threshold = 10;
        for (int i = 0; i < threshold; i++)
        {
            ItemData item = inventory.items[i].item;
            if (item != null)
            {
                int amount = inventory.GetAmount(item);
                if (amount <= 0)
                {
                    hotbar.ClearSlot(i);
                    slots[i].Set(null, 0);
                    continue;
                }
                slots[i].Set(item, amount);
            }
            else
            {
                slots[i].Set(null, 0);
            }
            // ItemData item = hotbar.GetSlot(i);
            // int amount = item != null ? inventory.GetAmount(item) : 0;

        }

        RefreshSelection();
    }

    public void RefreshSelection()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].SetSelected(i == hotbar.selectedIndex);
    }
}