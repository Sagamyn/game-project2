using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Individual item slot in the shop UI
/// Shows item icon, name, price
/// </summary>
public class ShopItemSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Elements")]
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI priceText;
    public Image background;
    
    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color hoverColor = Color.gray;
    
    private ShopItem shopItem;
    private ShopUIManager shopUI;
    private bool isSelling;

    public void Setup(ShopItem item, ShopUIManager uiManager, bool selling = false)
    {
        shopItem = item;
        shopUI = uiManager;
        isSelling = selling;
        
        // Set icon
        if (itemIcon != null && item.item.icon != null)
        {
            itemIcon.sprite = item.item.icon;
        }
        
        // Set name
        if (itemNameText != null)
        {
            itemNameText.text = item.item.itemName;
        }
        
        // Set price
        if (priceText != null)
        {
            int price = selling ? item.sellPrice : item.buyPrice;
            priceText.text = $"${price}";
            priceText.color = selling ? Color.green : Color.white;
        }
        
        // Set background
        if (background != null)
        {
            background.color = normalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (shopUI != null && shopItem != null)
        {
            shopUI.SelectItem(shopItem, isSelling);
            
            // Visual feedback
            if (background != null)
            {
                background.color = selectedColor;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (background != null)
        {
            background.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (background != null)
        {
            background.color = normalColor;
        }
    }
}