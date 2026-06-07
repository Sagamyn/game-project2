using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Kebab Recipe", menuName = "Restaurant/Kebab Recipe")]
public class KebabRecipeData : ScriptableObject
{
    [Header("Info")]
    public string recipeName;
    public Sprite recipeIcon;

    [Header("Required Ingredients")]
    [Tooltip("Semua item ini harus ada di teflon")]
    // public List<ItemData> requiredIngredients = new List<ItemData>();
    public KebabIngredientData[] requiredIngredients;

    // [Header("Optional Toppings")]
    // [Tooltip("Boleh ada, boleh tidak — kalau ada dapat bonus")]
    // public List<ItemData> optionalIngredients = new List<ItemData>();

    [Header("Pricing")]
    public int basePrice = 20;
    [Tooltip("Bonus kalau semua optional topping juga ada")]
    public int bonusPrice = 5;

    [Header("Patience")]
    [Tooltip("Override patience customer untuk order ini. 0 = pakai default")]
    public float patienceOverride = 0f;
}