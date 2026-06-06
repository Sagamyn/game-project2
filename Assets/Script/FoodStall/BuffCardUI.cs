using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class BuffCardUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("Card Visual")]
    public Image cardImage;             // shows card art

    [Header("Card Text — hidden until expanded")]
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI priceText;

    [Header("Detail Panel — shown when expanded")]
    public GameObject detailPanel;
    public Button buyButton;

    [Header("Hover Settings")]
    public float hoverLiftAmount = 30f;
    public float hoverSpeed      = 10f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip cardSound;

    // Runtime
    private BuffData buffData;
    private MerchantShop shop;
    private RectTransform rectTransform;
    private Sprite normalSprite;
    private Sprite outlineSprite;
    private bool isSelected     = false;
    private bool isInteractable = false;
    private bool isHovered      = false;
    private Coroutine hoverCoroutine;
    private Vector2 restPosition;
    private float restRotation;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // =========================================
    // SETUP
    // =========================================

    public void Setup(BuffData data, Sprite backSprite, MerchantShop merchantShop)
    {
        buffData      = data;
        shop          = merchantShop;
        normalSprite  = data.cardSprite;
        outlineSprite = data.cardSpriteOutline;

        // Start showing back sprite
        if (cardImage != null)
            cardImage.sprite = backSprite;

        // Hide all text and detail panel
        SetTextVisible(false);

        if (detailPanel != null)
            detailPanel.SetActive(false);

        // Wire buy button
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }

        isInteractable = false;
        isSelected     = false;
        isHovered      = false;

        gameObject.SetActive(false);
    }

    // =========================================
    // SLIDE IN FROM BELOW
    // =========================================

    public IEnumerator SlideInFromBelow(
        Vector2 targetPos,
        float targetRotation,
        float delay)
    {
        yield return new WaitForSeconds(delay);

        gameObject.SetActive(true);

        Vector2 startPos = targetPos + Vector2.down * 600f;
        rectTransform.anchoredPosition = startPos;
        rectTransform.localRotation    = Quaternion.Euler(0, 0, targetRotation);
        rectTransform.localScale       = Vector3.one;

        PlayCardSound();

        float duration = 0.4f;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t      = elapsed / duration;
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.anchoredPosition = Vector2.Lerp(
                startPos, targetPos, smooth);

            yield return null;
        }

        // Snap to final
        rectTransform.anchoredPosition = targetPos;
        rectTransform.localRotation    = Quaternion.Euler(0, 0, targetRotation);

        restPosition = targetPos;
        restRotation = targetRotation;

        // Show normal card front sprite — no text yet
        if (cardImage != null && normalSprite != null)
            cardImage.sprite = normalSprite;

        PlayCardSound();

        // Text stays hidden in fan mode
        SetTextVisible(false);

        isInteractable = true;
    }

    // =========================================
    // FLY BACK TO MERCHANT
    // =========================================

    public IEnumerator FlyBackToMerchant(Vector2 merchantPos)
    {
        isInteractable = false;
        isHovered      = false;

        SetTextVisible(false);

        if (detailPanel != null)
            detailPanel.SetActive(false);

        // Reset to normal sprite before flying back
        if (cardImage != null && normalSprite != null)
            cardImage.sprite = normalSprite;

        PlayCardSound();

        float duration   = 0.35f;
        float elapsed    = 0f;
        Vector2 startPos = rectTransform.anchoredPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t      = elapsed / duration;
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.anchoredPosition = Vector2.Lerp(
                startPos, merchantPos, smooth);

            rectTransform.localScale = Vector3.Lerp(
                Vector3.one, Vector3.one * 0.1f, smooth);

            rectTransform.localRotation = Quaternion.Lerp(
                rectTransform.localRotation,
                Quaternion.identity,
                smooth);

            yield return null;
        }

        gameObject.SetActive(false);
        rectTransform.localScale = Vector3.one;
    }

    // =========================================
    // HOVER
    // =========================================

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable || isSelected) return;

        isHovered = true;

        // Swap to outline sprite
        if (cardImage != null && outlineSprite != null)
            cardImage.sprite = outlineSprite;

        PlayCardSound();

        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        hoverCoroutine = StartCoroutine(HoverMove(true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInteractable || isSelected) return;

        isHovered = false;

        // Swap back to normal sprite
        if (cardImage != null && normalSprite != null)
            cardImage.sprite = normalSprite;

        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        hoverCoroutine = StartCoroutine(HoverMove(false));
    }

    IEnumerator HoverMove(bool liftUp)
    {
        Vector2 target = liftUp
            ? restPosition + Vector2.up * hoverLiftAmount
            : restPosition;

        while (Vector2.Distance(
            rectTransform.anchoredPosition, target) > 0.5f)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(
                rectTransform.anchoredPosition,
                target,
                Time.deltaTime * hoverSpeed);

            yield return null;
        }

        rectTransform.anchoredPosition = target;
    }

    // =========================================
    // CLICK
    // =========================================

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable || isSelected) return;

        shop?.OnCardClicked(this);
    }

    // =========================================
    // EXPAND TO CENTER
    // =========================================

    public IEnumerator ExpandToCenter(Vector2 centerPos)
    {
        isSelected     = true;
        isInteractable = false;
        isHovered      = false;

        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);

        // Reset to normal sprite when expanded
        if (cardImage != null && normalSprite != null)
            cardImage.sprite = normalSprite;

        float duration   = 0.3f;
        float elapsed    = 0f;
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector3 startScale = rectTransform.localScale;
        float startRot   = rectTransform.localEulerAngles.z;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t      = elapsed / duration;
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.anchoredPosition = Vector2.Lerp(
                startPos, centerPos, smooth);

            rectTransform.localScale = Vector3.Lerp(
                startScale, Vector3.one * 1.4f, smooth);

            float angle = Mathf.LerpAngle(startRot, 0f, smooth);
            rectTransform.localRotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }

        rectTransform.anchoredPosition = centerPos;
        rectTransform.localScale       = Vector3.one * 1.4f;
        rectTransform.localRotation    = Quaternion.identity;

        // Now show text and detail panel
        SetTextVisible(true);

        if (cardNameText != null)    cardNameText.text    = buffData.buffName;
        if (descriptionText != null) descriptionText.text = buffData.description;
        if (priceText != null)       priceText.text       = $"${buffData.price}";

        if (detailPanel != null)
            detailPanel.SetActive(true);
    }

    // =========================================
    // SHRINK BACK TO FAN
    // =========================================

    public IEnumerator ShrinkBackToFan()
    {
        isSelected = false;

        // Hide text and detail panel
        SetTextVisible(false);

        if (detailPanel != null)
            detailPanel.SetActive(false);

        float duration   = 0.25f;
        float elapsed    = 0f;
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector3 startScale = rectTransform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t      = elapsed / duration;
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.anchoredPosition = Vector2.Lerp(
                startPos, restPosition, smooth);

            rectTransform.localScale = Vector3.Lerp(
                startScale, Vector3.one, smooth);

            float angle = Mathf.LerpAngle(0f, restRotation, smooth);
            rectTransform.localRotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }

        rectTransform.anchoredPosition = restPosition;
        rectTransform.localScale       = Vector3.one;
        rectTransform.localRotation    = Quaternion.Euler(0, 0, restRotation);

        isInteractable = true;
    }

    // =========================================
    // SLIDE DOWN / UP (when another card is selected)
    // =========================================

    public IEnumerator SlideDown()
    {
        isInteractable = false;

        Vector2 targetPos = restPosition + Vector2.down * 700f;
        float duration    = 0.3f;
        float elapsed     = 0f;
        Vector2 startPos  = rectTransform.anchoredPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float smooth = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            rectTransform.anchoredPosition = Vector2.Lerp(
                startPos, targetPos, smooth);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPos;
    }

    public IEnumerator SlideUp()
    {
        // Make sure card is active and visible
        gameObject.SetActive(true);

        Vector2 targetPos = restPosition;
        float duration    = 0.3f;
        float elapsed     = 0f;
        Vector2 startPos  = rectTransform.anchoredPosition;

        // Restore fan rotation while sliding up
        Quaternion targetRot = Quaternion.Euler(0, 0, restRotation);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float smooth = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            rectTransform.anchoredPosition = Vector2.Lerp(
                startPos, targetPos, smooth);

            rectTransform.localRotation = Quaternion.Lerp(
                rectTransform.localRotation,
                targetRot,
                smooth);

            yield return null;
        }

        rectTransform.anchoredPosition = targetPos;
        rectTransform.localRotation    = targetRot;
        isInteractable = true;
    }

    // Keep for compatibility
    public void SetDimmed(bool dimmed) { }

    // =========================================
    // BUY
    // =========================================

    void OnBuyClicked()
    {
        shop?.OnCardPurchased(this, buffData);
    }

    // =========================================
    // HELPERS
    // =========================================

    public BuffData GetBuffData() => buffData;

    public void SetInteractable(bool value)
    {
        isInteractable = value;
    }

    void SetTextVisible(bool visible)
    {
        if (cardNameText != null)
            cardNameText.gameObject.SetActive(visible);

        if (descriptionText != null)
            descriptionText.gameObject.SetActive(visible);

        if (priceText != null)
            priceText.gameObject.SetActive(visible);
    }

    void PlayCardSound()
    {
        if (audioSource == null || cardSound == null) return;

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(cardSound);
    }

    public void HideInstant()
    {
        // Safe null check in case Awake hasnt run yet
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        gameObject.SetActive(false);

        rectTransform.localScale    = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;

        if (detailPanel != null)
            detailPanel.SetActive(false);

        SetTextVisible(false);

        isSelected     = false;
        isInteractable = false;
        isHovered      = false;
    }
}