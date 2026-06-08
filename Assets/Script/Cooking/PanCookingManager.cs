using UnityEngine;
using System;

public class PanCookingManager : MonoBehaviour
{
    public static PanCookingManager Instance;

    [SerializeField] private KebabResult currentResult = new KebabResult();

    public event Action<KebabIngredientData> OnIngredientAdded;
    public event Action<KebabResult> OnCookingFinalized;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddIngredient(KebabIngredientData item)
    {
        if (item == null) return;

        currentResult.AddIngredient(item);
        OnIngredientAdded?.Invoke(item);

        Debug.Log($"[Pan] Added: {item.ingredientName} | total: {currentResult.ingredients.Count}");
    }

    public KebabResult FinalizeCooking()
    {
        KebabResult result = currentResult;
        OnCookingFinalized?.Invoke(result);

        Debug.Log($"[Pan] Finalized with {result.ingredients.Count} ingredients");

        currentResult = new KebabResult();
        return result;
    }

    public void ClearPan()
    {
        currentResult.Clear();
        Debug.Log("[Pan] Cleared");
    }

    public KebabResult GetCurrentResult() => currentResult;
}