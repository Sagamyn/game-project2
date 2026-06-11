using UnityEngine;
using UnityEngine.UI;

public class CookingPOVPatienceUI : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup canvasGroup;
    public Image customerIcon;
    public Image patienceBar; // Harus diset Filled, Horizontal
    public Transform orderIconsContainer;
    public GameObject orderIconPrefab;

    [Header("Visual Settings")]
    public Color highColor = Color.green;
    public Color medColor = Color.yellow;
    public Color lowColor = Color.red;

    private FoodStallCustomer currentCustomer;
    private RectTransform rectTransform;
    private Vector2 originalPosition;

    private static Sprite dummySprite;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;

        if (patienceBar != null && patienceBar.type != Image.Type.Filled)
        {
            patienceBar.type = Image.Type.Filled;
            patienceBar.fillMethod = Image.FillMethod.Horizontal;
        }

        if (patienceBar != null && patienceBar.sprite == null)
        {
            if (dummySprite == null)
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                dummySprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            }
            patienceBar.sprite = dummySprite;
        }
    }

    void Update()
    {
        // Cari customer jika belum ada
        if (currentCustomer == null || !currentCustomer.IsWaiting || currentCustomer.IsLeaving)
        {
            currentCustomer = null;
            FoodStallCustomer[] customers = FindObjectsOfType<FoodStallCustomer>(true);
            foreach (var c in customers)
            {
                if (c.IsWaiting && !c.IsLeaving)
                {
                    currentCustomer = c;
                    break;
                }
            }

            if (currentCustomer != null)
            {
                SetupCustomer(currentCustomer);
            }
            else
            {
                if (canvasGroup != null) canvasGroup.alpha = 0f;
                return;
            }
        }

        // Jika customer ada, tampilkan dan update
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        // Update timer
        float currentPatience = currentCustomer.CurrentPatienceTimer;
        float totalPatience = currentCustomer.TotalPatience;
        float percent = Mathf.Clamp01(currentPatience / totalPatience);

        if (patienceBar != null)
        {
            patienceBar.fillAmount = percent;
            patienceBar.color = percent > 0.6f ? highColor
                              : percent > 0.3f ? medColor
                                               : lowColor;
        }

        // Shake effect jika waktu mau habis (< 5 detik)
        if (currentPatience <= 5f && currentPatience > 0f)
        {
            float shakeAmount = 2f;
            rectTransform.anchoredPosition = originalPosition + new Vector2(
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount)
            );
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    void SetupCustomer(FoodStallCustomer customer)
    {
        if (customerIcon != null)
            customerIcon.sprite = customer.CustomerSprite;

        // Bersihkan icon lama
        if (orderIconsContainer != null)
        {
            foreach (Transform child in orderIconsContainer)
            {
                Destroy(child.gameObject);
            }
        }

        if (orderIconsContainer != null)
        {
            // Icon Tortilla
            if (customer.tortillaData != null && customer.tortillaData.ingredientSprite.Length > 0)
            {
                CreateIcon(customer.tortillaData.ingredientSprite[0]);
            }

            // Icon Toppings
            if (customer.AssignedRecipe != null)
            {
                foreach (var ingredient in customer.AssignedRecipe.requiredIngredients)
                {
                    if (ingredient.ingredientSprite == null || ingredient.ingredientSprite.Length == 0) continue;
                    CreateIcon(ingredient.ingredientSprite[0]);
                }
            }
            else if (customer.OrderedItem != null)
            {
                // Kalau bukan kebab (sistem lama)
                CreateIcon(customer.OrderedItem.icon);
            }
        }
    }

    void CreateIcon(Sprite sprite)
    {
        if (sprite == null || orderIconsContainer == null) return;

        GameObject iconObj;
        if (orderIconPrefab != null)
        {
            iconObj = Instantiate(orderIconPrefab, orderIconsContainer);
        }
        else
        {
            iconObj = new GameObject("OrderIcon");
            iconObj.transform.SetParent(orderIconsContainer, false);
            iconObj.AddComponent<RectTransform>();
            
            // Tambahkan LayoutElement agar ukurannya tidak dikecilkan oleh HorizontalLayoutGroup
            var le = iconObj.AddComponent<UnityEngine.UI.LayoutElement>();
            le.minWidth = 45f;
            le.minHeight = 45f;
            le.preferredWidth = 45f;
            le.preferredHeight = 45f;
        }

        Image img = iconObj.GetComponent<Image>();
        if (img == null) img = iconObj.AddComponent<Image>();
        
        img.sprite = sprite;
        img.preserveAspect = true;
        img.rectTransform.sizeDelta = new Vector2(45f, 45f);
    }
}
