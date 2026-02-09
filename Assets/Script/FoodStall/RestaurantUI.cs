using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RestaurantUI : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory playerInventory;
    public GameObject panel;
    public UIAnimator uiAnimator;

    [Header("Menu Setup Panel")]
    public GameObject menuSetupPanel;
    public Transform menuSlotsParent;
    public MenuSlotUI menuSlotPrefab;
    public Transform playerFoodListParent;
    public FoodItemButton foodItemButtonPrefab;
    public Button openRestaurantButton;

    [Header("Orders Panel")]
    public GameObject ordersPanel;
    public Transform ordersListParent;
    public OrderItemUI orderItemPrefab;
    public TextMeshProUGUI statsText;

    private Restaurant currentRestaurant;
    private bool isOpen = false;
    private List<MenuSlotUI> menuSlots = new List<MenuSlotUI>();

    void Awake()
    {
        if (uiAnimator != null)
            uiAnimator.HideInstant();
        else if (panel != null)
            panel.SetActive(false);

        if (openRestaurantButton != null)
            openRestaurantButton.onClick.AddListener(OnOpenRestaurantClicked);
    }

    public void OpenMenuSetup(Restaurant restaurant)
    {
        currentRestaurant = restaurant;
        isOpen = true;

        // Show UI
        if (uiAnimator != null)
        {
            panel.SetActive(true);
            uiAnimator.Show();
        }
        else if (panel != null)
        {
            panel.SetActive(true);
        }

        // Show menu setup panel
        if (menuSetupPanel != null)
            menuSetupPanel.SetActive(true);

        if (ordersPanel != null)
            ordersPanel.SetActive(false);

        // Create menu slots
        CreateMenuSlots();

        // Show player's cooked food
        PopulatePlayerFoodList();

        Debug.Log("Menu setup opened");
    }

    public void OpenOrdersPanel(Restaurant restaurant, List<CustomerOrder> orders)
    {
        currentRestaurant = restaurant;
        isOpen = true;

        // Show UI
        if (uiAnimator != null)
        {
            panel.SetActive(true);
            uiAnimator.Show();
        }
        else if (panel != null)
        {
            panel.SetActive(true);
        }

        // Show orders panel
        if (menuSetupPanel != null)
            menuSetupPanel.SetActive(false);

        if (ordersPanel != null)
            ordersPanel.SetActive(true);

        // Populate orders
        PopulateOrdersList(orders);

        // Update stats
        UpdateStats();

        Debug.Log("Orders panel opened");
    }

    public void Close()
    {
        if (!isOpen) return;

        isOpen = false;

        // Hide UI
        if (uiAnimator != null)
            uiAnimator.Hide();
        else if (panel != null)
            panel.SetActive(false);

        currentRestaurant = null;

        PlayerMovement.Instance?.LockMovement(false);

        Debug.Log("Restaurant UI closed");
    }

    void CreateMenuSlots()
    {
        // Clear existing
        foreach (var slot in menuSlots)
            if (slot != null) Destroy(slot.gameObject);
        menuSlots.Clear();

        // Create slots
        for (int i = 0; i < currentRestaurant.menu.maxMenuSlots; i++)
        {
            MenuSlotUI slot = Instantiate(menuSlotPrefab, menuSlotsParent);
            slot.Initialize(i, this);
            menuSlots.Add(slot);

            // If item already in menu, show it
            if (i < currentRestaurant.menu.menuItems.Count)
            {
                var menuItem = currentRestaurant.menu.menuItems[i];
                slot.SetItem(menuItem.food, menuItem.price);
            }
        }
    }

    void PopulatePlayerFoodList()
    {
        // Clear existing
        foreach (Transform child in playerFoodListParent)
            Destroy(child.gameObject);

        // Show only cooked food
        foreach (var slot in playerInventory.items)
        {
            if (slot.item != null && slot.item.type == ItemType.CookedFood && slot.amount > 0)
            {
                FoodItemButton button = Instantiate(foodItemButtonPrefab, playerFoodListParent);
                button.Setup(slot.item, slot.amount, this);
            }
        }
    }

    void PopulateOrdersList(List<CustomerOrder> orders)
    {
        // Clear existing
        foreach (Transform child in ordersListParent)
            Destroy(child.gameObject);

        // Create order items
        foreach (var order in orders)
        {
            if (order.status == CustomerOrder.OrderStatus.Waiting)
            {
                OrderItemUI orderUI = Instantiate(orderItemPrefab, ordersListParent);
                orderUI.Setup(order, this);
            }
        }
    }

    public void AddToMenu(ItemData food, int price)
    {
        if (currentRestaurant != null)
        {
            bool success = currentRestaurant.menu.AddToMenu(food, price);
            if (success)
            {
                RefreshMenuSlots();
            }
        }
    }

    public void RemoveFromMenu(int slotIndex)
    {
        if (currentRestaurant != null)
        {
            currentRestaurant.menu.RemoveFromMenu(slotIndex);
            RefreshMenuSlots();
        }
    }

    void RefreshMenuSlots()
    {
        for (int i = 0; i < menuSlots.Count; i++)
        {
            if (i < currentRestaurant.menu.menuItems.Count)
            {
                var menuItem = currentRestaurant.menu.menuItems[i];
                menuSlots[i].SetItem(menuItem.food, menuItem.price);
            }
            else
            {
                menuSlots[i].Clear();
            }
        }
    }

    void OnOpenRestaurantClicked()
    {
        if (currentRestaurant != null)
        {
            currentRestaurant.StartRestaurant();
        }
    }

    public void ServeOrder(CustomerOrder order)
    {
        if (currentRestaurant != null)
        {
            currentRestaurant.ServeOrder(order);
            // Refresh orders list
            if (ordersPanel.activeSelf)
            {
                PopulateOrdersList(currentRestaurant.activeOrders);
            }
        }
    }

    void UpdateStats()
    {
        if (statsText != null && currentRestaurant != null)
        {
            statsText.text = $"Served: {currentRestaurant.customersServed} | Earnings: ${currentRestaurant.totalEarnings}";
        }
    }

    void Update()
    {
        // Close with ESC
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }

        // Update orders in real-time if panel is open
        if (isOpen && ordersPanel != null && ordersPanel.activeSelf)
        {
            UpdateStats();
        }
    }
}