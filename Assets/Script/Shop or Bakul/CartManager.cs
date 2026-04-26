using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Singleton that manages the shopping cart.
/// Handles add/remove items, quantity changes, discount events, and checkout.
///
/// HOW TO USE:
///   CartManager.Instance.AddToCart(shopItem);
///   CartManager.Instance.Checkout();
///
/// DISCOUNT EVENT:
///   To activate:   CartManager.Instance.ActivateDiscountEvent(5);   // -5 coins per item
///   To deactivate: CartManager.Instance.DeactivateDiscountEvent();
/// </summary>
public class CartManager : MonoBehaviour
{
    // ─── Singleton ───────────────────────────────────────────────
    public static CartManager Instance { get; private set; }

    // ─── References ──────────────────────────────────────────────
    [Header("References")]
    public PlayerInventory playerInventory;
    public PlayerMoney playerMoney;

    // ─── Cart State ──────────────────────────────────────────────
    private List<CartItem> cartItems = new List<CartItem>();

    // ─── Discount State ──────────────────────────────────────────
    // DISCOUNT: This block controls the flat discount event.
    // To fully disable discount feature: comment out everything inside
    // ActivateDiscountEvent() and DeactivateDiscountEvent() methods below.
    private bool isDiscountEventActive = false;
    private int activeDiscountAmount = 0;     // flat coins off per item unit

    // ─── Events (for UI to subscribe to) ─────────────────────────
    public event Action OnCartChanged;          // fires whenever cart is modified
    public event Action<string> OnCheckoutFail; // fires with reason message on failure
    public event Action OnCheckoutSuccess;      // fires after successful checkout

    // ─────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Auto-find references if not assigned in Inspector
        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerInventory>();

