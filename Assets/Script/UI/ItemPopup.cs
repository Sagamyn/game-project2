using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Individual pickup notification UI element
/// Shows item icon, name, and amount
/// Renamed to match ItemPopupManager
/// </summary>
public class ItemPopup : MonoBehaviour
{
    [Header("UI References")]
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI amountText;
    public Image backgroundImage;
    
    private ItemData item;
    private int amount;
    private ItemPopupManager manager;
    private float creationTime;
    
    public void Initialize(ItemData itemData, int pickupAmount, ItemPopupManager managerRef)
    {
        item = itemData;
        amount = pickupAmount;
        manager = managerRef;
        creationTime = Time.time;
        
        UpdateDisplay();
    }
    
    void UpdateDisplay()
    {
        // Set item icon
        if (itemIcon != null && item != null && item.icon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = true;
        }
        
        // Set item name
        if (itemNameText != null && item != null)
        {
            itemNameText.text = item.itemName;
        }
        
        // Set amount
        if (amountText != null)
        {
            amountText.text = $"+{amount}";
            
            // Color based on amount
            if (amount >= 10)
                amountText.color = Color.green;
            else if (amount >= 5)
                amountText.color = Color.yellow;
            else
                amountText.color = Color.white;
        }
    }
    
    /// <summary>
    /// Add more items to this notification (stacking)
    /// </summary>
    public void AddAmount(int additionalAmount)
    {
        amount += additionalAmount;
        UpdateDisplay();
        
    }
    

    public void ResetTimer()
    {
        creationTime = Time.time;
    }
    
    public float GetAge()
    {
        return Time.time - creationTime;
    }
}