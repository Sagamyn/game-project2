using UnityEngine;
using System.Collections;

public class UIAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public AnimationType animationType = AnimationType.SlideFromTop;
    public float animationDuration = 0.3f;
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool disableOnHide = false; // Option to disable GameObject when hidden

    [Header("Slide Settings")]
    public float slideDistance = 1000f; // Distance to slide from

    [Header("Scale Settings")]
    public Vector3 startScale = new Vector3(0.8f, 0.8f, 1f);

    [Header("Fade Settings")]
    public bool fadeWithAnimation = true;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Vector3 originalScale;
    private Coroutine currentAnimation;

    public enum AnimationType
    {
        None,
        SlideFromTop,
        SlideFromBottom,
        SlideFromLeft,
        SlideFromRight,
        ScaleUp,
        Fade,
        SlideAndScale
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Store original values
        originalPosition = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;
    }

    public void Show()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        // Always activate when showing
        gameObject.SetActive(true);
        currentAnimation = StartCoroutine(AnimateShow());
    }

    public void Hide(System.Action onComplete = null)
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimateHide(onComplete));
    }

    IEnumerator AnimateShow()
    {
        // Set starting state
        SetStartState();

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsed / animationDuration);

            ApplyAnimation(t);

            yield return null;
        }

        // Ensure final state
        ApplyAnimation(1f);
        currentAnimation = null;
    }

    IEnumerator AnimateHide(System.Action onComplete)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - animationCurve.Evaluate(elapsed / animationDuration);

            ApplyAnimation(t);

            yield return null;
        }

        // Only disable if option is enabled
        if (disableOnHide)
        {
            gameObject.SetActive(false);
        }
        else
        {
            // Just make it invisible/off-screen
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        currentAnimation = null;
        onComplete?.Invoke();
    }

    void SetStartState()
    {
        switch (animationType)
        {
            case AnimationType.SlideFromTop:
                rectTransform.anchoredPosition = originalPosition + Vector2.up * slideDistance;
                break;
            case AnimationType.SlideFromBottom:
                rectTransform.anchoredPosition = originalPosition + Vector2.down * slideDistance;
                break;
            case AnimationType.SlideFromLeft:
                rectTransform.anchoredPosition = originalPosition + Vector2.left * slideDistance;
                break;
            case AnimationType.SlideFromRight:
                rectTransform.anchoredPosition = originalPosition + Vector2.right * slideDistance;
                break;
            case AnimationType.ScaleUp:
                rectTransform.localScale = startScale;
                break;
            case AnimationType.SlideAndScale:
                rectTransform.anchoredPosition = originalPosition + Vector2.up * slideDistance;
                rectTransform.localScale = startScale;
                break;
        }

        if (fadeWithAnimation)
            canvasGroup.alpha = 0f;
    }

    void ApplyAnimation(float t)
    {
        switch (animationType)
        {
            case AnimationType.SlideFromTop:
                rectTransform.anchoredPosition = Vector2.Lerp(
                    originalPosition + Vector2.up * slideDistance,
                    originalPosition,
                    t
                );
                break;
            case AnimationType.SlideFromBottom:
                rectTransform.anchoredPosition = Vector2.Lerp(
                    originalPosition + Vector2.down * slideDistance,
                    originalPosition,
                    t
                );
                break;
            case AnimationType.SlideFromLeft:
                rectTransform.anchoredPosition = Vector2.Lerp(
                    originalPosition + Vector2.left * slideDistance,
                    originalPosition,
                    t
                );
                break;
            case AnimationType.SlideFromRight:
                rectTransform.anchoredPosition = Vector2.Lerp(
                    originalPosition + Vector2.right * slideDistance,
                    originalPosition,
                    t
                );
                break;
            case AnimationType.ScaleUp:
                rectTransform.localScale = Vector3.Lerp(startScale, originalScale, t);
                break;
            case AnimationType.Fade:
                // Position stays the same, only fade
                break;
            case AnimationType.SlideAndScale:
                rectTransform.anchoredPosition = Vector2.Lerp(
                    originalPosition + Vector2.up * slideDistance,
                    originalPosition,
                    t
                );
                rectTransform.localScale = Vector3.Lerp(startScale, originalScale, t);
                break;
        }

        if (fadeWithAnimation)
        {
            canvasGroup.alpha = t;
        }

        // Control interactivity
        canvasGroup.blocksRaycasts = t > 0.5f;
        canvasGroup.interactable = t > 0.5f;
    }

    // Manual control methods
    public void ShowInstant()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        gameObject.SetActive(true);
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale = originalScale;
        canvasGroup.alpha = 1f;
    }

    public void HideInstant()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        if (disableOnHide)
        {
            gameObject.SetActive(false);
        }
        else
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}