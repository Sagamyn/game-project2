using UnityEngine;
// system logic for hotbar
public class Hotbar : MonoBehaviour
{
    [Header("Slots")]
    public ItemData[] slots = new ItemData[10];
    private const int HotbarSize = 10;

    [Header("Selection")]
    public int selectedIndex;

    [Header("References")]
    public PlayerFarming playerFarming;
    public HotbarUI hotbarUI;
    public PlayerInventory playerInventory;

    [Header("Scroll")]
    public float scrollCooldown = 0.1f;
    float lastScrollTime;


    public ItemData SelectedItem =>
        (selectedIndex >= 0 && selectedIndex < HotbarSize && playerInventory != null)
            ? playerInventory.items[selectedIndex].item
            : null;

    void Start()
    {
        // Subscribe to inventory changes to auto-clear hotbar when items are removed
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += CheckHotbarValidity;
        }

        UpdateSelection();
    }

    void Awake()
    {
        if (playerFarming == null)
        {
            playerFarming = FindObjectOfType<PlayerFarming>();
        }
    }

    void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= CheckHotbarValidity;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Select(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Select(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) Select(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) Select(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) Select(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) Select(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) Select(6);
        if (Input.GetKeyDown(KeyCode.Alpha8)) Select(7);
        if (Input.GetKeyDown(KeyCode.Alpha9)) Select(8);
        if (Input.GetKeyDown(KeyCode.Alpha0)) Select(9);

        HandleScroll();
    }

    void HandleScroll()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll == 0)
            return;

        if (Time.time - lastScrollTime < scrollCooldown)
            return;

        lastScrollTime = Time.time;

        if (scroll > 0) SelectPrevious();
        else SelectNext();
    }

    void SelectNext()
    {
        Select((selectedIndex + 1) % HotbarSize);
    }

    void SelectPrevious()
    {
        Select((selectedIndex - 1 + HotbarSize) % HotbarSize);
    }

    public void Select(int index)
    {
        if (index < 0 || index >= HotbarSize)
            return;

        selectedIndex = index;
        UpdateSelection();
    }

    void UpdateSelection()
    {
        Debug.Log($"selectedIndex: {selectedIndex}");
        Debug.Log($"playerInventory null: {playerInventory == null}");
        Debug.Log($"playerInventory.items count: {playerInventory?.items.Count}");
        Debug.Log($"item at index: {playerInventory?.items[selectedIndex]?.item}");

        if (playerFarming != null)
            playerFarming.selectedItem = SelectedItem;

        if (hotbarUI != null)
            hotbarUI.RefreshSelection();

        Debug.Log(
            SelectedItem != null
                ? $"Hotbar selected: {SelectedItem.itemName}"
                : "Hotbar selected: EMPTY"
        );
    }

    public void SetSlot(int index, ItemData item)
    {
        if (index < 0 || index >= HotbarSize)
            return;

        // cek apakah item sudah ada dihotbar, kalau iya, clear slot lama untuk mencegah dupe
        if (item != null)
        {
            for (int i = 0; i < HotbarSize; i++)
            {
                if (i != index && playerInventory.items[i].item == item)
                {
                    playerInventory.items[i].item = null;
                    playerInventory.items[i].amount = 0;
                    playerInventory.NotifyChanged();
                    Debug.Log($"Hotbar: cleared duplicate item {item.itemName} from slot {i}");
                }
            }
        }
        playerInventory.items[index].item = item;
        playerInventory.NotifyChanged();

        if (hotbarUI != null)
            hotbarUI.Refresh();

        UpdateSelection();
    }

    public void ClearSlot(int index)
    {
        if (index < 0 || index >= HotbarSize)
            return;

        playerInventory.items[index].item = null;
        playerInventory.items[index].amount = 0;
        playerInventory.NotifyChanged();

        if (hotbarUI != null)
            hotbarUI.Refresh();

        UpdateSelection();
    }

    public ItemData GetSlot(int index)
    {
        if (index < 0 || index >= HotbarSize)
            return null;
        if (playerInventory == null) return null;
        return playerInventory.items[index].item;
    }

    // Check if hotbar items still exist in player inventory
    void CheckHotbarValidity()
    {
        if (playerInventory == null) return;

        bool changed = false;

        for (int i = 0; i < HotbarSize; i++)
        {
            if (playerInventory.items[i].item == null) continue;
            // cek qty item di inventory
            int amount = playerInventory.GetAmount(playerInventory.items[i].item);
            // clear slot jika item qty <= 0
            if (amount <= 0)
            {
                Debug.Log($"Hotbar : clearing slot {i} {{ playerInventory.items[i].item?.itemName }} - no longer in inventory");
                playerInventory.items[i].item = null;
                changed = true;

                if (i == selectedIndex && playerFarming != null)
                    playerFarming.selectedItem = null;
            }
            // original code, cek keberadaan item di inventory
            // {
            //     // Check if player still has this item
            //     if (!playerInventory.HasItem(slots[i]))
            //     {
            //         Debug.Log($"Item {slots[i].itemName} no longer in inventory - clearing hotbar slot {i}");
            //         slots[i] = null;
            //         changed = true;

            //         if (i == selectedIndex && playerFarming != null)
            //             playerFarming.selectedItem = null;
            //     }
            // }
        }

        if (changed && hotbarUI != null)
            hotbarUI.Refresh();
        if (changed)
            UpdateSelection();
    }

    public void ClearSlotsContaining(ItemData item)
    {
        bool cleared = false;

        for (int i = 0; i < HotbarSize; i++)
        {
            if (slots[i] == item)
            {
                slots[i] = null;
                cleared = true;

                if (i == selectedIndex && playerFarming != null)
                    playerFarming.selectedItem = null;
            }
        }

        if (cleared && hotbarUI != null)
            hotbarUI.Refresh();
    }
}