using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image foodIcon;
    public TextMeshProUGUI foodName;
    public TextMeshProUGUI priceText;
    public GameObject emptyState;
    public GameObject filledState;
    public Button removeButton;

    private int slotIndex;
    private RestaurantUI restaurantUI;
    private ItemData currentFood;

    public void Initialize(int index, RestaurantUI ui)
    {
        slotIndex = index;
        restaurantUI = ui;

        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(OnRemoveClicked);
        }

        Clear();
    }

    public void SetItem(ItemData food, int price)
    {
        currentFood = food;

        if (foodIcon != null)
        {
            foodIcon.sprite = food.icon;
            foodIcon.enabled = true;
        }

        if (foodName != null)
            foodName.text = food.itemName;

        if (priceText != null)
            priceText.text = $"${price}";

        if (emptyState != null)
            emptyState.SetActive(false);

        if (filledState != null)
            filledState.SetActive(true);
    }

    public void Clear()
    {
        currentFood = null;

        if (foodIcon != null)
            foodIcon.enabled = false;

        if (foodName != null)
            foodName.text = "Empty";

        if (priceText != null)
            priceText.text = "";

        if (emptyState != null)
            emptyState.SetActive(true);

        if (filledState != null)
            filledState.SetActive(false);
    }

    void OnRemoveClicked()
    {
        if (restaurantUI != null)
        {
            restaurantUI.RemoveFromMenu(slotIndex);
        }
    }
}