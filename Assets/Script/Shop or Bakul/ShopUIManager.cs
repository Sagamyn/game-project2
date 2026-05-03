using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the shop UI.
/// Refactored to support:
///   - "Buy Now"     → CartManager.BuyNow()   (instant purchase)
///   - "Add to Cart" → CartManager.AddToCart() (add to cart, checkout later)
///   - Cart Summary  → shows cart total + checkout button
/// </summary>
public class ShopUIManager : MonoBehaviour
{
    // ─── Root UI ─────────────────────────────────────────────────
    [Header("Root UI")]
    public GameObject shopUIRoot;

    // ─── Panels ──────────────────────────────────────────────────
    [Header("UI Panels")]
    public GameObject shopPanel;
    public GameObject buyPanel;
    public GameObject sellPanel;

    // ─── Shop Info ───────────────────────────────────────────────
    [Header("Shop Info")]
    public TextMeshProUGUI shopNameText;
    public TextMeshProUGUI playerMoneyText;

    // ─── Item Grid ───────────────────────────────────────────────
    [Header("Item Display")]
    public Transform itemGridContainer;
    public GameObject shopItemSlotPrefab;

    // ─── Selected Item Panel ─────────────────────────────────────
    [Header("Selected Item Panel")]
    public GameObject selectedItemPanel;
    public Image selectedItemIcon;
    public TextMeshProUGUI selectedItemName;
    public TextMeshProUGUI selectedItemDescription;
    public TextMeshProUGUI selectedItemPrice;   // shows price per unit (with discount if active)

    // Buy Now: instant purchase, no cart
    public Button buyNowButton;
    public TextMeshProUGUI buyNowButtonText;

    // Add to Cart: stacks in cart, checkout later
    public Button addToCartButton;
    public TextMeshProUGUI addToCartButtonText; // e.g. "Add to Cart (x2)"

    // Sell button (sell tab only)
    public Button sellButton;

    // ─── Cart Summary Panel ───────────────────────────────────────
    [Header("Cart Summary Panel")]
    public GameObject cartSummaryPanel;         // a small panel always visible while shop is open
    public TextMeshProUGUI cartItemCountText;   // e.g. "3 items"
    public TextMeshProUGUI cartTotalText;       // e.g. "Total: 45 coins"
    public Button checkoutButton;
    public Button clearCartButton;

    // ─── Tabs ────────────────────────────────────────────────────
    [Header("Tabs")]
    public Button buyTabButton;
    public Button sellTabButton;

    // ─── Audio ───────────────────────────────────────────────────
    [Header("Audio")]
    public AudioClip purchaseSound;
    public AudioClip addToCartSound;
    public AudioClip sellSound;
    public AudioClip errorSound;

    // ─── Private State ───────────────────────────────────────────
    private ShopNPC currentShop;
    private ShopItem selectedShopItem;
    private bool isBuyMode = true;
    private bool isShopOpen = false;

    private List<GameObject> spawnedSlots = new List<GameObject>();
    private AudioSource audioSource;

    // ─── Public Accessor (used by InventoryUI blocking check) ────
    public bool IsOpen() => isShopOpen;

    // ─────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (shopUIRoot == null) shopUIRoot = gameObject;

