using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory inventory;
    public PlayerMovement playerMovement;
    public PlayerFarming playerFarming;

    public InventorySlotUI slotPrefab;
    public Transform gridParent;
    public Hotbar hotbar;
    [Header("UI")]
    public GameObject panel; //  the visual panel only

    InventorySlotUI selectedSlot;
    bool isOpen;

    void Start()
    {
        inventory.OnInventoryChanged += Refresh;
        panel.SetActive(false);
        isOpen = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isOpen)
                CloseInventory();
            else
                OpenInventory();
        }
    }

    void OpenInventory()
    {
        isOpen = true;
        panel.SetActive(true);

        if (playerMovement != null)
            playerMovement.LockMovement(true);

        Refresh();
    }

    void CloseInventory()
    {
        isOpen = false;
        panel.SetActive(false);

        if (playerMovement != null)
            playerMovement.LockMovement(false);
    }

    void Refresh()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        selectedSlot = null;

        foreach (var itemSlot in inventory.items)
        {
            InventorySlotUI slot =
                Instantiate(slotPrefab, gridParent);

            slot.Set(itemSlot.item, itemSlot.amount);

            slot.GetComponent<UnityEngine.UI.Button>()
                .onClick.AddListener(() =>
                    SelectSlot(slot));
        }
    }

    void SelectSlot(InventorySlotUI slot)
    {
        if (slot == null)
        {
            Debug.LogError("Slot is null");
            return;
        }

        if (hotbar == null)
        {
            Debug.LogError("Hotbar reference is NULL in InventoryUI");
            return;
        }

        ItemData item = slot.GetItem();
        if (item == null)
        {
            Debug.LogWarning("Selected slot has no item");
            return;
        }

        int index = hotbar.selectedIndex;

        Debug.Log($"Assigning {item.itemName} to hotbar slot {index}");

        hotbar.SetSlot(index, item);
        hotbar.Select(index);

        if (playerFarming != null)
            playerFarming.selectedItem = item;
    }


}
