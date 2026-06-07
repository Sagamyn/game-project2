using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Attach ini ke prefab IngredientSlot.
/// Prefab structure:
///   IngredientSlot (this script + Button + Image)
///   └── IngredientSprite (Image - untuk icon ingredient)
/// </summary>
public class IngredientSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    public Image ingredientIcon;

    [Header("State")]
    public bool isAdded = false;       // sudah dimasukin ke teflon?

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color addedColor = new Color(1f, 1f, 1f, 0.4f); // redup kalau sudah dipakai

    private KebabIngredientData KebabIngredientData;

    public void Setup(KebabIngredientData item)
    {
        KebabIngredientData = item;
        isAdded = false;

        if (ingredientIcon != null && item?.ingredientSprite.Length > 0)
        {
            ingredientIcon.sprite = item.ingredientSprite[0];
            ingredientIcon.color = normalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isAdded) return;            // sudah dipakai, skip
        if (KebabIngredientData == null) return;

        // Kirim ke KebabAssembly
        KebabAssembly assembly = FindObjectOfType<KebabAssembly>();
        if (assembly == null) return;
        bool success = assembly.AddIngredient(KebabIngredientData);

        // Visual feedback — redup supaya player tahu sudah ditambah
        isAdded = true;
        if (ingredientIcon != null)
            ingredientIcon.color = addedColor;
    }

    // Dipanggil CookingPOVManager saat order baru / pan di-clear
    public void ResetSlot()
    {
        isAdded = false;
        if (ingredientIcon != null)
            ingredientIcon.color = normalColor;
    }
}