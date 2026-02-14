using UnityEngine;

/// <summary>
/// Controls farming indicator visibility based on equipped tool
/// Shows indicator CONTINUOUSLY when hoe/watering can/shovel is equipped
/// Hides indicator when other tools or items are equipped
/// </summary>
public class ToolIndicatorController : MonoBehaviour
{
    [Header("References")]
    public TilemapIndicator indicator;
    public PlayerFarming playerFarming;
    
    [Header("Settings")]
    [Tooltip("Which tools should show the farming indicator")]
    public bool showForHoe = true;
    public bool showForWateringCan = true;
    public bool showForShovel = true;
    public bool showForSeeds = true;
    
    [Header("Indicator Behavior")]
    [Tooltip("Keep indicator visible while tool is equipped")]
    public bool stayVisibleWhileEquipped = true;
    
    private ItemData currentlyEquippedItem;
    private bool isIndicatorActive = false;

    void Update()
    {
        // Check if selected item changed
        if (playerFarming != null && playerFarming.selectedItem != currentlyEquippedItem)
        {
            currentlyEquippedItem = playerFarming.selectedItem;
            UpdateIndicatorVisibility();
        }
        
        // Keep indicator active while farming tool is equipped
        if (stayVisibleWhileEquipped && isIndicatorActive && indicator != null)
        {
            // Make sure indicator stays enabled
            if (!indicator.gameObject.activeSelf)
            {
                indicator.gameObject.SetActive(true);
            }
        }
    }

    void UpdateIndicatorVisibility()
    {
        if (indicator == null) return;

        // No item equipped
        if (currentlyEquippedItem == null)
        {
            isIndicatorActive = false;
            indicator.gameObject.SetActive(false);
            return;
        }

        // Check if it's a tool
        if (currentlyEquippedItem is ToolItem tool)
        {
            bool shouldShow = tool.toolType switch
            {
                ToolType.Hoe => showForHoe,
                ToolType.WateringCan => showForWateringCan,
                ToolType.Shovel => showForShovel,
                _ => false // Axe, Pickaxe don't need indicator
            };

            isIndicatorActive = shouldShow;
            indicator.gameObject.SetActive(shouldShow);
            
            if (shouldShow)
            {
                Debug.Log($"✓ Indicator ENABLED for {tool.toolType} (will stay visible)");
            }
            else
            {
                Debug.Log($"✗ Indicator DISABLED for {tool.toolType}");
            }
        }
        // Check if it's seeds
        else if (currentlyEquippedItem is SeedItem && showForSeeds)
        {
            isIndicatorActive = true;
            indicator.gameObject.SetActive(true);
            Debug.Log($"✓ Indicator ENABLED for seeds (will stay visible)");
        }
        // Other items (food, resources, etc.)
        else
        {
            isIndicatorActive = false;
            indicator.gameObject.SetActive(false);
            Debug.Log($"✗ Indicator DISABLED for non-farming item");
        }
    }

    // Call this when you equip an item from inventory/hotbar
    public void OnItemEquipped(ItemData item)
    {
        currentlyEquippedItem = item;
        UpdateIndicatorVisibility();
    }

    // Call this when you unequip
    public void OnItemUnequipped()
    {
        currentlyEquippedItem = null;
        isIndicatorActive = false;
        UpdateIndicatorVisibility();
    }
    
    // Public method to check if indicator should be visible
    public bool ShouldShowIndicator()
    {
        return isIndicatorActive;
    }
}