        shopUIRoot.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);

        // Wire up buttons
        if (buyNowButton != null)
            buyNowButton.onClick.AddListener(OnBuyNowClicked);

        if (addToCartButton != null)
            addToCartButton.onClick.AddListener(OnAddToCartClicked);

        if (sellButton != null)
            sellButton.onClick.AddListener(OnSellClicked);

        if (checkoutButton != null)
            checkoutButton.onClick.AddListener(OnCheckoutClicked);

        if (clearCartButton != null)
            clearCartButton.onClick.AddListener(OnClearCartClicked);

        if (buyTabButton != null)
            buyTabButton.onClick.AddListener(() => SwitchTab(true));

        if (sellTabButton != null)
            sellTabButton.onClick.AddListener(() => SwitchTab(false));

        // Hide cart summary until shop opens
        if (cartSummaryPanel != null)
            cartSummaryPanel.SetActive(false);

        if (selectedItemPanel != null)
            selectedItemPanel.SetActive(false);
    }

    void OnEnable()
    {
        // Subscribe to cart changes so the summary updates automatically
        if (CartManager.Instance != null)
        {
            CartManager.Instance.OnCartChanged += RefreshCartSummary;
            CartManager.Instance.OnCheckoutSuccess += OnCheckoutSuccess;
            CartManager.Instance.OnCheckoutFail += OnCheckoutFail;
        }
    }

    void OnDisable()
    {
        if (CartManager.Instance != null)
        {
            CartManager.Instance.OnCartChanged -= RefreshCartSummary;
            CartManager.Instance.OnCheckoutSuccess -= OnCheckoutSuccess;
            CartManager.Instance.OnCheckoutFail -= OnCheckoutFail;
        }
    }

    void Update()
    {
        if (shopPanel != null && shopPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            CloseShop();
    }

    // ─────────────────────────────────────────────────────────────
    // OPEN / CLOSE
    // ─────────────────────────────────────────────────────────────

    public void OpenShop(ShopNPC shop)
    {
        currentShop = shop;
        isShopOpen = true;

        if (shopUIRoot != null) shopUIRoot.SetActive(true);
        if (shopPanel != null) shopPanel.SetActive(true);

        if (cartSummaryPanel != null) cartSummaryPanel.SetActive(true);

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.LockMovement(true);

        if (shopNameText != null) shopNameText.text = shop.shopName;

        SwitchTab(true);
        UpdatePlayerMoneyDisplay();
        RefreshCartSummary();

        Debug.Log($"[ShopUI] Opened shop: {shop.shopName}");
    }

    public void CloseShop()
    {
        isShopOpen = false;

        if (shopUIRoot != null) shopUIRoot.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (cartSummaryPanel != null) cartSummaryPanel.SetActive(false);

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.LockMovement(false);

        ClearItemDisplay();
        currentShop = null;

        Debug.Log("[ShopUI] Shop closed.");
    }

    // ─────────────────────────────────────────────────────────────
    // TABS
    // ─────────────────────────────────────────────────────────────

    void SwitchTab(bool buyMode)
    {
        isBuyMode = buyMode;

        if (shopPanel != null && !shopPanel.activeSelf)
            shopPanel.SetActive(true);

        if (buyPanel != null) buyPanel.SetActive(buyMode);
        if (sellPanel != null) sellPanel.SetActive(!buyMode);

        UpdateTabVisuals();

        if (buyMode)
            DisplayBuyItems();
        else
            DisplaySellItems();

        // Hide selected item panel when switching tabs
        if (selectedItemPanel != null) selectedItemPanel.SetActive(false);
        selectedShopItem = null;
    }

    void UpdateTabVisuals()
    {
        if (buyTabButton != null)
        {
            ColorBlock c = buyTabButton.colors;
            c.normalColor = isBuyMode ? Color.green : Color.gray;
            buyTabButton.colors = c;
        }

        if (sellTabButton != null)
        {
            ColorBlock c = sellTabButton.colors;
            c.normalColor = !isBuyMode ? Color.green : Color.gray;
            sellTabButton.colors = c;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // ITEM GRID
    // ─────────────────────────────────────────────────────────────

    void DisplayBuyItems()
    {
        ClearItemDisplay();
        if (currentShop == null) return;

        foreach (ShopItem shopItem in currentShop.shopInventory)
        {
            if (!shopItem.IsAvailable("spring")) continue; // TODO: pass real season
            if (!shopItem.InStock()) continue;
            CreateItemSlot(shopItem, isSelling: false);
        }
    }

    void DisplaySellItems()
    {
        ClearItemDisplay();

        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
        if (inventory == null) return;

        foreach (var slot in inventory.items)
        {
            if (slot.item == null || slot.amount <= 0) continue;

            ShopItem tempShopItem = ScriptableObject.CreateInstance<ShopItem>();
            tempShopItem.item = slot.item;
            tempShopItem.sellPrice = slot.item.sellPrice;

            CreateItemSlot(tempShopItem, isSelling: true);
        }
    }

    void CreateItemSlot(ShopItem shopItem, bool isSelling)
    {
        if (shopItemSlotPrefab == null || itemGridContainer == null) return;

        GameObject slotObj = Instantiate(shopItemSlotPrefab, itemGridContainer);
        spawnedSlots.Add(slotObj);

        ShopItemSlot slot = slotObj.GetComponent<ShopItemSlot>();
        if (slot != null)
            slot.Setup(shopItem, this, isSelling);
    }

    void ClearItemDisplay()
    {
        foreach (GameObject slot in spawnedSlots)
            Destroy(slot);
        spawnedSlots.Clear();

        if (selectedItemPanel != null) selectedItemPanel.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────
    // SELECTED ITEM PANEL
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by ShopItemSlot when the player clicks an item.
    /// </summary>
    public void SelectItem(ShopItem shopItem, bool isSelling)
    {
        selectedShopItem = shopItem;

        if (selectedItemPanel != null) selectedItemPanel.SetActive(true);

        // Icon
        if (selectedItemIcon != null && shopItem.item.icon != null)
            selectedItemIcon.sprite = shopItem.item.icon;

        // Name
        if (selectedItemName != null)
            selectedItemName.text = shopItem.item.itemName;

        // Description
        if (selectedItemDescription != null)
            selectedItemDescription.text = GetItemDescription(shopItem.item);

        // Price (show discounted price if event is active)
        if (selectedItemPrice != null)
        {
            if (!isSelling)
            {
                int displayPrice = shopItem.buyPrice;

                // DISCOUNT: Show discounted Buy Now price if event active
                // Comment this block to always show base price:
                if (CartManager.Instance != null && CartManager.Instance.IsDiscountEventActive)
                    displayPrice = Mathf.Max(0, displayPrice - CartManager.Instance.ActiveDiscountAmount);
                // END DISCOUNT

                selectedItemPrice.text = $"{displayPrice} coins";
            }
            else
            {
                selectedItemPrice.text = $"Sell for {shopItem.sellPrice} coins";
            }
        }

        // Buy Now button (buy tab only)
        if (buyNowButton != null)
        {
            buyNowButton.gameObject.SetActive(!isSelling);
            if (buyNowButtonText != null)
                buyNowButtonText.text = "Buy Now";
        }

        // Add to Cart button (buy tab only)
        if (addToCartButton != null)
        {
            addToCartButton.gameObject.SetActive(!isSelling);
            RefreshAddToCartLabel();
        }

        // Sell button (sell tab only)
        if (sellButton != null)
        {
            sellButton.gameObject.SetActive(isSelling);
        }

        Debug.Log($"[ShopUI] Selected: {shopItem.item.itemName} | selling={isSelling}");
    }

    public void BuyItemDirectly(ShopItem shopItem, bool isSelling)
    {
        if (currentShop == null) return;
        bool success = currentShop.PurchaseItem(shopItem, 1);

        if (success)
        {
            UpdatePlayerMoneyDisplay();
            DisplayBuyItems();
            Debug.Log($"✅ Langsung beli: {shopItem.item.itemName}");
        }
        else
        {
            Debug.Log($"❌ Gagal membeli: {shopItem.item.itemName}");
        }
    }

    /// <summary>
    /// Updates the Add to Cart button label to show current quantity in cart.
    /// </summary>
    void RefreshAddToCartLabel()
    {
        if (addToCartButton == null || addToCartButtonText == null) return;
        if (selectedShopItem == null) return;

        int qty = CartManager.Instance != null
            ? CartManager.Instance.GetQuantityInCart(selectedShopItem)
            : 0;

        addToCartButtonText.text = qty > 0
            ? $"Add to Cart (x{qty})"
            : "Add to Cart";
    }

    string GetItemDescription(ItemData item)
    {
        if (item is SeedItem seed)
            return $"Plant this to grow {seed.cropData?.cropName ?? "crops"}";
        else if (item is ToolItem tool)
            return $"{tool.toolType} tool for farming";
        else
            return $"A {item.type} item";
    }

    // ─────────────────────────────────────────────────────────────
    // BUTTON HANDLERS
    // ─────────────────────────────────────────────────────────────

    void OnBuyNowClicked()
    {
        if (selectedShopItem == null || CartManager.Instance == null) return;

        bool success = CartManager.Instance.BuyNow(selectedShopItem, 1);

        if (success)
        {
            PlaySound(purchaseSound);
            UpdatePlayerMoneyDisplay();
            DisplayBuyItems();          // refresh grid (stock may have changed)
            RefreshCartSummary();
        }
        else
        {
            PlaySound(errorSound);
        }
    }

    void OnAddToCartClicked()
    {
        if (selectedShopItem == null || CartManager.Instance == null) return;

        CartManager.Instance.AddToCart(selectedShopItem);

        PlaySound(addToCartSound);
        RefreshAddToCartLabel();        // updates the "(x2)" counter on the button
        RefreshCartSummary();
    }

    void OnSellClicked()
    {
        if (selectedShopItem == null || currentShop == null) return;

        bool success = currentShop.SellItem(selectedShopItem.item, 1);

        if (success)
        {
            PlaySound(sellSound);
            UpdatePlayerMoneyDisplay();
            DisplaySellItems();
        }
        else
        {
            PlaySound(errorSound);
        }
    }

    void OnCheckoutClicked()
    {
        if (CartManager.Instance == null) return;
        CartManager.Instance.Checkout();
    }

    void OnClearCartClicked()
    {
        if (CartManager.Instance == null) return;
        CartManager.Instance.ClearCart();
    }

    // ─────────────────────────────────────────────────────────────
    // CART SUMMARY
    // ─────────────────────────────────────────────────────────────

    void RefreshCartSummary()
    {
        if (CartManager.Instance == null) return;

        int count = CartManager.Instance.GetTotalItemCount();
        int total = CartManager.Instance.GetTotalCost();

        if (cartItemCountText != null)
            cartItemCountText.text = count == 0 ? "Cart is empty" : $"{count} item{(count > 1 ? "s" : "")}";

        if (cartTotalText != null)
            cartTotalText.text = $"Total: {total} coins";

        // Disable checkout if cart is empty
        if (checkoutButton != null)
            checkoutButton.interactable = count > 0;

        // Also refresh Add to Cart label since cart changed
        RefreshAddToCartLabel();
    }

    // ─────────────────────────────────────────────────────────────
    // CART EVENT CALLBACKS
    // ─────────────────────────────────────────────────────────────

    void OnCheckoutSuccess()
    {
        PlaySound(purchaseSound);
        UpdatePlayerMoneyDisplay();
        DisplayBuyItems();
        Debug.Log("[ShopUI] Checkout successful!");
    }

    void OnCheckoutFail(string reason)
    {
        PlaySound(errorSound);
        Debug.LogWarning($"[ShopUI] Checkout failed: {reason}");
        // TODO: Show reason in a UI popup/toast if you have one
    }

    // ─────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────

    void UpdatePlayerMoneyDisplay()
    {
        if (playerMoneyText == null) return;

        PlayerMoney playerMoney = FindObjectOfType<PlayerMoney>();
        if (playerMoney != null)
            playerMoneyText.text = $"{playerMoney.CurrentMoney}";
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}