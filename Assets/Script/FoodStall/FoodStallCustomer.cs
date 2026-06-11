using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FoodStallCustomer : MonoBehaviour
{
    [Header("UI References")]
    public Image customerImage;
    public GameObject speechBubble;
    public KebabIngredientData tortillaData;
    // public Image speechBubbleFoodIcon;
    public Transform orderIconContainer;
    public GameObject orderIconPrefab;
    public Image patienceBar;
    public GameObject happyEffect;
    public GameObject angryEffect;

    [Header("Patience Bar Colors")]
    public Color patienceHighColor = Color.green;
    public Color patienceMedColor = Color.yellow;
    public Color patienceLowColor = Color.red;

    [Header("Attack")]
    public int attackDamage = 15;
    public Image impactFrame;
    public AudioClip smackSound;

    // Runtime
    private CustomerData data;
    private ItemData orderedItem;
    private KebabRecipeData assignedRecipe; // null kalau customer pakai ItemData biasa
    private float patienceTimer;
    private bool isWaiting = false;
    private bool isLeaving = false;
    private bool isHappyLeave = false;
    private bool hasLeft = false;
    private float lastDisableTime = -1f;

    // Harga override dari KebabResult.CalculatePrice()
    [HideInInspector] public int ServePriceOverride = -1;

    // Events
    public System.Action<FoodStallCustomer, bool, int> OnLeft;

    public ItemData OrderedItem => orderedItem;
    public KebabRecipeData AssignedRecipe => assignedRecipe;
    public CustomerData Data => data;
    public float CurrentPatienceTimer 
    {
        get 
        {
            if (BuffManager.Instance != null && BuffManager.Instance.noPatience)
                return TotalPatience;

            if (gameObject.activeInHierarchy || lastDisableTime <= 0f)
                return patienceTimer;
                
            float timePassed = Time.time - lastDisableTime;
            float multiplier = BuffManager.Instance?.patienceMultiplier ?? 1f;
            return Mathf.Max(0f, patienceTimer - (timePassed / multiplier));
        }
    }
    public float TotalPatience => (assignedRecipe?.patienceOverride > 0) ? assignedRecipe.patienceOverride : data.patience;
    public Sprite CustomerSprite => data.idleSprite;
    public bool IsWaiting => isWaiting;
    public bool IsLeaving => isLeaving;

    // =========================================
    // SPAWN
    // =========================================

    public void Spawn(CustomerData customerData)
    {
        data = customerData;

        // Coba ambil KebabRecipeData dulu
        KebabRecipeData recipe = data.GetRandomRecipe();

        if (recipe != null)
        {
            // Mode kebab — pakai KebabRecipeData
            assignedRecipe = recipe;
            orderedItem = null;

            // Tampilkan icon dari recipe
            // if (speechBubbleFoodIcon != null && recipe.recipeIcon != null)
            //     speechBubbleFoodIcon.sprite = recipe.recipeIcon;
            if (recipe != null)
            {
                ShowOrderIcons(recipe);
            }
        }
        else if (data.possibleOrders != null && data.possibleOrders.Length > 0)
        {
            // Fallback — pakai ItemData biasa (sistem lama tetap jalan)
            assignedRecipe = null;
            orderedItem = data.possibleOrders[
                Random.Range(0, data.possibleOrders.Length)];

            // if (speechBubbleFoodIcon != null && orderedItem != null)
            //     speechBubbleFoodIcon.sprite = orderedItem.icon;
            Debug.Log($"Fallback order: {orderedItem?.itemName}");
        }
        else
        {
            Debug.LogError($"Customer {data.customerName} has no possible orders!");
            return;
        }

        if (customerImage != null) 
        {
            customerImage.sprite = data.idleSprite;
            // Ukuran diperbesar sewajarnya agar kepala tidak terpotong atap warung
            customerImage.rectTransform.localScale = new Vector3(1.8f, 1.8f, 1f);
        }
        if (speechBubble != null) speechBubble.SetActive(true);
        if (happyEffect != null) happyEffect.SetActive(false);
        if (angryEffect != null) angryEffect.SetActive(false);

        // Skala speech bubble dikembalikan normal karena parentnya sudah normal
        if (speechBubble != null)
        {
            speechBubble.transform.localScale = Vector3.one;
        }

        float patience = (assignedRecipe?.patienceOverride > 0)
                           ? assignedRecipe.patienceOverride
                           : data.patience;
        patienceTimer = patience;
        isWaiting = true;
        isLeaving = false;

        StartCoroutine(SlideIn());

        Debug.Log($"✓ {data.customerName} appeared, wants: " +
                  $"{(assignedRecipe != null ? assignedRecipe.recipeName : orderedItem?.itemName)}");
    }

    // =========================================
    // UPDATE — patience tick
    // =========================================

    void ShowOrderIcons(KebabRecipeData recipe)
    {
        if (orderIconContainer == null || orderIconPrefab == null) return;
        // Clear icon yang ada sebelumnya
        foreach (Transform child in orderIconContainer)
        {
            Destroy(child.gameObject);
        }
        // Munculkan icon tortilla di message bubble dulu (kalau ada), baru toppingnya
        if (tortillaData != null && tortillaData.ingredientSprite.Length > 0)
        {
            GameObject tortillaIcon = Instantiate(orderIconPrefab, orderIconContainer);
            Image img = tortillaIcon.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = tortillaData.ingredientSprite[0];
                img.rectTransform.sizeDelta = new Vector2(40f, 40f); // fixed size semua
                // img.SetNativeSize();
            }
        }

        // Munculkan icon sisanya di samping bubble
        foreach (var ingredient in recipe.requiredIngredients)
        {
            if (ingredient.ingredientSprite == null ||
            ingredient.ingredientSprite.Length == 0) continue;

            GameObject iconObj = Instantiate(orderIconPrefab, orderIconContainer);
            Image img = iconObj.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = ingredient.ingredientSprite[0];
                img.rectTransform.sizeDelta = new Vector2(40f, 40f); // fixed size semua
                // img.SetNativeSize();
            }
        }
    }

    void Update()
    {
        if (!isWaiting || isLeaving) return;

        if (BuffManager.Instance != null && BuffManager.Instance.noPatience)
        {
            if (patienceBar != null)
            {
                patienceBar.fillAmount = 1f;
                patienceBar.color = patienceHighColor;
            }
            return;
        }

        float multiplier = BuffManager.Instance?.patienceMultiplier ?? 1f;
        patienceTimer -= Time.deltaTime / multiplier;

        UpdatePatienceBar();

        if (patienceTimer <= 0f)
            StartCoroutine(LeaveAngry());
    }

    private static Sprite dummySprite;

    void UpdatePatienceBar()
    {
        if (patienceBar == null) return;

        // BUG FIX: Jika Temen user tidak menset FillAmount, paksa set di sini
        if (patienceBar.type != Image.Type.Filled)
        {
            patienceBar.type = Image.Type.Filled;
            patienceBar.fillMethod = Image.FillMethod.Vertical;
            patienceBar.fillOrigin = (int)Image.OriginVertical.Bottom;
        }

        // BUG FIX: Image Component di Unity TIDAK AKAN bisa Filled kalau Sprite-nya kosong (None)
        if (patienceBar.sprite == null)
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

        float total = TotalPatience;
        float percent = Mathf.Clamp01(patienceTimer / total);

        patienceBar.fillAmount = percent;
        patienceBar.color = percent > 0.6f ? patienceHighColor
                          : percent > 0.3f ? patienceMedColor
                                           : patienceLowColor;
    }

    // =========================================
    // SERVE — ItemData (sistem lama, tetap jalan)
    // =========================================

    public void TryServe(ItemData food)
    {
        if (!isWaiting || isLeaving) return;

        if (food == orderedItem)
            StartCoroutine(LeaveHappy());
        else if (BuffManager.Instance != null && BuffManager.Instance.wrongFoodNoLeave)
            StartCoroutine(LeaveHappy());
        else
            StartCoroutine(WrongFoodReaction());
    }

    // =========================================
    // SERVE — KebabResult (sistem baru)
    // =========================================

    public void TryServeKebab(KebabResult result, KebabRecipeData recipe)
    {
        Debug.Log($"TryServeKebab dipanggil! isWaiting:{isWaiting} isLeaving:{isLeaving}");
        Debug.Log(System.Environment.StackTrace); // ini kasih tau siapa yang manggil
        if (!isWaiting || isLeaving) return;

        if (result.Matches(recipe))
            StartCoroutine(LeaveHappy());
        else if (BuffManager.Instance != null && BuffManager.Instance.wrongFoodNoLeave)
            StartCoroutine(LeaveHappy());
        else
            StartCoroutine(WrongFoodReaction());
    }

    // =========================================
    // LEAVE HAPPY
    // =========================================

    IEnumerator LeaveHappy()
    {
        isWaiting = false;
        isLeaving = true;
        isHappyLeave = true;

        if (speechBubble != null) speechBubble.SetActive(false);
        if (patienceBar != null) patienceBar.gameObject.SetActive(false);

        if (customerImage != null && data.happySprite != null)
            customerImage.sprite = data.happySprite;

        if (happyEffect != null) happyEffect.SetActive(true);

        yield return new WaitForSeconds(1.2f);
        yield return StartCoroutine(SlideOut());

        CompleteLeave();
    }

    // =========================================
    // LEAVE ANGRY
    // =========================================

    IEnumerator LeaveAngry()
    {
        isWaiting = false;
        isLeaving = true;
        isHappyLeave = false;

        if (speechBubble != null) speechBubble.SetActive(false);
        if (patienceBar != null) patienceBar.gameObject.SetActive(false);

        if (customerImage != null && data.angrySprite != null)
            customerImage.sprite = data.angrySprite;

        if (angryEffect != null) angryEffect.SetActive(true);

        bool isInvincible = BuffManager.Instance?.isInvincible ?? false;
        bool skipDamage = false;

        if (BuffManager.Instance != null && BuffManager.Instance.skipAngryCount > 0)
        {
            BuffManager.Instance.skipAngryCount--;
            skipDamage = true;
            Debug.Log("Skipped damage from angry customer due to buff!");
        }

        if (!isInvincible && !skipDamage)
        {
            PlayerHealth.Instance?.TakeDamage(attackDamage);

            if (smackSound != null)
                AudioManager.Instance.PlaySFX(smackSound);

            if (impactFrame != null)
            {
                impactFrame.gameObject.SetActive(true);
                Color c = impactFrame.color;
                c.a = 1f;
                impactFrame.color = c;
            }
        }

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(SlideOut());

        CompleteLeave();
    }

    private void CompleteLeave()
    {
        if (hasLeft) return; // Mencegah double trigger
        hasLeft = true;

        if (isHappyLeave)
        {
            float multiplier = BuffManager.Instance?.moneyMultiplier ?? 1f;
            int bonus = BuffManager.Instance?.bonusPerHappyCustomer ?? 0;

            // Pakai ServePriceOverride kalau ada (dari KebabResult.CalculatePrice)
            int baseAmount = ServePriceOverride >= 0 ? ServePriceOverride : data.payAmount;
            int finalPay = Mathf.RoundToInt(baseAmount * multiplier) + bonus;

            OnLeft?.Invoke(this, true, finalPay);
        }
        else
        {
            bool stillPays = BuffManager.Instance?.angryCustomerStillPays ?? false;
            // success tetap false walau dia masih bayar, agar tercatat marah di Result
            OnLeft?.Invoke(this, false, stillPays ? data.payAmount : 0);
        }

        Destroy(gameObject);
    }

    private void OnDisable()
    {
        lastDisableTime = Time.time;

        // BUG FIX: Jika customer di-disable (karena pindah POV) SAAT dia sedang leave,
        // Coroutine akan mati. Kita paksa selesaikan transaksinya agar antrean tidak stuck.
        if (isLeaving && !hasLeft)
        {
            CompleteLeave();
        }
    }

    private void OnEnable()
    {
        // Hitung waktu yang berlalu selama object ini didisable (karena pindah POV)
        if (lastDisableTime > 0f && isWaiting && !isLeaving)
        {
            float timePassed = Time.time - lastDisableTime;
            float multiplier = BuffManager.Instance?.patienceMultiplier ?? 1f;
            patienceTimer -= timePassed / multiplier;
            
            UpdatePatienceBar();
        }
    }

    // =========================================
    // WRONG FOOD
    // =========================================

    IEnumerator WrongFoodReaction()
    {
        if (customerImage != null && data.angrySprite != null)
            customerImage.sprite = data.angrySprite;

        yield return new WaitForSeconds(0.5f);

        if (customerImage != null)
            customerImage.sprite = data.idleSprite;

        Debug.Log($"{data.customerName}: That's not what I ordered!");
    }

    // =========================================
    // SLIDE IN / OUT
    // =========================================

    IEnumerator SlideIn()
    {
        // Muncul dari arah kiri (Vector3.left)
        Vector3 startPos = transform.localPosition + Vector3.left * 600f;
        Vector3 endPos = transform.localPosition;
        transform.localPosition = startPos;

        float duration = 0.4f, elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(
                startPos, endPos, Mathf.SmoothStep(0f, 1f, elapsed / duration));
            yield return null;
        }
        transform.localPosition = endPos;
    }

    IEnumerator SlideOut()
    {
        Vector3 startPos = transform.localPosition;
        // Pergi ke arah kanan (Vector3.right)
        Vector3 endPos = transform.localPosition + Vector3.right * 600f;

        float duration = 0.3f, elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(
                startPos, endPos, Mathf.SmoothStep(0f, 1f, elapsed / duration));
            yield return null;
        }
    }
}