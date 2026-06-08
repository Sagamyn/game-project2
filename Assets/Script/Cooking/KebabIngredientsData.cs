using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Kebab/Ingredient")]
public class KebabIngredientData : ScriptableObject
{
    [Header("Info")]
    public string ingredientName;

    // public KebabIngredientData[] ingredients; // untuk bahan yang terdiri dari beberapa bahan (misal: kebab yang punya daging, sayur, dll)
    public Sprite[] ingredientSprite; // sprite yang muncul di teflon
    public bool isTortilla = false; // flag khusus tortilla

    [Header("Spawn Settings")]
    public int minSpawn = 1;
    public int maxSpawn = 3;
}