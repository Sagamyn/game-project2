using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Handles all visual feedback for the Cooking Temperature Minigame.
/// Attach to the same GameObject as CookingTemperatureMinigame.
/// 
/// Features:
/// - Catch bar glow when target is inside
/// - Progress bar color gradient (green → yellow → red)
/// - Screen shake when progress drops
/// - Instruction text with auto-fade
/// - Result popup ("BERHASIL!" / "GOSONG!") with scale-punch animation
/// - Background dim overlay
/// </summary>
public class CookingMinigameUI : MonoBehaviour
{
    [Header("Shared References (sama dengan CookingTemperatureMinigame)")]
    public RectTransform catchBar;
    public RectTransform target;
    public Slider progressBar;

    [Header("Visual References")]
    public Image catchBarImage;
    public Image targetImage;
    public Image progressFillImage;
    public Image backgroundDim;
    public RectTransform minigameContainer;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI resultText;

    [Header("Catch Bar Colors")]
    public Color catchBarNormal = new Color(0.29f, 0.87f, 0.5f, 0.7f);
    public Color catchBarGlow = new Color(0.53f, 0.94f, 0.67f, 1f);
    public float colorLerpSpeed = 10f;

    [Header("Progress Colors")]
    public Color progressGood = new Color(0.13f, 0.77f, 0.37f);
    public Color progressMid = new Color(0.92f, 0.70f, 0.03f);
    public Color progressLow = new Color(0.94f, 0.27f, 0.27f);

    [Header("Effects")]
    public float shakeIntensity = 4f;
    public float shakeDuration = 0.12f;
    public float pulseSpeed = 4f;
    public float resultDisplayTime = 1.2f;

    [Header("Result Colors")]
    public Color successColor = new Color(0.2f, 0.95f, 0.35f);
    public Color failColor = new Color(0.95f, 0.25f, 0.2f);

    // ─── Private State ───────────────────────────────
    private float lastProgress;
    private float shakeTimer;
    private Vector2 originalContainerPos;
    private bool isMinigameActive;
    private Color currentCatchBarColor;
    private Coroutine instructionCoroutine;
    private Coroutine resultCoroutine;

    // ─── Public API (dipanggil oleh CookingTemperatureMinigame) ───

    /// <summary>
    /// Dipanggil saat minigame mulai.
    /// </summary>
    public void OnMinigameStart()
    {
        isMinigameActive = true;
        lastProgress = 50f;
        shakeTimer = 0f;

        if (minigameContainer != null)
            originalContainerPos = minigameContainer.anchoredPosition;

        // Fade in background dim (Disabled per user request)
        if (backgroundDim != null)
        {
            backgroundDim.color = Color.clear;
            backgroundDim.gameObject.SetActive(false);
        }

        // Hapus juga background tipis dari container (akar masalahnya)
        if (minigameContainer != null)
        {
            Image containerImg = minigameContainer.GetComponent<Image>();
            if (containerImg != null) containerImg.color = Color.clear;
        }

        Image selfImg = GetComponent<Image>();
        if (selfImg != null) selfImg.color = Color.clear;

        // Show instruction text
        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(true);
            instructionText.text = "Klik Tahan untuk Menaikkan!";
            instructionText.alpha = 1f;
            if (instructionCoroutine != null) StopCoroutine(instructionCoroutine);
            instructionCoroutine = StartCoroutine(FadeOutInstruction(2.5f));
        }

        // Hide result text
        if (resultText != null)
            resultText.gameObject.SetActive(false);

