using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeButton : MonoBehaviour
{
    [Header("UI Elements")]
    public Image recipeIcon;
    public TextMeshProUGUI recipeName;
    public GameObject canCraftIndicator;
    public GameObject lockedOverlay;
    public Button button;

    private Recipe recipe;
    private CookingUI cookingUI;

    public void Setup(Recipe recipe, CookingUI ui)
    {
        this.recipe = recipe;
        this.cookingUI = ui;

        if (recipeIcon != null)
            recipeIcon.sprite = recipe.recipeIcon;

        if (recipeName != null)
            recipeName.text = recipe.recipeName;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    public void SetCraftable(bool canCraft)
    {
        if (canCraftIndicator != null)
            canCraftIndicator.SetActive(canCraft);

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!canCraft);

        // Visual feedback
        if (recipeName != null)
        {
            recipeName.color = canCraft ? Color.white : Color.gray;
        }
    }

    void OnClick()
    {
        if (cookingUI != null && recipe != null)
        {
            cookingUI.SelectRecipe(recipe);
        }
    }
}