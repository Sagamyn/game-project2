using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the shop UI - displays items, handles purchases
/// </summary>
public class ShopUIManager : MonoBehaviour
{
    [Header("Root UI")]
    public GameObject shopUIRoot;

    [Header("UI Panels")]
    public GameObject shopPanel;
    public GameObject buyPanel;
    public GameObject sellPanel;
    
    [Header("Shop Info")]
    public TextMeshProUGUI shopNameText;
    public TextMeshProUGUI playerMoneyText;
    
    [Header("Item Display")]
    public Transform itemGridContainer;
    public GameObject shopItemSlotPrefab;
    
    [Header("Selected Item Panel")]
    public GameObject selectedItemPanel;
    public Image selectedItemIcon;
    public TextMeshProUGUI selectedItemName;
    public TextMeshProUGUI selectedItemDescription;
    public TextMeshProUGUI selectedItemPrice;
    public Button buyButton;
    public Button buyMultipleButton;
    public TextMeshProUGUI buyButtonText;
    
    [Header("Tabs")]
    public Button buyTabButton;
    public Button sellTabButton;
    
    [Header("Audio")]
    public AudioClip purchaseSound;
    public AudioClip sellSound;
    public AudioClip errorSound;
    
    private ShopNPC currentShop;
    private ShopItem selectedShopItem;
    private List<GameObject> spawnedSlots = new List<GameObject>();
    private bool isBuyMode = true;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // If not assigned in inspector, default to this GameObject as the root canvas/container.
        if (shopUIRoot == null)
            shopUIRoot = gameObject;
        
        if (shopUIRoot != null)
            shopUIRoot.SetActive(false);
        
        if (shopPanel != null)
            shopPanel.SetActive(false);
        
        // Setup button listeners
        if (buyButton != null)
            buyButton.onClick.AddListener(() => BuySelectedItem(1));
        
        if (buyMultipleButton != null)
            buyMultipleButton.onClick.AddListener(() => BuySelectedItem(5));
        
        if (buyTabButton != null)
            buyTabButton.onClick.AddListener(() => SwitchTab(true));
        
