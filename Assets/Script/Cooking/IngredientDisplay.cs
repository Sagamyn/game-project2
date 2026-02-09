using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public Image ingredientIcon;
    public TextMeshProUGUI ingredientName;
    public TextMeshProUGUI amountText;
    public Image statusIcon; // NEW - checkmark or X
    public Image background;

    [Header("Colors")]
    public Color hasEnoughColor = new Color(0.3f, 0.8f, 0.3f, 1f); // Green
    public Color notEnoughColor = new Color(0.8f, 0.3f, 0.3f, 1f); // Red
    public Color backgroundHasColor = new Color(0.2f, 0.5f, 0.2f, 0.3f); // Green tint
    public Color backgroundNeedColor = new Color(0.5f, 0.2f, 0.2f, 0.3f); // Red tint

    public void Setup(ItemData ingredient, int required, int has)
    {
        if (ingredientIcon != null)
            ingredientIcon.sprite = ingredient.icon;

        if (ingredientName != null)
            ingredientName.text = ingredient.itemName;

        bool hasEnough = has >= required;

        // Update amount text with color
        if (amountText != null)
        {
            amountText.text = $"{has}/{required}";
            amountText.color = hasEnough ? hasEnoughColor : notEnoughColor;
        }

        // Update status icon
        if (statusIcon != null)
        {
            statusIcon.color = hasEnough ? hasEnoughColor : notEnoughColor;
            statusIcon.enabled = true; // Always show
            // Optional: Change sprite based on status
            // statusIcon.sprite = hasEnough ? checkmarkSprite : xMarkSprite;
        }

        // Update background color based on status
        if (background != null)
        {
            background.color = hasEnough ? backgroundHasColor : backgroundNeedColor;
        }
    }
}