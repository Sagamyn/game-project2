using UnityEngine;

[CreateAssetMenu(fileName = "NewCustomer", menuName = "FoodStall/Customer Data")]
public class CustomerData : ScriptableObject
{
    [Header("Identity")]
    public string customerName = "Customer";

    [Header("Art")]
    public Sprite idleSprite;    // Default/waiting
    public Sprite happySprite;   // When served correctly
    public Sprite angrySprite;   // When patience runs out or wrong food

    [Header("Order — pilih salah satu")]
    [Tooltip("Isi ini kalau customer pesan dari cooking minigame (kebab)")]
    public KebabRecipeData[] possibleRecipes;

    [Tooltip("Isi ini kalau customer pesan item biasa dari inventory (sistem lama)")]
    public ItemData[] possibleOrders;


    [Header("Patience")]
    public float patience = 30f;

    [Header("Reward")]
    public int payAmount = 50;

    // Pilih order random saat customer spawn
    public KebabRecipeData GetRandomRecipe()
    {
        if (possibleRecipes == null || possibleRecipes.Length == 0)
            return null;

        return possibleRecipes[Random.Range(0, possibleRecipes.Length)];
    }
}