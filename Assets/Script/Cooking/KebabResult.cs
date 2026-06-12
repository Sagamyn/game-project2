using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class KebabResult
{
    public List<KebabIngredientData> ingredients = new List<KebabIngredientData>();

    public void AddIngredient(KebabIngredientData item)
    {
        if (item == null) return;
        ingredients.Add(item);
    }

    public void Clear()
    {
        ingredients.Clear();
    }

    public bool Matches(KebabRecipeData recipe)
    {
        if (recipe == null) return false;

        // 1. Cek apakah semua bahan wajib ada
        foreach (var required in recipe.requiredIngredients)
        {
            bool found = ingredients.Exists(
                ing => ing.ingredientName.Trim().ToLower() ==
                       required.ingredientName.Trim().ToLower()
            );

            Debug.Log($"Check: {required.ingredientName} → {(found ? "ADA" : "TIDAK ADA")}");

            if (!found) return false;
        }

        // 2. Cek apakah ada bahan berlebih / salah (selain tortilla)
        foreach (var ing in ingredients)
        {
            if (ing.isTortilla) continue;

            bool isRequired = false;
            foreach (var required in recipe.requiredIngredients)
            {
                if (ing.ingredientName.Trim().ToLower() == required.ingredientName.Trim().ToLower())
                {
                    isRequired = true;
                    break;
                }
            }

            if (!isRequired)
            {
                Debug.Log($"Extra/Wrong Ingredient Found: {ing.ingredientName}. Order ditolak!");
                return false;
            }
        }

        return true;
    }

    public int CalculatePrice(KebabRecipeData recipe)
    {
        if (recipe == null) return 0;
        return recipe.basePrice;
    }
}