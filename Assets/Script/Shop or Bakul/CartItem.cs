using UnityEngine;
using System;

/// <summary>
/// Represents a single entry in the shopping cart.
/// Holds the ShopItem reference, quantity, and discount info.
/// </summary>
[Serializable]
public class CartItem
{
    // ─── Core Data ───────────────────────────────────────────────
    public ShopItem shopItem;       // Reference to the ShopItem ScriptableObject
    public int quantity;            // How many units in cart

    // ─── Discount ────────────────────────────────────────────────
    // DISCOUNT: Toggle discount on/off by commenting the block in CartManager.ApplyDiscount()
    public int flatDiscount;        // Flat coin discount applied to this item (e.g. -5 coins per unit)
    public bool isDiscounted;       // Whether discount is currently active on this entry

    // ─── Constructor ─────────────────────────────────────────────
    public CartItem(ShopItem item, int qty = 1)
    {
        shopItem = item;
        quantity = qty;
        flatDiscount = 0;
        isDiscounted = false;
    }

    // ─── Price Helpers ───────────────────────────────────────────

    /// <summary>
    /// Price per unit after discount (never goes below 0).
    /// </summary>
    public int PricePerUnit()
    {
        int base_price = shopItem.buyPrice;
        int discounted = base_price - flatDiscount;
        return Mathf.Max(0, discounted);
    }

    /// <summary>
    /// Total price for this cart entry (unit price × quantity).
    /// </summary>
    public int TotalPrice()
    {
        return PricePerUnit() * quantity;
    }

    /// <summary>
    /// Total savings from discount for this entry.
    /// </summary>
    public int TotalSavings()
    {
        return flatDiscount * quantity;
    }
}