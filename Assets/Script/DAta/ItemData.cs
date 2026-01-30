using UnityEngine;

public enum ItemType
{
    Seed,
    Crop,
    Tool,
    Resource,
    CookedFood,
    Ingredient,
    Other
}

public abstract class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite icon;
    public ItemType type;

    [Header("Stack Settings")]
    public bool stackable = true;
    public int maxStack = 99;

    [Header("Value")]
    public int sellPrice = 10;
    public int buyPrice = 20;

    [Header("Cooking (For Ingredients)")]
    public bool canBeCrafted = false;
    public Recipe[] recipes;
}
