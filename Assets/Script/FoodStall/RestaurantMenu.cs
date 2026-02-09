using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class MenuItem
{
    public ItemData food;
    public int price;
    public bool isAvailable = true;
}

public class RestaurantMenu : MonoBehaviour
{
    [Header("Today's Menu")]
    public List<MenuItem> menuItems = new List<MenuItem>();
    public int maxMenuSlots = 5;

    public event Action OnMenuChanged;

    public bool AddToMenu(ItemData food, int price)
    {
        if (food == null)
        {
            Debug.LogError("Food is null!");
            return false;
        }

        if (menuItems.Count >= maxMenuSlots)
        {
            Debug.LogWarning("Menu is full!");
            return false;
        }

        // Check if already in menu
        if (IsInMenu(food))
        {
            Debug.LogWarning($"{food.itemName} is already in the menu!");
            return false;
        }

        menuItems.Add(new MenuItem { food = food, price = price, isAvailable = true });
        OnMenuChanged?.Invoke();
        Debug.Log($"Added {food.itemName} to menu at ${price}");
        return true;
    }

    public bool RemoveFromMenu(ItemData food)
    {
        MenuItem item = menuItems.Find(m => m.food == food);
        if (item != null)
        {
            menuItems.Remove(item);
            OnMenuChanged?.Invoke();
            Debug.Log($"Removed {food.itemName} from menu");
            return true;
        }
        return false;
    }

    public bool RemoveFromMenu(int index)
    {
        if (index < 0 || index >= menuItems.Count)
            return false;

        menuItems.RemoveAt(index);
        OnMenuChanged?.Invoke();
        return true;
    }

    public bool IsInMenu(ItemData food)
    {
        return menuItems.Exists(m => m.food == food);
    }

    public MenuItem GetMenuItem(ItemData food)
    {
        return menuItems.Find(m => m.food == food);
    }

    public MenuItem GetRandomMenuItem()
    {
        if (menuItems.Count == 0)
            return null;

        // Filter available items
        List<MenuItem> available = menuItems.FindAll(m => m.isAvailable);
        if (available.Count == 0)
            return null;

        return available[UnityEngine.Random.Range(0, available.Count)];
    }

    public void ClearMenu()
    {
        menuItems.Clear();
        OnMenuChanged?.Invoke();
    }

    public bool HasSpace()
    {
        return menuItems.Count < maxMenuSlots;
    }
}