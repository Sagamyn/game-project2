using UnityEngine;
using System.Collections;


public class EKeyPrompt : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer promptRenderer;
    public Sprite[] frames;             // 2 frames of your E key art

    [Header("Position")]
    public Vector3 offset = new Vector3(0f, 1.5f, 0f); // above the object

    [Header("Animation")]
    public float frameRate    = 0.4f;   // seconds per frame
    public float bobAmount    = 0.15f;  // how much it bobs up/down
    public float bobSpeed     = 2f;

    private bool isVisible    = false;
    private Coroutine animCoroutine;
    private Transform parentTransform;

    void Awake()
    {
        if (promptRenderer == null)
            promptRenderer = GetComponentInChildren<SpriteRenderer>();

        // Store the parent so we always compute position relative to it
        parentTransform = transform.parent != null ? transform.parent : transform;

        // Start hidden
        if (promptRenderer != null)
            promptRenderer.enabled = false;
    }

    public void Show()
    {
        if (isVisible) return;
        isVisible = true;

        if (promptRenderer != null)
            promptRenderer.enabled = true;

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimatePrompt());
    }

    public void Hide()
    {
        if (!isVisible) return;
        isVisible = false;

        if (animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
            animCoroutine = null;
        }

        if (promptRenderer != null)
            promptRenderer.enabled = false;
    }

    IEnumerator AnimatePrompt()
    {
        int frameIndex  = 0;
        float bobTimer  = 0f;
        float frameTimer = 0f;

        while (isVisible)
        {
            // Swap frames
            frameTimer += Time.deltaTime;
            if (frameTimer >= frameRate)
            {
                frameTimer = 0f;
                frameIndex = (frameIndex + 1) % frames.Length;

                if (promptRenderer != null && frames.Length > 0)
                    promptRenderer.sprite = frames[frameIndex];
            }

            // Bob up and down
            bobTimer += Time.deltaTime * bobSpeed;
            float yOffset = Mathf.Sin(bobTimer) * bobAmount;

            // Always compute from parent's current world position — never accumulate
            transform.position = parentTransform.position + offset + Vector3.up * yOffset;

            yield return null;
        }
    }
}