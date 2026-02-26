using UnityEngine;

/// <summary>
/// Defines an item that can be purchased in a shop
/// Links to existing ItemData with price information
/// </summary>
[CreateAssetMenu(fileName = "ShopItem", menuName = "Items/Shop Item", order = 3)]
public class ShopItem : ScriptableObject
{
    [Header("Item Reference")]
    public ItemData item; // The actual item (seed, ingredient, etc.)
    
    [Header("Shop Info")]
    public int buyPrice; // How much it costs to buy
    public int sellPrice; // How much player gets when selling (usually 50% of buy price)
    
    [Header("Stock")]
    public bool hasUnlimitedStock = true;
    public int stockAmount = 10; // If not unlimited
    
    [Header("Availability")]
    public bool availableInSpring = true;
    public bool availableInSummer = true;
    public bool availableInFall = true;
    public bool availableInWinter = true;
    
    [Header("Requirements")]
    public bool requiresUnlock = false;
    public string unlockCondition = ""; // e.g., "Reach level 5 farming"
    
    /// <summary>
    /// Check if item is currently available
    /// </summary>
    public bool IsAvailable(string currentSeason)
    {
        if (requiresUnlock)
        {
            // Check unlock condition here
            // For now, just return false if locked
            return false;
        }
        
        switch (currentSeason.ToLower())
        {
            case "spring": return availableInSpring;
            case "summer": return availableInSummer;
            case "fall": return availableInFall;
            case "winter": return availableInWinter;
            default: return true;
        }
    }
    
    /// <summary>
    /// Check if item is in stock
    /// </summary>
    public bool InStock()
    {
        return hasUnlimitedStock || stockAmount > 0;
    }
    

    public void Purchase(int amount = 1)
    {
        if (!hasUnlimitedStock)
        {
            stockAmount -= amount;
            if (stockAmount < 0) stockAmount = 0;
        }
    }
}