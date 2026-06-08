using UnityEngine;
using UnityEngine.UI;

public class FoodStallServeHandler : MonoBehaviour
{
    public static FoodStallServeHandler Instance;

    [Header("References")]
    public FoodStallWaveManager waveManager;

    [Header("Serve Button (optional)")]
    public Button serveButton;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (serveButton != null)
            serveButton.onClick.AddListener(OnServeButtonClicked);
    }

    // ── Dipanggil tombol Serve di POV ────────────────────────────────
    void OnServeButtonClicked()
    {
        if (PanCookingManager.Instance == null)
        {
            // Fallback: tidak pakai cooking minigame, pakai inventory langsung
            waveManager?.ServeCurrentCustomer();
            return;
        }

        KebabResult result = PanCookingManager.Instance.FinalizeCooking();
        ServeWithResult(result);
    }

    // ── Dipanggil CookingPOVManager setelah masak selesai ────────────
    public void ServeWithResult(KebabResult result)
    {
        FoodStallCustomer customer = waveManager?.activeCustomer;

        Debug.Log($"ServeWithResult dipanggil! customer null: {customer == null}");

        if (waveManager == null)
        {
            Debug.LogError("FoodStallServeHandler: waveManager not assigned!");
            return;
        }

        if (customer == null)
        {
            Debug.LogWarning("Tidak ada customer yang aktif!");
            return;
        }

        // Ambil recipe yang dipesan customer saat ini
        KebabRecipeData recipe = customer.AssignedRecipe;
        Debug.Log($"AssignedRecipe null: {recipe == null}");

        if (recipe == null)
        {
            // Customer tidak pakai KebabRecipeData — fallback ke ItemData biasa
            waveManager.ServeCurrentCustomer();
            return;
        }

        if (result.Matches(recipe))
        {
            // Override harga dengan kalkulasi bonus optional
            int finalPrice = result.CalculatePrice(recipe);
            customer.ServePriceOverride = finalPrice;

            customer.TryServeKebab(result, recipe);
        }
        else
        {
            Debug.LogWarning($"Kebab tidak cocok dengan pesanan {recipe.recipeName}!");
            customer.TryServeKebab(result, recipe); // tetap kirim, customer akan react wrong food
        }
    }
}