        if (playerMoney == null)
            playerMoney = FindObjectOfType<PlayerMoney>();
    }

    // ─────────────────────────────────────────────────────────────
    // CART OPERATIONS
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Add one unit of a ShopItem to the cart.
    /// If the item is already in the cart, increments quantity instead.
    /// </summary>
    public void AddToCart(ShopItem shopItem)
    {
        if (shopItem == null)
        {
            Debug.LogWarning("[CartManager] Tried to add null ShopItem to cart.");
            return;
        }

        // Check stock availability
        if (!shopItem.InStock())
        {
            Debug.LogWarning($"[CartManager] {shopItem.item.itemName} is out of stock.");
            OnCheckoutFail?.Invoke($"{shopItem.item.itemName} is out of stock!");
            return;
        }

        // Check if already in cart → stack quantity
        CartItem existing = GetCartItem(shopItem);
        if (existing != null)
        {
            existing.quantity++;
            Debug.Log($"[CartManager] {shopItem.item.itemName} quantity → {existing.quantity}");
        }
        else
        {
            // Create new cart entry
            CartItem newEntry = new CartItem(shopItem, 1);

            // DISCOUNT: Apply active discount to new entry if event is on
            // Comment out this block to disable discount application on add:
            if (isDiscountEventActive)
            {
                ApplyDiscountToEntry(newEntry);
            }
            // END DISCOUNT

            cartItems.Add(newEntry);
            Debug.Log($"[CartManager] Added {shopItem.item.itemName} to cart.");
        }

        OnCartChanged?.Invoke();
    }

    /// <summary>
    /// Remove one unit from cart. Removes the entry entirely if quantity reaches 0.
    /// </summary>
    public void RemoveFromCart(ShopItem shopItem)
    {
        CartItem entry = GetCartItem(shopItem);
        if (entry == null)
        {
            Debug.LogWarning($"[CartManager] {shopItem.item.itemName} not found in cart.");
            return;
        }

        entry.quantity--;

        if (entry.quantity <= 0)
        {
            cartItems.Remove(entry);
            Debug.Log($"[CartManager] Removed {shopItem.item.itemName} from cart.");
        }
        else
        {
            Debug.Log($"[CartManager] {shopItem.item.itemName} quantity → {entry.quantity}");
        }

        OnCartChanged?.Invoke();
    }

    /// <summary>
    /// Remove a ShopItem from cart completely regardless of quantity.
    /// </summary>
    public void RemoveAllFromCart(ShopItem shopItem)
    {
        CartItem entry = GetCartItem(shopItem);
        if (entry == null) return;

        cartItems.Remove(entry);
        Debug.Log($"[CartManager] Fully removed {shopItem.item.itemName} from cart.");
        OnCartChanged?.Invoke();
    }

    /// <summary>
    /// Set exact quantity for an item in cart.
    /// Passing qty <= 0 removes the item entirely.
    /// </summary>
    public void SetQuantity(ShopItem shopItem, int qty)
    {
        if (qty <= 0)
        {
            RemoveAllFromCart(shopItem);
            return;
        }

        CartItem entry = GetCartItem(shopItem);
        if (entry == null)
        {
            AddToCart(shopItem);
            entry = GetCartItem(shopItem);
            if (entry == null) return;
        }

        entry.quantity = qty;
        Debug.Log($"[CartManager] Set {shopItem.item.itemName} qty to {qty}.");
        OnCartChanged?.Invoke();
    }

    /// <summary>
    /// Clear all items from the cart.
    /// </summary>
    public void ClearCart()
    {
        cartItems.Clear();
        Debug.Log("[CartManager] Cart cleared.");
        OnCartChanged?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────
    // CHECKOUT
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Attempt to buy all items currently in the cart.
    /// Deducts money and adds items to player inventory on success.
    /// Fires OnCheckoutSuccess or OnCheckoutFail accordingly.
    /// </summary>
    public void Checkout()
    {
        if (cartItems.Count == 0)
        {
            Debug.LogWarning("[CartManager] Cart is empty.");
            OnCheckoutFail?.Invoke("Your cart is empty!");
            return;
        }

        // ── Pre-validate everything before touching money/inventory ──

        int totalCost = GetTotalCost();

        // Check if player has enough money
        if (!playerMoney.HasMoney(totalCost))
        {
            string msg = $"Not enough coins! Need {totalCost}, have {playerMoney.CurrentMoney}.";
            Debug.LogWarning($"[CartManager] {msg}");
            OnCheckoutFail?.Invoke(msg);
            return;
        }

        // Check if inventory has enough empty slots
        int slotsNeeded = CountSlotsNeeded();
        int slotsAvailable = CountEmptySlots();
        if (slotsAvailable < slotsNeeded)
        {
            string msg = $"Not enough inventory space! Need {slotsNeeded} slot(s).";
            Debug.LogWarning($"[CartManager] {msg}");
            OnCheckoutFail?.Invoke(msg);
            return;
        }

        // Check stock for each item
        foreach (CartItem entry in cartItems)
        {
            if (!entry.shopItem.InStock())
            {
                string msg = $"{entry.shopItem.item.itemName} is out of stock!";
                Debug.LogWarning($"[CartManager] {msg}");
                OnCheckoutFail?.Invoke(msg);
                return;
            }
        }

        // ── All checks passed — process the purchase ──

        playerMoney.RemoveMoney(totalCost);

        foreach (CartItem entry in cartItems)
        {
            playerInventory.AddItem(entry.shopItem.item, entry.quantity);

            // Reduce stock if not unlimited
            if (!entry.shopItem.hasUnlimitedStock)
                entry.shopItem.Purchase(entry.quantity);

            Debug.Log($"[CartManager] Purchased {entry.quantity}x {entry.shopItem.item.itemName} " +
                      $"for {entry.TotalPrice()} coins (saved {entry.TotalSavings()}).");
        }

        ClearCart();
        OnCheckoutSuccess?.Invoke();
        Debug.Log($"[CartManager] Checkout successful! Total paid: {totalCost}");
    }

    /// <summary>
    /// Instantly buy a single item without going through the cart.
    /// Mirrors the original ShopNPC.PurchaseItem() behavior.
    /// </summary>
    public bool BuyNow(ShopItem shopItem, int amount = 1)
    {
        if (shopItem == null) return false;

        int unitPrice = shopItem.buyPrice;

        // DISCOUNT: Apply flat discount to Buy Now price if event is active.
        // Comment out this block to make Buy Now ignore discounts:
        if (isDiscountEventActive)
        {
            unitPrice = Mathf.Max(0, unitPrice - activeDiscountAmount);
        }
        // END DISCOUNT

        int totalCost = unitPrice * amount;

        if (!playerMoney.HasMoney(totalCost))
        {
            string msg = $"Not enough coins! Need {totalCost}, have {playerMoney.CurrentMoney}.";
            Debug.LogWarning($"[CartManager] BuyNow failed — {msg}");
            OnCheckoutFail?.Invoke(msg);
            return false;
        }

        if (!shopItem.InStock())
        {
            string msg = $"{shopItem.item.itemName} is out of stock!";
            OnCheckoutFail?.Invoke(msg);
            return false;
        }

        playerMoney.RemoveMoney(totalCost);
        playerInventory.AddItem(shopItem.item, amount);

        if (!shopItem.hasUnlimitedStock)
            shopItem.Purchase(amount);

        Debug.Log($"[CartManager] BuyNow: {amount}x {shopItem.item.itemName} for {totalCost} coins.");
        OnCheckoutSuccess?.Invoke();
        return true;
    }

    // ─────────────────────────────────────────────────────────────
    // DISCOUNT EVENT
    // ─────────────────────────────────────────────────────────────

    // DISCOUNT: ActivateDiscountEvent / DeactivateDiscountEvent
    // Comment out the contents of these two methods to fully disable discount feature.

    /// <summary>
    /// Activate a flat discount event. All items currently in cart
    /// and newly added items will have [discountAmount] coins deducted per unit.
    /// </summary>
    public void ActivateDiscountEvent(int discountAmount)
    {
        // DISCOUNT START — comment this whole block to disable
        isDiscountEventActive = true;
        activeDiscountAmount = Mathf.Max(0, discountAmount);

        // Apply discount retroactively to items already in cart
        foreach (CartItem entry in cartItems)
        {
            ApplyDiscountToEntry(entry);
        }

        Debug.Log($"[CartManager] Discount event activated: -{activeDiscountAmount} coins per item.");
        OnCartChanged?.Invoke();
        // DISCOUNT END
    }

    /// <summary>
    /// Deactivate the discount event and remove discounts from all cart entries.
    /// </summary>
    public void DeactivateDiscountEvent()
    {
        // DISCOUNT START — comment this whole block to disable
        isDiscountEventActive = false;
        activeDiscountAmount = 0;

        // Remove discount from all current cart entries
        foreach (CartItem entry in cartItems)
        {
            entry.flatDiscount = 0;
            entry.isDiscounted = false;
        }

        Debug.Log("[CartManager] Discount event deactivated.");
        OnCartChanged?.Invoke();
        // DISCOUNT END
    }

    /// <summary>
    /// Apply the active flat discount to a single cart entry.
    /// </summary>
    private void ApplyDiscountToEntry(CartItem entry)
    {
        // DISCOUNT: Comment this method body to skip discount application
        entry.flatDiscount = activeDiscountAmount;
        entry.isDiscounted = true;
    }

    // ─────────────────────────────────────────────────────────────
    // QUERIES / HELPERS
    // ─────────────────────────────────────────────────────────────

    /// <summary>Read-only snapshot of cart contents.</summary>
    public IReadOnlyList<CartItem> GetCartItems() => cartItems.AsReadOnly();

    /// <summary>Total number of item units across all entries.</summary>
    public int GetTotalItemCount()
    {
        int total = 0;
        foreach (CartItem entry in cartItems)
            total += entry.quantity;
        return total;
    }

    /// <summary>Total cost of the entire cart after discounts.</summary>
    public int GetTotalCost()
    {
        int total = 0;
        foreach (CartItem entry in cartItems)
            total += entry.TotalPrice();
        return total;
    }

    /// <summary>Total savings from discounts across the whole cart.</summary>
    public int GetTotalSavings()
    {
        int savings = 0;
        foreach (CartItem entry in cartItems)
            savings += entry.TotalSavings();
        return savings;
    }

    /// <summary>Returns true if a ShopItem is already in the cart.</summary>
    public bool IsInCart(ShopItem shopItem) => GetCartItem(shopItem) != null;

    /// <summary>Returns quantity of a ShopItem in cart (0 if not present).</summary>
    public int GetQuantityInCart(ShopItem shopItem)
    {
        CartItem entry = GetCartItem(shopItem);
        return entry != null ? entry.quantity : 0;
    }

    public bool IsDiscountEventActive => isDiscountEventActive;
    public int ActiveDiscountAmount => activeDiscountAmount;

    // ─── Private Helpers ─────────────────────────────────────────

    private CartItem GetCartItem(ShopItem shopItem)
    {
        return cartItems.Find(e => e.shopItem == shopItem);
    }

    /// <summary>
    /// Count how many distinct inventory slots the cart checkout will consume.
    /// Stackable items that already exist in inventory don't need a new slot.
    /// </summary>
    private int CountSlotsNeeded()
    {
        int needed = 0;
        foreach (CartItem entry in cartItems)
        {
            if (entry.shopItem.item.stackable &&
                playerInventory.HasItem(entry.shopItem.item))
            {
                // Will stack into existing slot — no new slot needed
                continue;
            }
            needed++;
        }
        return needed;
    }

    private int CountEmptySlots()
    {
        int empty = 0;
        foreach (var slot in playerInventory.items)
        {
            if (slot.IsEmpty) empty++;
        }
        return empty;
    }
}