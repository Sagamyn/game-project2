using UnityEngine;
using System;

[Serializable]
public class RecipeIngredient
{
    public ItemData ingredient;
    public int amount;
}

[CreateAssetMenu(fileName = "New Recipe", menuName = "Cooking/Recipe")]
public class Recipe : ScriptableObject
{
    [Header("Recipe Info")]
    public string recipeName;
    public Sprite recipeIcon;
    
    [Header("Ingredients")]
    public RecipeIngredient[] ingredients;
    
    [Header("Result")]
    public ItemData resultItem;
    public int resultAmount = 1;
    
    [Header("Cooking Settings")]
    public float cookingTime = 3f; // Seconds to cook
    public int experienceGained = 10;

    [Header("Description")]
    [TextArea(3, 5)]
    public string description;

    // Check if player has all ingredients
    public bool CanCraft(PlayerInventory inventory)
    {
        if (inventory == null) return false;

        foreach (var ing in ingredients)
        {
            if (!inventory.HasItem(ing.ingredient, ing.amount))
            {
                return false;
            }
        }

        return true;
    }

    // Get missing ingredients
    public string GetMissingIngredients(PlayerInventory inventory)
    {
        string missing = "";
        
        foreach (var ing in ingredients)
        {
            int has = inventory.GetAmount(ing.ingredient);
            if (has < ing.amount)
            {
                missing += $"{ing.ingredient.itemName}: {has}/{ing.amount}\n";
            }
        }

        return missing;
    }
}