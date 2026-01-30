using UnityEngine;
using System.Collections;

public class CookingStation : Interactable
{
    [Header("Cooking Station")]
    public Recipe[] availableRecipes;
    public CookingUI cookingUI;

    [Header("Visual Feedback")]
    public ParticleSystem cookingParticles;
    public AudioSource cookingSound;

    private bool isCooking = false;
    private bool isOpen = false;

    public override void Interact()
    {
        if (isCooking)
        {
            Debug.Log("Already cooking!");
            return;
        }

        if (isOpen)
        {
            CloseCookingUI();
        }
        else
        {
            OpenCookingUI();
        }
    }

    void OpenCookingUI()
    {
        if (cookingUI == null)
        {
            Debug.LogError("CookingUI not assigned!");
            return;
        }

        isOpen = true;
        cookingUI.Open(this, availableRecipes);

        PlayerMovement.Instance?.LockMovement(true);

        // Hide hotbar
        if (HotbarVisibilityManager.Instance != null)
        {
            HotbarVisibilityManager.Instance.HideHotbar();
        }

        Debug.Log("Cooking Station opened");
    }

    void CloseCookingUI()
    {
        if (cookingUI == null) return;

        isOpen = false;
        cookingUI.Close();

        PlayerMovement.Instance?.LockMovement(false);

        // Show hotbar
        if (HotbarVisibilityManager.Instance != null)
        {
            HotbarVisibilityManager.Instance.ShowHotbar();
        }

        Debug.Log("Cooking Station closed");
    }

    public void StartCooking(Recipe recipe, PlayerInventory inventory)
    {
        if (isCooking)
        {
            Debug.LogWarning("Already cooking!");
            return;
        }

        if (!recipe.CanCraft(inventory))
        {
            Debug.LogWarning("Not enough ingredients!");
            return;
        }

        StartCoroutine(CookingProcess(recipe, inventory));
    }

    IEnumerator CookingProcess(Recipe recipe, PlayerInventory inventory)
    {
        isCooking = true;

        // Consume ingredients
        foreach (var ing in recipe.ingredients)
        {
            inventory.ConsumeItem(ing.ingredient, ing.amount);
        }

        Debug.Log($"Started cooking {recipe.recipeName}...");

        // Visual feedback
        if (cookingParticles != null)
            cookingParticles.Play();

        if (cookingSound != null)
            cookingSound.Play();

        // Update UI
        if (cookingUI != null)
        {
            cookingUI.ShowCookingProgress(recipe, recipe.cookingTime);
        }

        // Wait for cooking time
        yield return new WaitForSeconds(recipe.cookingTime);

        // Add result to inventory
        bool success = inventory.AddItem(recipe.resultItem, recipe.resultAmount);

        if (success)
        {
            Debug.Log($"✓ Cooked {recipe.resultAmount}x {recipe.resultItem.itemName}!");
        }
        else
        {
            Debug.LogWarning("Inventory full! Cooked item lost!");
        }

        // Stop effects
        if (cookingParticles != null)
            cookingParticles.Stop();

        if (cookingSound != null)
            cookingSound.Stop();

        isCooking = false;

        // Update UI
        if (cookingUI != null)
        {
            cookingUI.OnCookingComplete(success);
        }
    }

    public bool IsCooking() => isCooking;

    void OnDestroy()
    {
        if (isOpen)
        {
            CloseCookingUI();
        }
    }
}