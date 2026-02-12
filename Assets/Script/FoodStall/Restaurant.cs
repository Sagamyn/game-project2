using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Restaurant : Interactable
{
    [Header("Restaurant Status")]
    public bool isOpen = false;
    public RestaurantState state = RestaurantState.Closed;

    [Header("Components")]
    public RestaurantMenu menu;
    public PlayerInventory playerInventory;
    public PlayerMoney playerMoney;

    [Header("Customer Spawning")]
    public CustomerNPC customerPrefab;
    public Transform spawnPoint;
    public Transform[] seatPositions;
    public float customerSpawnInterval = 10f;
    public int maxCustomers = 4;

    [Header("UI")]
    public RestaurantUI restaurantUI;

    [Header("Stats")]
    public int customersServed = 0;
    public int totalEarnings = 0;

    public List<CustomerNPC> activeCustomers = new List<CustomerNPC>();
    public List<CustomerOrder> activeOrders = new List<CustomerOrder>();
    private Coroutine customerSpawner;

    public enum RestaurantState
    {
        Closed,
        MenuSetup,
        Open
    }

    protected override void Awake()
    {
        base.Awake();

        if (menu == null)
            menu = GetComponent<RestaurantMenu>();

        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerInventory>();

        if (playerMoney == null)
            playerMoney = FindObjectOfType<PlayerMoney>();
    }

    public override void Interact()
    {
        if (state == RestaurantState.Closed || state == RestaurantState.MenuSetup)
        {
            OpenMenuSetup();
        }
        else if (state == RestaurantState.Closed || state == RestaurantState.MenuSetup)
        {
            CloseMenuSetup();
        }
        else if (state == RestaurantState.Open)
        {
            OpenOrdersPanel();
        }
    }

    void OpenMenuSetup()
    {
        if (restaurantUI == null)
        {
            Debug.LogError("RestaurantUI not assigned!");
            return;
        }

        state = RestaurantState.MenuSetup;
        restaurantUI.OpenMenuSetup(this);

        PlayerMovement.Instance?.LockMovement(true);
    }

    void CloseMenuSetup()
    {
        if (restaurantUI == null) return;

        state = RestaurantState.Closed;
        restaurantUI.Close();

        PlayerMovement.Instance?.LockMovement(false);
    }

    public void StartRestaurant()
    {
        if (menu.menuItems.Count == 0)
        {
            Debug.LogWarning("Menu is empty! Add items first.");
            return;
        }

        state = RestaurantState.Open;
        isOpen = true;

        Debug.Log(" Restaurant is now OPEN!");

        // Start spawning customers
        if (customerSpawner == null)
            customerSpawner = StartCoroutine(SpawnCustomers());

        // Close UI
        if (restaurantUI != null)
            restaurantUI.Close();

        PlayerMovement.Instance?.LockMovement(false);
    }

    public void CloseRestaurant()
    {
        Debug.Log(" Starting restaurant closure...");
        
        state = RestaurantState.Closed;
        isOpen = false;

        // Stop spawning NEW customers
        if (customerSpawner != null)
        {
            StopCoroutine(customerSpawner);
            customerSpawner = null;
            Debug.Log("✓ Stopped customer spawning");
        }

        // Get count before clearing
        int customerCount = activeCustomers.Count;
        Debug.Log($" Found {customerCount} active customers to remove");

        // Make all EXISTING customers leave
        // Use a copy of the list to avoid modification during iteration
        List<CustomerNPC> customersSnapshot = new List<CustomerNPC>(activeCustomers);
        
        foreach (var customer in customersSnapshot)
        {
            if (customer != null)
            {
                Debug.Log($" Telling {customer.customerName} to leave...");
                customer.LeaveImmediately();
            }
        }

        // Clear lists immediately (customers will handle their own cleanup)
        activeCustomers.Clear();
        activeOrders.Clear();
        Debug.Log("✓ Cleared customer lists");

        // Clear menu
        menu.ClearMenu();

        // Close UI
        if (restaurantUI != null)
            restaurantUI.Close();
            
        PlayerMovement.Instance?.LockMovement(false);
        
        Debug.Log(" Restaurant CLOSED successfully");
    }

    void OpenOrdersPanel()
    {
        if (restaurantUI == null) return;

        restaurantUI.OpenOrdersPanel(this, activeOrders);
        PlayerMovement.Instance?.LockMovement(true);
    }

    IEnumerator SpawnCustomers()
    {
        while (isOpen)
        {
            yield return new WaitForSeconds(customerSpawnInterval);

            // Check if we have space
            if (activeCustomers.Count < maxCustomers && HasAvailableSeat())
            {
                SpawnCustomer();
            }
        }
    }

    void SpawnCustomer()
    {
        // Get random menu item
        MenuItem menuItem = menu.GetRandomMenuItem();
        if (menuItem == null)
        {
            Debug.LogWarning("No menu items available!");
            return;
        }

        // Get available seat
        Transform seat = GetAvailableSeat();
        if (seat == null)
        {
            Debug.LogWarning("No seats available!");
            return;
        }

        // Spawn customer
        CustomerNPC customer = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        customer.Initialize(this, menuItem, seat);

        activeCustomers.Add(customer);
        activeOrders.Add(customer.currentOrder);

        Debug.Log($"✓ Customer spawned! Ordered: {menuItem.food.itemName}");
    }

    Transform GetAvailableSeat()
    {
        foreach (var seat in seatPositions)
        {
            bool occupied = activeCustomers.Exists(c => c != null && c.seatPosition == seat);
            if (!occupied)
                return seat;
        }
        return null;
    }

    bool HasAvailableSeat()
    {
        return GetAvailableSeat() != null;
    }

    public void ServeOrder(CustomerOrder order)
    {
        // Find customer with this order
        CustomerNPC customer = activeCustomers.Find(c => c != null && c.currentOrder.orderId == order.orderId);
        if (customer == null)
        {
            Debug.LogError("Customer not found!");
            return;
        }

        // Check if player has the food
        if (!playerInventory.HasItem(order.orderedFood))
        {
            Debug.LogWarning("Don't have that food!");
            return;
        }

        // Remove from inventory
        playerInventory.ConsumeItem(order.orderedFood, 1);

        // Give to customer
        customer.ReceiveFood(order.orderedFood);

        Debug.Log($"✓ Served {order.orderedFood.itemName} to customer!");
    }

    public void OnCustomerLeft(CustomerNPC customer, bool paid)
    {
        if (customer == null) return;
        
        if (paid)
        {
            // Give money to player
            playerMoney.AddMoney(customer.currentOrder.price);
            customersServed++;
            totalEarnings += customer.currentOrder.price;

            Debug.Log($" Earned ${customer.currentOrder.price}! Total: ${totalEarnings}");
        }
        else
        {
            Debug.LogWarning($"❌ {customer.customerName} left without paying!");
        }

        // Remove from lists
        activeCustomers.Remove(customer);
        if (customer.currentOrder != null)
            activeOrders.Remove(customer.currentOrder);
            
        Debug.Log($" Customers remaining: {activeCustomers.Count}");
    }
}