using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodItemButton : MonoBehaviour
{
    [Header("UI Elements")]
    public Image foodIcon;
    public TextMeshProUGUI foodName;
    public TextMeshProUGUI amountText;
    public TMP_InputField priceInput;
    public Button addButton;

    private ItemData food;
    private RestaurantUI restaurantUI;

    public void Setup(ItemData foodData, int amount, RestaurantUI ui)
    {
        food = foodData;
        restaurantUI = ui;

        if (foodIcon != null)
            foodIcon.sprite = food.icon;

        if (foodName != null)
            foodName.text = food.itemName;

        if (amountText != null)
            amountText.text = $"x{amount}";

        // Set default price
        if (priceInput != null)
            priceInput.text = food.sellPrice.ToString();

        if (addButton != null)
        {
            addButton.onClick.RemoveAllListeners();
            addButton.onClick.AddListener(OnAddClicked);
        }
    }

    void OnAddClicked()
    {
        if (restaurantUI != null && food != null)
        {
            int price = food.sellPrice; // Default

            // Try to parse custom price
            if (priceInput != null && int.TryParse(priceInput.text, out int customPrice))
            {
                price = customPrice;
            }

            restaurantUI.AddToMenu(food, price);
        }
    }
}