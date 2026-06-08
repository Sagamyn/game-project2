using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class KebabAssembly : MonoBehaviour
{
    [Header("References")]
    public Transform firstLayerParent;   // drag FirstLayer di sini
    public Transform toppingsParent;     // drag Toppings di sini

    [Header("Result")]
    public KebabFoodItem kebabResult;
    public PlayerInventory playerInventory;

    [Header("Settings")]
    // public Vector2 toppingOffset = new Vector2(0, 20f);
    // setiap topping naik 20px dari sebelumnya

    private bool hasTortilla = false;
    private List<KebabIngredientData> addedIngredients = new List<KebabIngredientData>();
    private int toppingCount = 0;

    public static KebabResult LastResult;
    public bool HasTortilla => hasTortilla;
    // Tortilla position
    public float tortillaRadius = 70f;
    public List<KebabIngredientData> GetIngredients() => addedIngredients;

    public bool AddIngredient(KebabIngredientData ingredient)
    {
        // Kalau belum ada tortilla dan yang ditambah bukan tortilla, tolak
        if (!hasTortilla && !ingredient.isTortilla)
        {
            Debug.Log("Taruh tortilla dulu!");
            return false;
        }

        if (ingredient.isTortilla)
        {
            if (hasTortilla)
            {
                Debug.Log("Tortilla udah ada!");
                return false;
            }

            // Spawn tortilla di FirstLayer
            // SpawnSprite(ingredient.ingredientSprite, firstLayerParent, Vector2.zero);
            // hasTortilla = true;
            Sprite randomSprite = ingredient.ingredientSprite[
                                  Random.Range(
                                    0,
                                    ingredient.ingredientSprite.Length
                                  )
                ];
            SpawnSprite(
                randomSprite,
                firstLayerParent,
                Vector2.zero
            );

            hasTortilla = true;
        }
        else
        {
            // Spawn topping di Toppings, dengan offset naik
            // Vector2 offset = toppingOffset * toppingCount;
            // SpawnSprite(ingredient.ingredientSprite, toppingsParent, offset);
            // toppingCount++;

            int amount = Random.Range(ingredient.minSpawn, ingredient.maxSpawn + 1);

            for (int i = 0; i < amount; i++)
            {
                Sprite randomSprite =
                    ingredient.ingredientSprite[
                        Random.Range(
                            0,
                            ingredient.ingredientSprite.Length
                        )
                    ];

                Vector2 randomPos =
                    Random.insideUnitCircle * tortillaRadius;

                SpawnSprite(
                    randomSprite,
                    toppingsParent,
                    randomPos
                );
            }
        }

        addedIngredients.Add(ingredient);
        Debug.Log($"Added: {ingredient.ingredientName}");
        return true;
    }

    // Serve kebab
    public void ServeKebab()
    {
        Debug.Log("ServeKebab dipanggil!");

        if (!hasTortilla)
        {
            Debug.Log("Tortilla belum ada!");
            return;
        }

        if (addedIngredients.Count < 2)
        {
            Debug.Log("Minimal 2 ingredient!");
            return;
        }

        // Simpan result untuk dicek saat ServeButton ditekan
        LastResult = new KebabResult();
        foreach (var ing in addedIngredients)
            LastResult.AddIngredient(ing);

        // Masukkan ke inventory seperti semula
        bool success = playerInventory.AddItem(kebabResult, 1);

        if (success)
        {
            Debug.Log("Kebab masuk inventory!");

            // Cek isi inventory sekarang
            foreach (var slot in playerInventory.items)
            {
                if (slot.item != null)
                    Debug.Log($"Inventory: {slot.item.itemName} x{slot.amount}");
            }
            ClearKebab();
        }
        else
        {
            Debug.Log("Inventory penuh!");
        }
    }

    void SpawnSprite(Sprite sprite, Transform parent, Vector2 position)
    {
        GameObject obj = new GameObject("IngredientSprite");
        obj.transform.SetParent(parent, false);

        Image img = obj.AddComponent<Image>();
        img.sprite = sprite;
        img.SetNativeSize();

        RectTransform rt = obj.GetComponent<RectTransform>();

        rt.anchoredPosition = position;

        rt.localRotation =
            Quaternion.Euler(
                0,
                0,
                Random.Range(0f, 360f)
            );

        float scale =
            Random.Range(0.8f, 1.2f);

        rt.localScale = Vector3.one * scale;
    }

    public void ClearKebab()
    {
        // Hapus semua children di FirstLayer dan Toppings
        foreach (Transform child in firstLayerParent)
            Destroy(child.gameObject);

        foreach (Transform child in toppingsParent)
            Destroy(child.gameObject);

        hasTortilla = false;
        // toppingCount = 0; // <- not used anymore since we use random position for toppings
        addedIngredients.Clear();
    }
}