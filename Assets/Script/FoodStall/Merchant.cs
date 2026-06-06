using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    public float bobSpeed  = 1.2f;

    [Header("Speech Bubble")]
    public MerchantSpeechBubble speechBubble;

    [Header("Intro Dialogues")]
    [TextArea(1, 3)]
    public string[] introDialogues;

    [Header("Shop")]
    public MerchantShop merchantShop;

    private Vector2 originalPosition;
    private Coroutine bobCoroutine;
    private bool isBobbing = false;
    private RectTransform merchantRect;
    private Color[] originalColors;

    void Awake()
    {
        merchantRect     = merchantImage.GetComponent<RectTransform>();
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
            float t      = elapsed / fadeInDuration;
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
                    Color dark     = original * darkenAmount;
                    dark.a         = original.a;

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

        // Play intro dialogue
        yield return StartCoroutine(PlayIntroDialogue());

        // After talking, show buff cards
        if (merchantShop != null)
            yield return StartCoroutine(merchantShop.StartShop());
    }

    IEnumerator PlayIntroDialogue()
    {
        if (speechBubble == null)     yield break;
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