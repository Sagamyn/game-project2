using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image foodIcon;
    public TextMeshProUGUI orderIdText;
    public TextMeshProUGUI foodNameText;
    public TextMeshProUGUI priceText;
    public Image patienceBar;
    public TextMeshProUGUI timeRemainingText;
    public Button serveButton;

    [Header("Colors")]
    public Color patienceGoodColor = Color.green;
    public Color patienceMediumColor = Color.yellow;
    public Color patienceLowColor = Color.red;

    private CustomerOrder order;
    private RestaurantUI restaurantUI;

    public void Setup(CustomerOrder customerOrder, RestaurantUI ui)
    {
        order = customerOrder;
        restaurantUI = ui;

        if (foodIcon != null)
            foodIcon.sprite = order.orderedFood.icon;

        if (orderIdText != null)
            orderIdText.text = $"Order #{order.orderId}";

        if (foodNameText != null)
            foodNameText.text = order.orderedFood.itemName;

        if (priceText != null)
            priceText.text = $"${order.price}";

        if (serveButton != null)
        {
            serveButton.onClick.RemoveAllListeners();
            serveButton.onClick.AddListener(OnServeClicked);
        }
    }

    void Update()
    {
        if (order == null) return;

        // Update patience bar
        float patiencePercent = order.PatiencePercent();

        if (patienceBar != null)
        {
            patienceBar.fillAmount = patiencePercent;

            // Change color based on patience
            if (patiencePercent > 0.6f)
                patienceBar.color = patienceGoodColor;
            else if (patiencePercent > 0.3f)
                patienceBar.color = patienceMediumColor;
            else
                patienceBar.color = patienceLowColor;
        }

        // Update time text
        if (timeRemainingText != null)
        {
            int seconds = Mathf.CeilToInt(order.TimeRemaining());
            timeRemainingText.text = $"{seconds}s";
        }

        // Destroy if order expired
        if (order.IsExpired())
        {
            Destroy(gameObject);
        }
    }

    void OnServeClicked()
    {
        if (restaurantUI != null && order != null)
        {
            restaurantUI.ServeOrder(order);
            Destroy(gameObject); // Remove from list
        }
    }
}