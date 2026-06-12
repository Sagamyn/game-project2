using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class Merchant : MonoBehaviour
{
    [Header("Merchant Visual")]
    public Image merchantImage;

    [Header("Objects To Darken")]
    public Image[] imagesToDarken;

    [Header("Darken Settings")]
    public float darkenAmount = 0.4f;

    [Header("Animation")]
    public float fadeInDuration = 0.8f;
    public float slideUpDistance = 60f;

    [Header("Idle Bob")]
    public float bobAmount = 8f;
    public float bobSpeed = 1.2f;

    [Header("Speech Bubble")]
    public MerchantSpeechBubble speechBubble;

    [Header("Intro Dialogues")]
    [TextArea(1, 3)]
    public string[] introDialogues;

    [Header("Shop")]
    public MerchantShop merchantShop;

    [Header("Debt Collection")]
    public GameObject debtButtonContainer;
    public Button payButton;
    public Button refuseButton;
    public TextMeshProUGUI payButtonText;

    private int requiredDebtAmount = 0;
    private bool hasPaidDebt = false;
    private bool debtDecisionMade = false;

    private Vector2 originalPosition;
    private Coroutine bobCoroutine;
    private bool isBobbing = false;
    private RectTransform merchantRect;
    private Color[] originalColors;

    void Awake()
    {
        merchantRect = merchantImage.GetComponent<RectTransform>();
        originalPosition = merchantRect.anchoredPosition;

        if (imagesToDarken != null)
        {
            originalColors = new Color[imagesToDarken.Length];
            for (int i = 0; i < imagesToDarken.Length; i++)
            {
                if (imagesToDarken[i] != null)
                    originalColors[i] = imagesToDarken[i].color;
            }
        }

        HideInstant();

        // Keep SpeechBubble active so coroutines can run
        if (speechBubble != null)
            speechBubble.gameObject.SetActive(true);

        if (payButton != null)
            payButton.onClick.AddListener(OnPayClicked);

        if (refuseButton != null)
            refuseButton.onClick.AddListener(OnRefuseClicked);

        if (debtButtonContainer != null)
            debtButtonContainer.SetActive(false);
    }

    void HideInstant()
    {
        if (merchantImage != null)
        {
            Color c = merchantImage.color;
            c.a = 0f;
            merchantImage.color = c;
        }
    }

    public IEnumerator AppearFromDark()
    {
        gameObject.SetActive(true);

        if (speechBubble != null)
            speechBubble.gameObject.SetActive(true);

        merchantRect.anchoredPosition =
            originalPosition + Vector2.down * slideUpDistance;

        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            if (merchantImage != null)
            {
                Color c = merchantImage.color;
                c.a = smooth;
                merchantImage.color = c;
            }

            merchantRect.anchoredPosition = Vector2.Lerp(
                originalPosition + Vector2.down * slideUpDistance,
                originalPosition,
                smooth);

            if (imagesToDarken != null)
            {
                for (int i = 0; i < imagesToDarken.Length; i++)
                {
                    if (imagesToDarken[i] == null) continue;

                    Color original = originalColors[i];
                    Color dark = original * darkenAmount;
                    dark.a = original.a;

                    imagesToDarken[i].color = Color.Lerp(
                        original, dark, smooth);
                }
            }

            yield return null;
        }

        merchantRect.anchoredPosition = originalPosition;

        if (merchantImage != null)
        {
            Color c = merchantImage.color;
            c.a = 1f;
            merchantImage.color = c;
        }

        // Start bob
        bobCoroutine = StartCoroutine(IdleBob());

        // Small pause before talking
        yield return new WaitForSeconds(0.3f);

        int currentDay = FindObjectOfType<DayNightManager>() != null ? FindObjectOfType<DayNightManager>().currentDay : 1;

        // Cek apakah hari ini hari penagihan hutang (Kelipatan 7)
        if (currentDay > 0 && currentDay % 7 == 0)
        {
            // Mode Penagihan Hutang
            yield return StartCoroutine(PlayDebtDialogue(currentDay));
        }
        else
        {
            // Play intro dialogue biasa
            yield return StartCoroutine(PlayIntroDialogue());

            // After talking, show buff cards
            if (merchantShop != null)
                yield return StartCoroutine(merchantShop.StartShop());
        }
    }

    public int CalculateDebt(int currentDay)
    {
        // Tagihan awal $500, lalu naik x1.2 setiap kelipatan 7
        int cycle = currentDay / 7;
        if (cycle < 1) cycle = 1;
        return Mathf.RoundToInt(500f * Mathf.Pow(1.2f, cycle - 1));
    }

    IEnumerator PlayDebtDialogue(int currentDay)
    {
        if (speechBubble == null) yield break;

        requiredDebtAmount = CalculateDebt(currentDay);
        debtDecisionMade = false;
        hasPaidDebt = false;

        speechBubble.gameObject.SetActive(true);

        string greeting = (currentDay > 7) ? "You can survived this long huh..." : "We meet again...";
        yield return StartCoroutine(speechBubble.ShowAndType(greeting));
        yield return new WaitForSeconds(1f);

        string demand = $"It's pay time. Hand over ${requiredDebtAmount} RIGHT NOW!";
        yield return StartCoroutine(speechBubble.ShowAndType(demand));

        if (payButtonText != null)
            payButtonText.text = $"Pay ${requiredDebtAmount}";
        if (debtButtonContainer != null)
            debtButtonContainer.SetActive(true);

        if (payButton != null) payButton.interactable = true;
        if (refuseButton != null) refuseButton.interactable = true;

        // Tunggu sampai pemain memilih
        while (!debtDecisionMade)
        {
            yield return null;
        }

        if (debtButtonContainer != null)
            debtButtonContainer.SetActive(false);

        if (hasPaidDebt)
        {
            yield return StartCoroutine(speechBubble.ShowAndType("HAHAHA... GOOD BOY. DO YOUR JOB HARDER THAN BEFORE, I KNOW YOU CAN DO IT!"));
            yield return new WaitForSeconds(2f);
            yield return StartCoroutine(speechBubble.Hide());

            // Lanjut ke toko buff biasa!
            if (merchantShop != null)
                yield return StartCoroutine(merchantShop.StartShop());
        }
        else
        {
            // Game over dipanggil dari coroutine FailRoutine
            yield break;
        }
    }

    public void OnPayClicked()
    {
        if (payButton != null) payButton.interactable = false;
        if (refuseButton != null) refuseButton.interactable = false;

        PlayerMoney pm = FindObjectOfType<PlayerMoney>();
        if (pm != null && pm.CurrentMoney >= requiredDebtAmount)
        {
            pm.RemoveMoney(requiredDebtAmount);
            hasPaidDebt = true;
            debtDecisionMade = true;
        }
        else
        {
            StartCoroutine(FailRoutine("Beraninya kau menipuku! Uangmu tidak cukup, MATILAH!"));
        }
    }

    public void OnRefuseClicked()
    {
        if (payButton != null) payButton.interactable = false;
        if (refuseButton != null) refuseButton.interactable = false;

        StartCoroutine(FailRoutine("Menolak? Bodoh sekali! MATILAH!"));
    }

    private IEnumerator FailRoutine(string failMessage)
    {
        debtDecisionMade = true;

        if (debtButtonContainer != null)
            debtButtonContainer.SetActive(false);

        yield return StartCoroutine(speechBubble.ShowAndType(failMessage));
        yield return new WaitForSeconds(2f);

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowCustomGameOver("They took you away...");
        }
    }

    IEnumerator PlayIntroDialogue()
    {
        if (speechBubble == null) yield break;
        if (introDialogues == null || introDialogues.Length == 0) yield break;

        speechBubble.gameObject.SetActive(true);

        foreach (string line in introDialogues)
        {
            yield return StartCoroutine(speechBubble.ShowAndType(line));
            yield return new WaitForSeconds(1f);
        }

        yield return StartCoroutine(speechBubble.Hide());
    }

    public void Hide()
    {
        if (bobCoroutine != null)
            StopCoroutine(bobCoroutine);

        isBobbing = false;
        HideInstant();

        if (speechBubble != null)
        {
            Image img = speechBubble.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = 0f;
                img.color = c;
            }
        }

        if (merchantRect != null)
            merchantRect.anchoredPosition = originalPosition;

        if (imagesToDarken != null)
        {
            for (int i = 0; i < imagesToDarken.Length; i++)
            {
                if (imagesToDarken[i] != null)
                    imagesToDarken[i].color = originalColors[i];
            }
        }
    }

    IEnumerator IdleBob()
    {
        isBobbing = true;
        float timer = 0f;

        while (isBobbing)
        {
            timer += Time.deltaTime * bobSpeed;
            float offset = Mathf.Sin(timer) * bobAmount;
            merchantRect.anchoredPosition =
                originalPosition + Vector2.up * offset;
            yield return null;
        }
    }
}