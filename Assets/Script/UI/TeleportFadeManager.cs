using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles fade transitions for teleporters
/// Create one in your scene for fade effects
/// </summary>
public class TeleportFadeManager : MonoBehaviour
{
    [Header("Fade Panel")]
    public Image fadeImage;
    public Canvas fadeCanvas;

    [Header("Auto Setup")]
    [Tooltip("Create canvas and image automatically on start")]
    public bool autoSetup = true;

    void Awake()
    {
        if (autoSetup && fadeImage == null)
        {
            SetupFadeCanvas();
        }
    }

    void SetupFadeCanvas()
    {
        // Create canvas
        if (fadeCanvas == null)
        {
            GameObject canvasGO = new GameObject("FadeCanvas");
            canvasGO.transform.SetParent(transform);
            fadeCanvas = canvasGO.AddComponent<Canvas>();
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeCanvas.sortingOrder = 9999; // Always on top

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasGO.AddComponent<GraphicRaycaster>();

            Debug.Log("✓ Fade Canvas created automatically");
        }

        // Create fade image
        if (fadeImage == null)
        {
            GameObject imageGO = new GameObject("FadeImage");
            imageGO.transform.SetParent(fadeCanvas.transform, false);
            
            fadeImage = imageGO.AddComponent<Image>();
            fadeImage.color = Color.black;
            fadeImage.raycastTarget = false;

            RectTransform rect = fadeImage.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            Debug.Log("✓ Fade Image created automatically");
        }

        // Start transparent
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;
    }

    public IEnumerator FadeOut(float speed, Color targetColor)
    {
        if (fadeImage == null)
        {
            Debug.LogWarning("No fade image! Skipping fade.");
            yield break;
        }

        Color c = targetColor;
        c.a = 0f;
        fadeImage.color = c;

        while (c.a < 1f)
        {
            c.a += Time.deltaTime * speed;
            fadeImage.color = c;
            yield return null;
        }

        c.a = 1f;
        fadeImage.color = c;
    }

    public IEnumerator FadeIn(float speed)
    {
        if (fadeImage == null)
        {
            Debug.LogWarning("No fade image! Skipping fade.");
            yield break;
        }

        Color c = fadeImage.color;

        while (c.a > 0f)
        {
            c.a -= Time.deltaTime * speed;
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0f;
        fadeImage.color = c;
    }

    // Instant fade functions
    public void SetFadeAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = Mathf.Clamp01(alpha);
            fadeImage.color = c;
        }
    }

    public void ShowFade()
    {
        SetFadeAlpha(1f);
    }

    public void HideFade()
    {
        SetFadeAlpha(0f);
    }
}