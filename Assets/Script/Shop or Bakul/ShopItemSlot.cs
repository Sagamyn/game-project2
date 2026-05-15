using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ShopItemSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Elements")]
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI priceText;
    public Image background;

    [Header("Discount UI")]
    public GameObject saleBadge;
    public TextMeshProUGUI originalPriceText;
    public TextMeshProUGUI discountedPriceText;

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

        if (itemIcon != null && item.item.icon != null)
            itemIcon.sprite = item.item.icon;

        if (itemNameText != null)
            itemNameText.text = item.item.itemName;

        if (background != null)
            background.color = normalColor;

        // Discount logic
        float discount = uiManager.currentShopDiscount;
        bool hasDiscount = !selling && discount > 0f;

        if (saleBadge != null)
            saleBadge.SetActive(hasDiscount);

        if (hasDiscount)
        {
            int finalPrice = Mathf.RoundToInt(item.buyPrice * (1f - discount));

            if (originalPriceText != null)
            {
                originalPriceText.gameObject.SetActive(true);
                originalPriceText.text = $"<s>${item.buyPrice}</s>";
                originalPriceText.color = Color.gray;
            }

            if (discountedPriceText != null)
            {
                discountedPriceText.gameObject.SetActive(true);
                discountedPriceText.text = $"${finalPrice}";
                discountedPriceText.color = Color.yellow;
            }

            if (priceText != null)
                priceText.gameObject.SetActive(false);
        }
        else
        {
            if (saleBadge != null)
                saleBadge.SetActive(false);

            if (originalPriceText != null)
                originalPriceText.gameObject.SetActive(false);

            if (discountedPriceText != null)
                discountedPriceText.gameObject.SetActive(false);

            if (priceText != null)
            {
                priceText.gameObject.SetActive(true);
                int price = selling ? item.sellPrice : item.buyPrice;
                priceText.text = $"${price}";
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (shopUI != null && shopItem != null)
        {
            shopUI.SelectItem(shopItem, isSelling);
            shopUI.BuyItemDirectly(shopItem, isSelling); // tambah ini

            if (background != null)
                background.color = selectedColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (background != null)
            background.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (background != null)
            background.color = normalColor;
    }
}