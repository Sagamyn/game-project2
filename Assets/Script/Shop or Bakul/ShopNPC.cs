using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Shop NPC that sells items to the player
/// Can be used for seed shop, tool shop, general store, etc.
/// </summary>
public class ShopNPC : Interactable // Changed from IInteractable to Interactable
{
    [Header("Shop Info")]
    public string shopName = "General Store";
    public string shopkeeperName = "Pierre";
    
    [Header("Shop Inventory")]
    public List<ShopItem> shopInventory = new List<ShopItem>();
    
    [Header("Interaction")]
    public float interactionRadius = 1.5f;
    
    [Header("UI")]
    public GameObject shopUIManager; // Reference to ShopUI GameObject
    
    [Header("Audio")]
    public AudioClip openShopSound;
    public AudioClip closeShopSound;
    
    private ShopUIManager shopUI;

    void Start()
    {
        if (shopUIManager != null)
        {
            shopUI = shopUIManager.GetComponent<ShopUIManager>();
        }
    }

    public override void Interact()
    {
        OpenShop();
    }

    public void OpenShop()
    {
        if (shopUI != null)
        {
            shopUI.OpenShop(this);
            
            if (openShopSound != null)
            {
                AudioSource.PlayClipAtPoint(openShopSound, transform.position);
            }
            
            Debug.Log($"🏪 Opened {shopName}");
        }
        else
        {
            Debug.LogError("Shop UI Manager not assigned!");
        }
    }

    public void CloseShop()
    {
        if (shopUI != null)
        {
            shopUI.CloseShop();
            
            if (closeShopSound != null)
            {
                AudioSource.PlayClipAtPoint(closeShopSound, transform.position);
            }
        }
    }

    /// <summary>
    /// Check if player can afford an item
    /// </summary>
    public bool CanAfford(ShopItem shopItem, int amount = 1)
    {
        PlayerMoney playerMoney = FindObjectOfType<PlayerMoney>();
        if (playerMoney == null) return false;
        
        int totalCost = shopItem.buyPrice * amount;
        return playerMoney.HasMoney(totalCost);
    }

    /// <summary>
    /// Purchase an item
    /// </summary>
    public bool PurchaseItem(ShopItem shopItem, int amount = 1)
    {
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
        PlayerMoney playerMoney = FindObjectOfType<PlayerMoney>();
        
        if (inventory == null || playerMoney == null)
        {
            Debug.LogError("Player inventory or money not found!");
            return false;
        }

        int totalCost = shopItem.buyPrice * amount;
        
        // Check if player can afford it
        if (!playerMoney.HasMoney(totalCost))
        {
            Debug.Log($"❌ Not enough money! Need ${totalCost}, have ${playerMoney.CurrentMoney}");
            return false;
        }

        // Check if in stock
        if (!shopItem.InStock())
        {
            Debug.Log($"❌ {shopItem.item.itemName} is out of stock!");
            return false;
        }

        // Check if inventory has space (check for empty slot)
        if (!HasEmptySlot(inventory))
        {
            Debug.Log($"❌ Inventory full!");
            return false;
        }

        // Process purchase
        if (!playerMoney.RemoveMoney(totalCost))
        {
            Debug.Log($"❌ Failed to remove money!");
            return false;
        }
        
        inventory.AddItem(shopItem.item, amount);
        shopItem.Purchase(amount);

        Debug.Log($"✅ Purchased {amount}x {shopItem.item.itemName} for ${totalCost}");
        return true;
    }

    /// <summary>
    /// Sell an item to the shop
    /// </summary>
    public bool SellItem(ItemData item, int amount = 1)
    {
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
        PlayerMoney playerMoney = FindObjectOfType<PlayerMoney>();
        
        if (inventory == null || playerMoney == null) return false;

        // Check if player has the item
        if (!inventory.HasItem(item, amount))
        {
            Debug.Log($"❌ You don't have {amount}x {item.itemName}");
            return false;
        }

        // Find shop item to get sell price
        ShopItem shopItem = shopInventory.Find(si => si.item == item);
        int sellPrice = shopItem != null ? shopItem.sellPrice : item.sellPrice; // Use ItemData.sellPrice
        int totalValue = sellPrice * amount;

        // Process sale
        inventory.ConsumeItem(item, amount);
        playerMoney.AddMoney(totalValue);

        Debug.Log($"✅ Sold {amount}x {item.itemName} for ${totalValue}");
        return true;
    }

    // Helper method to check for empty slot
    private bool HasEmptySlot(PlayerInventory inventory)
    {
        foreach (var slot in inventory.items)
        {
            if (slot.IsEmpty)
                return true;
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}