        // Reset catch bar color
        currentCatchBarColor = catchBarNormal;
        if (catchBarImage != null)
            catchBarImage.color = currentCatchBarColor;
    }

    /// <summary>
    /// Dipanggil saat minigame selesai (berhasil atau gagal).
    /// </summary>
    public void OnMinigameEnd(bool success)
    {
        isMinigameActive = false;

        // Stop instruction fade if still running
        if (instructionCoroutine != null)
        {
            StopCoroutine(instructionCoroutine);
            instructionCoroutine = null;
        }
        if (instructionText != null)
            instructionText.gameObject.SetActive(false);

        // Show result popup
        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = success ? "BERHASIL!" : "GOSONG!";
            resultText.color = success ? successColor : failColor;
            resultText.alpha = 1f;

            if (resultCoroutine != null) StopCoroutine(resultCoroutine);
            resultCoroutine = StartCoroutine(ResultPunchAnimation(resultText.rectTransform));
        }

        // Fade out background (Disabled per user request)
        if (backgroundDim != null)
        {
            backgroundDim.gameObject.SetActive(false);
            // backgroundDim.CrossFadeAlpha(0f, 0.6f, false);
        }

        // Reset shake position
        if (minigameContainer != null)
            minigameContainer.anchoredPosition = originalContainerPos;

        // Reset catch bar scale
        if (catchBar != null)
            catchBar.localScale = Vector3.one;
    }

    // ─── Update Loop ─────────────────────────────────

    void Update()
    {
        if (!isMinigameActive) return;
        if (progressBar == null || catchBar == null || target == null) return;

        float progress = progressBar.value;

        UpdateCatchBarGlow();
        UpdateProgressColor(progress);
        UpdateShake(progress);

        lastProgress = progress;
    }

    // ─── Visual Effects ──────────────────────────────

    private void UpdateCatchBarGlow()
    {
        if (catchBarImage == null) return;

        bool isInside = IsTargetInsideCatchBar();
        Color targetColor = isInside ? catchBarGlow : catchBarNormal;

        currentCatchBarColor = Color.Lerp(
            currentCatchBarColor, targetColor,
            Time.deltaTime * colorLerpSpeed
        );
        catchBarImage.color = currentCatchBarColor;

        // Subtle horizontal pulse when target is inside
        if (isInside)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.04f;
            catchBar.localScale = new Vector3(pulse, 1f, 1f);
        }
        else
        {
            catchBar.localScale = Vector3.Lerp(
                catchBar.localScale, Vector3.one,
                Time.deltaTime * 8f
            );
        }
    }

    private void UpdateProgressColor(float progress)
    {
        if (progressFillImage == null) return;

        Color color;
        if (progress > 60f)
            color = Color.Lerp(progressMid, progressGood, (progress - 60f) / 40f);
        else if (progress > 30f)
            color = Color.Lerp(progressLow, progressMid, (progress - 30f) / 30f);
        else
            color = progressLow;

        // Pulse alpha when critically low
        if (progress < 20f)
        {
            float pulse = Mathf.PingPong(Time.time * 3f, 1f);
            color.a = Mathf.Lerp(0.55f, 1f, pulse);
        }

        progressFillImage.color = color;
    }

    private void UpdateShake(float progress)
    {
        if (minigameContainer == null) return;

        // Trigger shake when progress drops sharply
        float delta = progress - lastProgress;
        if (delta < -0.3f)
            shakeTimer = shakeDuration;

        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            float t = shakeTimer / shakeDuration;
            float intensity = shakeIntensity * t;
            Vector2 offset = new Vector2(
                Random.Range(-intensity, intensity),
                Random.Range(-intensity, intensity)
            );
            minigameContainer.anchoredPosition = originalContainerPos + offset;
        }
        else
        {
            minigameContainer.anchoredPosition = originalContainerPos;
        }
    }

    // ─── Helpers ─────────────────────────────────────

    private bool IsTargetInsideCatchBar()
    {
        float targetY = target.anchoredPosition.y;
        float catchY = catchBar.anchoredPosition.y;
        float catchHeight = catchBar.sizeDelta.y;
        float half = catchHeight / 2f;
        return targetY >= catchY - half && targetY <= catchY + half;
    }

    // ─── Coroutines ──────────────────────────────────

    private IEnumerator FadeOutInstruction(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (instructionText == null) yield break;

        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            instructionText.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        instructionText.gameObject.SetActive(false);
        instructionText.alpha = 1f; // Reset untuk pemakaian berikutnya
        instructionCoroutine = null;
    }

    private IEnumerator ResultPunchAnimation(RectTransform rt)
    {
        // Scale punch: 0 → 1.4 → 1.0
        float punchDuration = 0.35f;
        float elapsed = 0f;

        while (elapsed < punchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / punchDuration;

            float scale;
            if (t < 0.5f)
            {
                // Expand: 0 → 1.4
                scale = Mathf.Lerp(0f, 1.4f, t / 0.5f);
            }
            else
            {
                // Settle: 1.4 → 1.0
                float t2 = (t - 0.5f) / 0.5f;
                scale = Mathf.Lerp(1.4f, 1f, t2 * t2); // ease-in
            }

            rt.localScale = Vector3.one * scale;
            yield return null;
        }
        rt.localScale = Vector3.one;

        // Hold
        yield return new WaitForSeconds(resultDisplayTime);

        // Fade out
        TextMeshProUGUI txt = rt.GetComponent<TextMeshProUGUI>();
        if (txt != null)
        {
            float fadeDuration = 0.4f;
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                txt.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            txt.alpha = 1f; // Reset
        }

        if (resultText != null)
            resultText.gameObject.SetActive(false);

        resultCoroutine = null;
    }
}
