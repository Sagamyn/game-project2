using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public Image ingredientIcon;
    public TextMeshProUGUI ingredientName;
    public TextMeshProUGUI amountText; // Shows "3/5" format
    public Image background;

    [Header("Colors")]
    public Color hasEnoughColor = Color.white;
    public Color notEnoughColor = Color.red;

    public void Setup(ItemData ingredient, int required, int has)
    {
        if (ingredientIcon != null)
            ingredientIcon.sprite = ingredient.icon;

        if (ingredientName != null)
            ingredientName.text = ingredient.itemName;

        bool hasEnough = has >= required;

        if (amountText != null)
        {
            amountText.text = $"{has}/{required}";
            amountText.color = hasEnough ? hasEnoughColor : notEnoughColor;
        }

        // Optional: color the background
        if (background != null)
        {
            Color bgColor = background.color;
            bgColor.a = hasEnough ? 0.3f : 0.5f;
            background.color = bgColor;
        }
    }
}