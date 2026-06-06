using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FoodStallCustomer : MonoBehaviour
{
    [Header("UI References")]
    public Image customerImage;
    public GameObject speechBubble;
    public Image speechBubbleFoodIcon;
    public Image patienceBar;
    public GameObject happyEffect;
    public GameObject angryEffect;

    [Header("Patience Bar Colors")]
    public Color patienceHighColor = Color.green;
    public Color patienceMedColor  = Color.yellow;
    public Color patienceLowColor  = Color.red;

    [Header("Attack")]
    public int attackDamage = 15;
    public Image impactFrame;
    public AudioClip smackSound;

    // Runtime
    private CustomerData data;
    private ItemData orderedItem;
    private float patienceTimer;
    private bool isWaiting = false;
    private bool isLeaving = false;

    // Events
    public System.Action<FoodStallCustomer, bool, int> OnLeft;

    public ItemData OrderedItem => orderedItem;

    // =========================================
    // SPAWN
    // =========================================

    public void Spawn(CustomerData customerData)
    {
        data = customerData;

        if (data.possibleOrders != null && data.possibleOrders.Length > 0)
            orderedItem = data.possibleOrders[
                Random.Range(0, data.possibleOrders.Length)];
        else
        {
            Debug.LogError($"Customer {data.customerName} has no possible orders!");
            return;
        }

        if (customerImage != null)
            customerImage.sprite = data.idleSprite;

        if (speechBubble != null)
            speechBubble.SetActive(true);

        if (speechBubbleFoodIcon != null && orderedItem != null)
            speechBubbleFoodIcon.sprite = orderedItem.icon;

        if (happyEffect != null) happyEffect.SetActive(false);
        if (angryEffect != null) angryEffect.SetActive(false);

        patienceTimer = data.patience;
        isWaiting     = true;
        isLeaving     = false;

        StartCoroutine(SlideIn());

        Debug.Log($"✓ {data.customerName} appeared, wants: {orderedItem.itemName}");
    }

    // =========================================
    // UPDATE — patience tick
    // =========================================

    void Update()
    {
        if (!isWaiting || isLeaving) return;

        // Death 13 buff — no patience countdown at all
        if (BuffManager.Instance != null &&
            BuffManager.Instance.noPatience)
        {
            // Keep bar full and green visually
            if (patienceBar != null)
            {
                patienceBar.fillAmount = 1f;
                patienceBar.color      = patienceHighColor;
            }
            return;
        }

        // MorePatience buff — divide delta by multiplier so timer drains slower
        float multiplier = BuffManager.Instance?.patienceMultiplier ?? 1f;
        patienceTimer -= Time.deltaTime / multiplier;

        UpdatePatienceBar();

        if (patienceTimer <= 0f)
            StartCoroutine(LeaveAngry());
    }

    void UpdatePatienceBar()
    {
        if (patienceBar == null) return;

        float percent          = Mathf.Clamp01(patienceTimer / data.patience);
        patienceBar.fillAmount = percent;

        if (percent > 0.6f)
            patienceBar.color = patienceHighColor;
        else if (percent > 0.3f)
            patienceBar.color = patienceMedColor;
        else
            patienceBar.color = patienceLowColor;
    }

    // =========================================
    // SERVE
    // =========================================

    public void TryServe(ItemData food)
    {
        if (!isWaiting || isLeaving) return;

        if (food == orderedItem)
        {
            StartCoroutine(LeaveHappy());
        }
        else
        {
            // Hey Ya! buff — wrong food still makes customer happy
            if (BuffManager.Instance != null &&
                BuffManager.Instance.wrongFoodNoLeave)
            {
                Debug.Log("Hey Ya! buff — wrong food accepted!");
                StartCoroutine(LeaveHappy());
            }
            else
            {
                StartCoroutine(WrongFoodReaction());
            }
        }
    }

    // =========================================
    // LEAVE HAPPY
    // =========================================

    IEnumerator LeaveHappy()
    {
        isWaiting = false;
        isLeaving = true;

        if (speechBubble != null) speechBubble.SetActive(false);
        if (patienceBar  != null) patienceBar.gameObject.SetActive(false);

        if (customerImage != null && data.happySprite != null)
            customerImage.sprite = data.happySprite;

        if (happyEffect != null) happyEffect.SetActive(true);

        yield return new WaitForSeconds(1.2f);

        yield return StartCoroutine(SlideOut());

        // Apply money multiplier + bonus per happy customer
        float multiplier = BuffManager.Instance?.moneyMultiplier ?? 1f;
        int   bonus      = BuffManager.Instance?.bonusPerHappyCustomer ?? 0;
        int   finalPay   = Mathf.RoundToInt(data.payAmount * multiplier) + bonus;

        OnLeft?.Invoke(this, true, finalPay);

        Destroy(gameObject);
    }

    // =========================================
    // LEAVE ANGRY
    // =========================================

        IEnumerator LeaveAngry()
    {
        isWaiting = false;
        isLeaving = true;

        if (speechBubble != null)
            speechBubble.SetActive(false);

        if (patienceBar != null)
            patienceBar.gameObject.SetActive(false);

        if (customerImage != null && data.angrySprite != null)
            customerImage.sprite = data.angrySprite;

        if (angryEffect != null)
            angryEffect.SetActive(true);

        bool isInvincible =
            BuffManager.Instance != null &&
            BuffManager.Instance.isInvincible;

        if (!isInvincible)
        {
            // DAMAGE PLAYER
            PlayerHealth.Instance?.TakeDamage(attackDamage);

            // PLAY SMACK SOUND
            if (smackSound != null)
            {
                AudioManager.Instance.PlaySFX(smackSound);
            }

            // SCREEN FLASH
                if (impactFrame != null)
            {
                impactFrame.gameObject.SetActive(true);

                Color c = impactFrame.color;
                c.a = 1f;
                impactFrame.color = c;
            }

            Debug.Log(
                $"{data.customerName} attacked player for {attackDamage} damage!"
            );
        }

        bool stillPays =
            BuffManager.Instance != null &&
            BuffManager.Instance.angryCustomerStillPays;

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(SlideOut());

        if (stillPays)
        {
            OnLeft?.Invoke(this, true, data.payAmount);
        }
        else
        {
            OnLeft?.Invoke(this, false, 0);
        }

        Destroy(gameObject);
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
        Vector3 startPos = transform.localPosition + Vector3.down * 300f;
        Vector3 endPos   = transform.localPosition;
        transform.localPosition = startPos;

        float duration = 0.4f;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / duration;
            transform.localPosition = Vector3.Lerp(
                startPos, endPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.localPosition = endPos;
    }

    IEnumerator SlideOut()
    {
        Vector3 startPos = transform.localPosition;
        Vector3 endPos   = transform.localPosition + Vector3.down * 300f;

        float duration = 0.3f;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / duration;
            transform.localPosition = Vector3.Lerp(
                startPos, endPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
    }
}