        if (sellTabButton != null)
            sellTabButton.onClick.AddListener(() => SwitchTab(false));
    }

    public void OpenShop(ShopNPC shop)
    {
        currentShop = shop;

        if (shopUIRoot != null)
            shopUIRoot.SetActive(true);
        
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }
        
        // Lock player movement
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
            player.LockMovement(true);
        
        // Set shop name
        if (shopNameText != null)
            shopNameText.text = shop.shopName;
        
        // Display items
        SwitchTab(true); // Start in buy mode
        
        UpdatePlayerMoneyDisplay();
    }

    public void CloseShop()
    {
        if (shopUIRoot != null)
            shopUIRoot.SetActive(false);

        if (shopPanel != null)
            shopPanel.SetActive(false);
        
        // Unlock player movement
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
            player.LockMovement(false);
        
        ClearItemDisplay();
        currentShop = null;
    }

    void SwitchTab(bool buyMode)
    {
        isBuyMode = buyMode;

        if (shopPanel != null && !shopPanel.activeSelf)
            shopPanel.SetActive(true);
        
        if (buyPanel != null)
            buyPanel.SetActive(buyMode);
        
        if (sellPanel != null)
            sellPanel.SetActive(!buyMode);
        
        // Update tab buttons
        UpdateTabVisuals();
        
        // Display appropriate items
        if (buyMode)
            DisplayBuyItems();
        else
            DisplaySellItems();
    }

    void UpdateTabVisuals()
    {
        if (buyTabButton != null)
        {
            ColorBlock colors = buyTabButton.colors;
            colors.normalColor = isBuyMode ? Color.green : Color.gray;
            buyTabButton.colors = colors;
        }
        
        if (sellTabButton != null)
        {
            ColorBlock colors = sellTabButton.colors;
            colors.normalColor = !isBuyMode ? Color.green : Color.gray;
            sellTabButton.colors = colors;
        }
    }

    void DisplayBuyItems()
    {
        ClearItemDisplay();
        
        if (currentShop == null) return;
        
        foreach (ShopItem shopItem in currentShop.shopInventory)
        {
            // Check if available
            if (!shopItem.IsAvailable("spring")) // TODO: Get actual season
                continue;
            
            if (!shopItem.InStock())
                continue;
            
            CreateItemSlot(shopItem);
        }
    }

    void DisplaySellItems()
    {
        ClearItemDisplay();
        
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
        if (inventory == null) return;
        
        // Display all items in player inventory that can be sold
        foreach (var slot in inventory.items)
        {
            if (slot.item != null && slot.amount > 0)
            {
                // Create a temporary shop item for selling
                ShopItem tempShopItem = ScriptableObject.CreateInstance<ShopItem>();
                tempShopItem.item = slot.item;
                tempShopItem.sellPrice = slot.item.sellPrice; // Use ItemData's sellPrice
                
                CreateItemSlot(tempShopItem, true);
            }
        }
    }

    void CreateItemSlot(ShopItem shopItem, bool isSelling = false)
    {
        if (shopItemSlotPrefab == null || itemGridContainer == null)
            return;
        
        GameObject slotObj = Instantiate(shopItemSlotPrefab, itemGridContainer);
        spawnedSlots.Add(slotObj);
        
        // Setup slot visuals
        ShopItemSlot slot = slotObj.GetComponent<ShopItemSlot>();
        if (slot != null)
        {
            slot.Setup(shopItem, this, isSelling);
        }
    }

    public void SelectItem(ShopItem shopItem, bool isSelling)
    {
        selectedShopItem = shopItem;
        
        if (selectedItemPanel != null)
            selectedItemPanel.SetActive(true);
        
        // Display item info
        if (selectedItemIcon != null && shopItem.item.icon != null)
            selectedItemIcon.sprite = shopItem.item.icon;
        
        if (selectedItemName != null)
            selectedItemName.text = shopItem.item.itemName;
        
        // Note: ItemData doesn't have a description field
        // You can add one or leave it blank
        if (selectedItemDescription != null)
            selectedItemDescription.text = GetItemDescription(shopItem.item);
        
        if (selectedItemPrice != null)
        {
            int price = isSelling ? shopItem.sellPrice : shopItem.buyPrice;
            string action = isSelling ? "Sell for" : "";
            selectedItemPrice.text = $"{action} ${price}";
        }
        
        // Update button
        if (buyButton != null)
        {
            buyButton.gameObject.SetActive(!isSelling);
        }
        
        if (buyButtonText != null)
        {
            buyButtonText.text = isSelling ? "Sell" : "Buy";
        }
    }

    // Helper to get description based on item type
    string GetItemDescription(ItemData item)
    {
        if (item is SeedItem seed)
        {
            return $"Plant this to grow {seed.cropData?.cropName ?? "crops"}";
        }
        else if (item is ToolItem tool)
        {
            return $"{tool.toolType} tool for farming";
        }
        else
        {
            return $"A {item.type} item"; // Fallback
        }
    }

    void BuySelectedItem(int amount)
    {
        if (selectedShopItem == null || currentShop == null)
            return;
        
        if (isBuyMode)
        {
            // Buy from shop
            bool success = currentShop.PurchaseItem(selectedShopItem, amount);
            
            if (success)
            {
                PlaySound(purchaseSound);
                UpdatePlayerMoneyDisplay();
                DisplayBuyItems(); // Refresh
            }
            else
            {
                PlaySound(errorSound);
            }
        }
        else
        {
            // Sell to shop
            bool success = currentShop.SellItem(selectedShopItem.item, amount);
            
            if (success)
            {
                PlaySound(sellSound);
                UpdatePlayerMoneyDisplay();
                DisplaySellItems(); // Refresh
            }
            else
            {
                PlaySound(errorSound);
            }
        }
    }

    void UpdatePlayerMoneyDisplay()
    {
        if (playerMoneyText != null)
        {
            PlayerMoney playerMoney = FindObjectOfType<PlayerMoney>();
            if (playerMoney != null)
            {
                playerMoneyText.text = $"{playerMoney.CurrentMoney}";
            }
        }
    }

    void ClearItemDisplay()
    {
        foreach (GameObject slot in spawnedSlots)
        {
            Destroy(slot);
        }
        spawnedSlots.Clear();
        
        if (selectedItemPanel != null)
            selectedItemPanel.SetActive(false);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void Update()
    {
        // Close shop with Escape
        if (shopPanel != null && shopPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }
}
