using UnityEngine;

public class Hotbar : MonoBehaviour
{
    [Header("Slots")]
    public ItemData[] slots = new ItemData[4];

    [Header("Selection")]
    public int selectedIndex;

    [Header("References")]
    public PlayerFarming playerFarming;
    public HotbarUI hotbarUI;

    [Header("Scroll")]
    public float scrollCooldown = 0.1f;
    float lastScrollTime;

    public ItemData SelectedItem =>
        (selectedIndex >= 0 && selectedIndex < slots.Length)
            ? slots[selectedIndex]
            : null;

    void Start()
    {
        UpdateSelection();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Select(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Select(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) Select(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) Select(3);

        HandleScroll();
    }

    void HandleScroll()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll == 0) return;

        if (Time.time - lastScrollTime < scrollCooldown)
            return;

        lastScrollTime = Time.time;

        if (scroll > 0) SelectNext();
        else SelectPrevious();
    }

    void SelectNext()
    {
        Select((selectedIndex + 1) % slots.Length);
    }

    void SelectPrevious()
    {
        Select((selectedIndex - 1 + slots.Length) % slots.Length);
    }

    public void Select(int index)
    {
        if (index < 0 || index >= slots.Length)
            return;

        selectedIndex = index;
        UpdateSelection();
    }

    void UpdateSelection()
    {
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
        if (index < 0 || index >= slots.Length)
            return;

        slots[index] = item;

        if (hotbarUI != null)
            hotbarUI.Refresh();

        UpdateSelection();
    }

    public void ClearSlot(int index)
    {
        if (index < 0 || index >= slots.Length)
            return;

        slots[index] = null;

        if (hotbarUI != null)
            hotbarUI.Refresh();

        UpdateSelection();
    }

    public ItemData GetSlot(int index)
    {
        if (index < 0 || index >= slots.Length)
            return null;

        return slots[index];
    }
}
