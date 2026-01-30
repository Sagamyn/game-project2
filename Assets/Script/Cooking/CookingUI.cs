using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CookingUI : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory playerInventory;
    public GameObject panel;
    public UIAnimator uiAnimator;

    [Header("Recipe List")]
    public Transform recipeListParent;
    public RecipeButton recipeButtonPrefab;

    [Header("Recipe Details")]
    public GameObject recipeDetailsPanel;
    public Image recipeIcon;
    public TextMeshProUGUI recipeName;
    public TextMeshProUGUI recipeDescription;
    public Transform ingredientsListParent;
    public IngredientDisplay ingredientDisplayPrefab;
    public Button cookButton;

    [Header("Cooking Progress")]
    public GameObject cookingProgressPanel;
    public Image progressBar;
    public TextMeshProUGUI progressText;

    private CookingStation currentStation;
    private Recipe[] availableRecipes;
    private Recipe selectedRecipe;
    private bool isOpen = false;

    void Awake()
    {
        if (uiAnimator != null)
            uiAnimator.HideInstant();
        else if (panel != null)
            panel.SetActive(false);

        if (cookingProgressPanel != null)
            cookingProgressPanel.SetActive(false);

        if (recipeDetailsPanel != null)
            recipeDetailsPanel.SetActive(false);
    }

    public void Open(CookingStation station, Recipe[] recipes)
    {
        if (station == null || recipes == null || recipes.Length == 0)
        {
            Debug.LogError("Invalid cooking station or recipes!");
            return;
        }

        currentStation = station;
        availableRecipes = recipes;
        isOpen = true;

        // Show UI
        if (uiAnimator != null)
        {
            panel.SetActive(true);
            uiAnimator.Show();
        }
        else if (panel != null)
        {
            panel.SetActive(true);
        }

        // Populate recipe list
        PopulateRecipeList();

        Debug.Log($"Cooking UI opened with {recipes.Length} recipes");
    }

    public void Close()
    {
        if (!isOpen) return;

        isOpen = false;

        // Hide UI
        if (uiAnimator != null)
            uiAnimator.Hide();
        else if (panel != null)
            panel.SetActive(false);

        // Clear selection
        selectedRecipe = null;
        if (recipeDetailsPanel != null)
            recipeDetailsPanel.SetActive(false);

        Debug.Log("Cooking UI closed");
    }

    void PopulateRecipeList()
    {
        // Clear existing buttons
        foreach (Transform child in recipeListParent)
        {
            Destroy(child.gameObject);
        }

        // Create button for each recipe
        foreach (var recipe in availableRecipes)
        {
            RecipeButton button = Instantiate(recipeButtonPrefab, recipeListParent);
            button.Setup(recipe, this);

            // Check if player can craft
            bool canCraft = recipe.CanCraft(playerInventory);
            button.SetCraftable(canCraft);
        }
    }

    public void SelectRecipe(Recipe recipe)
    {
        selectedRecipe = recipe;
        ShowRecipeDetails(recipe);
    }

    void ShowRecipeDetails(Recipe recipe)
    {
        if (recipeDetailsPanel != null)
            recipeDetailsPanel.SetActive(true);

        // Show recipe info
        if (recipeIcon != null)
            recipeIcon.sprite = recipe.recipeIcon;

        if (recipeName != null)
            recipeName.text = recipe.recipeName;

        if (recipeDescription != null)
            recipeDescription.text = recipe.description;

        // Show ingredients
        foreach (Transform child in ingredientsListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var ing in recipe.ingredients)
        {
            IngredientDisplay display = Instantiate(ingredientDisplayPrefab, ingredientsListParent);
            int has = playerInventory.GetAmount(ing.ingredient);
            display.Setup(ing.ingredient, ing.amount, has);
        }

        // Update cook button
        bool canCraft = recipe.CanCraft(playerInventory);
        if (cookButton != null)
        {
            cookButton.interactable = canCraft && !currentStation.IsCooking();
            cookButton.onClick.RemoveAllListeners();
            cookButton.onClick.AddListener(() => OnCookButtonClicked());
        }
    }

    void OnCookButtonClicked()
    {
        if (selectedRecipe == null || currentStation == null)
        {
            Debug.LogError("No recipe selected or station is null!");
            return;
        }

        if (currentStation.IsCooking())
        {
            Debug.LogWarning("Already cooking!");
            return;
        }

        currentStation.StartCooking(selectedRecipe, playerInventory);
    }

    public void ShowCookingProgress(Recipe recipe, float duration)
    {
        if (cookingProgressPanel != null)
        {
            cookingProgressPanel.SetActive(true);
            StartCoroutine(UpdateProgressBar(duration));
        }

        // Disable cook button while cooking
        if (cookButton != null)
            cookButton.interactable = false;
    }

    IEnumerator UpdateProgressBar(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            if (progressBar != null)
                progressBar.fillAmount = progress;

            if (progressText != null)
                progressText.text = $"Cooking... {Mathf.RoundToInt(progress * 100)}%";

            yield return null;
        }

        if (progressBar != null)
            progressBar.fillAmount = 1f;

        if (progressText != null)
            progressText.text = "Done!";
    }

    public void OnCookingComplete(bool success)
    {
        if (cookingProgressPanel != null)
            cookingProgressPanel.SetActive(false);

        // Refresh recipe list (update craftable status)
        PopulateRecipeList();

        // Refresh selected recipe details
        if (selectedRecipe != null)
            ShowRecipeDetails(selectedRecipe);

        if (success)
        {
            Debug.Log("✓ Cooking complete!");
        }
        else
        {
            Debug.LogWarning("Cooking complete but inventory was full!");
        }
    }

    void Update()
    {
        // Close with ESC key
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }
}