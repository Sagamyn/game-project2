using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Attach ke CookingPOV Canvas.
/// Spawn IngredientSlot prefab secara dinamis dari KebabRecipeData.
/// </summary>
public class CookingPOVManager : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject ingredientSlotPrefab;  // prefab IngredientSlot

    [Header("Parents (drag dari hierarchy)")]
    public Transform toppingsParent;         // parent untuk topping slots
    public Transform firstLayerParent;       // parent untuk base/layer pertama (tortilla, dll)
    public FoodStallServeHandler RestaurantServeHandler;

    [Header("Serve Button")]
    public Button serveButton;

    // Active slots — untuk reset saat order baru
    private List<IngredientSlotUI> activeSlots = new List<IngredientSlotUI>();

    // Recipe yang sedang aktif (di-set dari luar saat customer spawn)
    private KebabRecipeData currentRecipe;

    void Start()
    {
        if (serveButton != null)
            serveButton.onClick.AddListener(OnServeClicked);
    }

    // ── Dipanggil saat customer baru datang ─────────────────────────
    public void LoadRecipe(KebabRecipeData recipe)
    {
        currentRecipe = recipe;
        ClearSlots();

        if (recipe == null) return;

        // Spawn required ingredients di toppingsParent
        foreach (var item in recipe.requiredIngredients)
            SpawnSlot(item, toppingsParent);

        // Spawn optional ingredients di toppingsParent juga (atau firstLayerParent)
        // foreach (var item in recipe.optionalIngredients)
        //     SpawnSlot(item, toppingsParent);

        // PanCookingManager.Instance?.ClearPan();
    }

    void SpawnSlot(KebabIngredientData item, Transform parent)
    {
        if (ingredientSlotPrefab == null || parent == null) return;

        GameObject go = Instantiate(ingredientSlotPrefab, parent);
        IngredientSlotUI slot = go.GetComponent<IngredientSlotUI>();

        if (slot != null)
        {
            slot.Setup(item);
            activeSlots.Add(slot);
        }
    }

    void ClearSlots()
    {
        foreach (var slot in activeSlots)
            if (slot != null) Destroy(slot.gameObject);

        activeSlots.Clear();
    }

    void OnServeClicked()
    {
        KebabAssembly assembly = FindObjectOfType<KebabAssembly>();
        if (assembly == null) return;

        assembly.ServeKebab();
    }
}