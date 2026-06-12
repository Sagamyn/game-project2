using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MerchantShop : MonoBehaviour
{
    [Header("References")]
    public Merchant merchant;
    public MerchantSpeechBubble speechBubble;
    public PlayerMoney playerMoney;

    [Header("Card Sprites")]
    public Sprite cardBackSprite;       // the back card sprite
    public Sprite[] cardFrontSprites;   // index matches BuffData order
    // OR assign per BuffData directly (see BuffData.cardSprite)

    [Header("Card Slots (exactly 3)")]
    public BuffCardUI[] cardSlots;

    [Header("All Buffs")]
    public List<BuffData> allBuffs;

    [Header("Fan Positions (anchored on Canvas)")]
    public Vector2 leftCardPos = new Vector2(-180f, -200f);
    public Vector2 centerCardPos = new Vector2(0f, -180f);
    public Vector2 rightCardPos = new Vector2(180f, -200f);

    [Header("Fan Rotations")]
    public float leftRotation = -12f;
    public float centerRotation = 0f;
    public float rightRotation = 12f;

    [Header("Merchant Spawn Position")]
    // Match this to where your Merchant Image sits on the canvas
    public Vector2 merchantSpawnPos = new Vector2(0f, 150f);

    [Header("Reroll")]
    public Button rerollButton;
    public TextMeshProUGUI rerollCostText;
    public int rerollBaseCost = 50;

    [Header("Selection Dim Overlay")]
    public Image selectionDimOverlay;
    public float selectionDimAlpha = 0.75f;

    [Header("ESC Hint")]
    public GameObject escHintObject;

    [Header("Skip")]
    public Button skipButton;

    // Runtime
    public bool IsCardSelected() => selectedCard != null;
    private List<BuffData> currentOfferedBuffs = new List<BuffData>();
    private int currentRerollCost;
    private bool cardsInteractable = false;
    private BuffCardUI selectedCard = null;
    private bool isHandling = false;

    void Start()
    {
        if (playerMoney == null)
            playerMoney = FindObjectOfType<PlayerMoney>();

        foreach (var card in cardSlots)
            card.HideInstant();

        if (rerollButton != null)
        {
            rerollButton.gameObject.SetActive(false);
            rerollButton.onClick.AddListener(OnRerollClicked);
        }

        // Skip merchant shop
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(false);
            skipButton.onClick.AddListener(OnSkipClicked);
        }

        HideDimOverlay();

        if (escHintObject != null)
            escHintObject.SetActive(false);
    }

    void Update()
    {
        // ESC to go back from selected card
        if (selectedCard != null &&
            !isHandling &&
            Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(DeselectCard());
        }
    }

    // =========================================
    // START SHOP — called from Merchant.cs
    // =========================================

    public IEnumerator StartShop()
    {
        cardsInteractable = false;
        currentRerollCost = rerollBaseCost;
        selectedCard = null;
        isHandling = false;

        foreach (var card in cardSlots)
            card.HideInstant();

        RollBuffs();

        // Deal 3 cards from below one by one
        yield return StartCoroutine(DealCards());

        // Show reroll button
        if (rerollButton != null)
        {
            rerollButton.gameObject.SetActive(true);
            UpdateRerollButton();
        }

        // Skip merchant button
        if (skipButton != null)
            skipButton.gameObject.SetActive(true);

        cardsInteractable = true;
    }

    // =========================================
    // ROLL BUFFS
    // =========================================

    void RollBuffs()
    {
        currentOfferedBuffs.Clear();

        List<BuffData> pool = new List<BuffData>(allBuffs);

        for (int i = 0; i < cardSlots.Length && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            currentOfferedBuffs.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
    }

    // =========================================
    // SKIP
    // =========================================

    void OnSkipClicked()
    {
        if (isHandling) return;
        StartCoroutine(HandleSkip());
    }
    IEnumerator HandleSkip()
    {
        isHandling = true;
        cardsInteractable = false;

        if (rerollButton != null)
            rerollButton.gameObject.SetActive(false);

        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        if (escHintObject != null)
            escHintObject.SetActive(false);

        // Kalau ada card yang sedang di-expand, kecilkan dulu
        if (selectedCard != null)
        {
            StartCoroutine(selectedCard.ShrinkBackToFan());
            selectedCard = null;
            yield return new WaitForSeconds(0.3f);
        }

        // Fly semua card balik ke merchant
        yield return StartCoroutine(FlyAllCardsBack());

        // Merchant kasih reaksi singkat
        if (speechBubble != null)
        {
            yield return StartCoroutine(speechBubble.ShowAndType("I guess you didn't have money for that."));
            yield return new WaitForSeconds(0.8f);
            yield return StartCoroutine(speechBubble.Hide());
        }

        FoodStallWaveManager waveManager =
            FindObjectOfType<FoodStallWaveManager>();

        waveManager?.OnMerchantShopDone();

        isHandling = false;
    }

    // =========================================
    // DEAL CARDS
    // =========================================

    IEnumerator DealCards()
    {
        Vector2[] positions = { leftCardPos, centerCardPos, rightCardPos };
        float[] rotations = { leftRotation, centerRotation, rightRotation };

        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (i >= currentOfferedBuffs.Count) break;

            BuffData buff = currentOfferedBuffs[i];

            // Get front sprite from BuffData directly
            // Setup card with back sprite first
            cardSlots[i].Setup(buff, cardBackSprite, this);

            // Start sliding in with stagger
            StartCoroutine(cardSlots[i].SlideInFromBelow(
                positions[i],
                rotations[i],
                i * 0.3f    // 0.3s stagger between each card
            ));
        }

        // Wait for all cards to finish sliding in
        // last card delay (0.3 * 2 = 0.6) + slide duration (0.4) + tiny buffer
        yield return new WaitForSeconds(1.2f);
    }

    // =========================================
    // CARD CLICKED
    // =========================================

    public void OnCardClicked(BuffCardUI card)
    {
        if (!cardsInteractable || isHandling) return;

        StartCoroutine(SelectCard(card));
    }

    IEnumerator SelectCard(BuffCardUI card)
    {
        isHandling = true;
        cardsInteractable = false;
        selectedCard = card;

        // Disable all cards interactivity
        foreach (var c in cardSlots)
            c.SetInteractable(false);

        // Slide other cards down off screen at same time as expanding
        foreach (var c in cardSlots)
        {
            if (c != card && c.gameObject.activeSelf)
                StartCoroutine(c.SlideDown());
        }

        // Expand selected card to center
        yield return StartCoroutine(card.ExpandToCenter(Vector2.zero));

        // Show ESC hint
        if (escHintObject != null)
            escHintObject.SetActive(true);

        isHandling = false;
    }

    // =========================================
    // DESELECT CARD (ESC)
    // =========================================

    IEnumerator DeselectCard()
    {
        if (isHandling) yield break;

        isHandling = true;

        if (escHintObject != null)
            escHintObject.SetActive(false);

        BuffCardUI cardToShrink = selectedCard;
        selectedCard = null;

        // Start shrinking the selected card
        StartCoroutine(cardToShrink.ShrinkBackToFan());

        // Slide other cards up simultaneously
        foreach (var c in cardSlots)
        {
            if (c != cardToShrink && c.gameObject.activeSelf)
            {
                // Stop any existing coroutines on that card first
                c.StopAllCoroutines();
                StartCoroutine(c.SlideUp());
            }
        }

        // Wait for all animations (ShrinkBackToFan and SlideUp both take 0.3s)
        yield return new WaitForSeconds(0.35f);

        cardsInteractable = true;
        isHandling = false;
    }

    // =========================================
    // PURCHASE
    // =========================================

    public void OnCardPurchased(BuffCardUI card, BuffData buff)
    {
        if (isHandling) return;
        StartCoroutine(HandlePurchase(card, buff));
    }

    IEnumerator HandlePurchase(BuffCardUI card, BuffData buff)
    {
        isHandling = true;
        cardsInteractable = false;

        // Check money
        if (!playerMoney.HasMoney(buff.price))
        {
            yield return StartCoroutine(
                speechBubble.ShowAndType("You can't afford that!"));

            yield return new WaitForSeconds(0.8f);

            yield return StartCoroutine(speechBubble.Hide());

            isHandling = false;
            cardsInteractable = true;
            yield break;
        }

        // Deduct money
        playerMoney.RemoveMoney(buff.price);

        // Hide ESC hint
        if (escHintObject != null)
            escHintObject.SetActive(false);

        // Hide skip button
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        // Hide reroll button
        if (rerollButton != null)
            rerollButton.gameObject.SetActive(false);

        // Fly all cards back to merchant
        yield return StartCoroutine(FlyAllCardsBack());

        // Apply buff effect
        BuffManager.Instance?.ApplyBuff(buff);

        // Merchant reacts
        string reaction = !string.IsNullOrEmpty(buff.merchantReaction)
            ? buff.merchantReaction
            : "Excellent choice!";

        yield return StartCoroutine(speechBubble.ShowAndType(reaction));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(speechBubble.Hide());

        // Tell wave manager we are done
        FoodStallWaveManager waveManager =
            FindObjectOfType<FoodStallWaveManager>();

        waveManager?.OnMerchantShopDone();
    }

    // =========================================
    // REROLL
    // =========================================

    void OnRerollClicked()
    {
        if (!cardsInteractable || isHandling) return;

        if (!playerMoney.HasMoney(currentRerollCost))
        {
            StartCoroutine(
                speechBubble.ShowAndType("Not enough coins to reroll!"));
            return;
        }

        StartCoroutine(HandleReroll());
    }

    IEnumerator HandleReroll()
    {
        isHandling = true;
        cardsInteractable = false;

        if (rerollButton != null)
            rerollButton.gameObject.SetActive(false);

        playerMoney.RemoveMoney(currentRerollCost);

        // Increase cost for next reroll
        currentRerollCost = Mathf.RoundToInt(currentRerollCost * 1.5f);

        // Fly all cards back
        yield return StartCoroutine(FlyAllCardsBack());

        // Roll new buffs and deal
        RollBuffs();
        yield return StartCoroutine(DealCards());

        if (rerollButton != null)
        {
            rerollButton.gameObject.SetActive(true);
            UpdateRerollButton();
        }

        cardsInteractable = true;
        isHandling = false;
    }

    // =========================================
    // FLY ALL CARDS BACK
    // =========================================

    IEnumerator FlyAllCardsBack()
    {
        foreach (var card in cardSlots)
        {
            if (card.gameObject.activeSelf)
                StartCoroutine(card.FlyBackToMerchant(merchantSpawnPos));
        }

        // Wait for fly back to finish
        yield return new WaitForSeconds(0.5f);
    }

    // =========================================
    // DIM OVERLAY
    // =========================================

    IEnumerator FadeDimOverlay(bool fadeIn)
    {
        if (selectionDimOverlay == null) yield break;

        selectionDimOverlay.gameObject.SetActive(true);

        float startAlpha = fadeIn ? 0f : selectionDimAlpha;
        float targetAlpha = fadeIn ? selectionDimAlpha : 0f;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Color c = selectionDimOverlay.color;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            selectionDimOverlay.color = c;
            yield return null;
        }

        if (!fadeIn)
            HideDimOverlay();
    }

    void HideDimOverlay()
    {
        if (selectionDimOverlay == null) return;

        Color c = selectionDimOverlay.color;
        c.a = 0f;
        selectionDimOverlay.color = c;
        selectionDimOverlay.gameObject.SetActive(false);
    }

    // =========================================
    // REROLL BUTTON
    // =========================================

    void UpdateRerollButton()
    {
        if (rerollCostText != null)
            rerollCostText.text = $"${currentRerollCost}";

        if (rerollButton != null)
            rerollButton.interactable =
                playerMoney.HasMoney(currentRerollCost);
    }
}