using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ShopItemWorld : MonoBehaviour
{
    [Header("Item")]
    public ShopItem shopItem;

    [Header("Display")]
    public SpriteRenderer iconRenderer;

    [Header("Price Tag (Optional)")]
    public GameObject priceTagObject;
    public TMPro.TextMeshPro priceTagText;

    [Header("Highlight")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(1f, 1f, 0.6f, 1f);
    public Color soldOutColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    // ── Ganti CartManager dengan referensi langsung ke ShopNPC ──
    private ShopNPC shopNPC;
    private bool isSoldOut = false;

    void Awake()
    {
        if (iconRenderer == null)
            iconRenderer = GetComponent<SpriteRenderer>();

        // Cari ShopNPC di scene
        shopNPC = FindObjectOfType<ShopNPC>();
    }

    void Start()
    {
        RefreshDisplay();
    }

    public void Setup(ShopItem item)
    {
        shopItem = item;
        RefreshDisplay();
    }

    void RefreshDisplay()
    {
        if (shopItem == null) return;

        if (iconRenderer != null && shopItem.item != null && shopItem.item.icon != null)
            iconRenderer.sprite = shopItem.item.icon;

        if (priceTagText != null)
            priceTagText.text = $"${shopItem.buyPrice}";

        if (iconRenderer != null)
            iconRenderer.color = isSoldOut ? soldOutColor : normalColor;
    }

    void OnMouseEnter()
    {
        if (isSoldOut) return;
        if (iconRenderer != null)
            iconRenderer.color = hoverColor;
    }

    void OnMouseExit()
    {
        if (isSoldOut) return;
        if (iconRenderer != null)
            iconRenderer.color = normalColor;
    }

    void OnMouseDown()
    {
        if (isSoldOut) return;
        if (shopItem == null) return;
        TryPurchase();
    }

    void TryPurchase()
    {
        if (shopNPC == null)
        {
            Debug.LogError("[ShopItemWorld] ShopNPC not found in scene!");
            return;
        }

        // Pakai ShopNPC.PurchaseItem() yang udah ada
        bool success = shopNPC.PurchaseItem(shopItem, 1);

        if (success)
        {
            Debug.Log($"[ShopItemWorld] Purchased: {shopItem.item.itemName}");
            SetSoldOut();
        }
        else
        {
            StartCoroutine(ShakeOnFail());
        }
    }

    public void SetSoldOut()
    {
        isSoldOut = true;

        if (iconRenderer != null)
            iconRenderer.color = soldOutColor;

        if (priceTagText != null)
            priceTagText.text = "SOLD";

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    System.Collections.IEnumerator ShakeOnFail()
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;
        float duration = 0.3f;
        float magnitude = 0.08f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float offsetX = Mathf.Sin(elapsed * 60f) * magnitude * (1f - elapsed / duration);
            transform.localPosition = originalPos + new Vector3(offsetX, 0f, 0f);
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}