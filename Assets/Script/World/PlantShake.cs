using UnityEngine;
using System.Collections;

/// <summary>
/// Makes crops/plants shake when player ENTERS their radius
/// Only shakes ONCE when player first gets close, not continuously
/// </summary>
public class PlantShake : MonoBehaviour
{
    [Header("Detection")]
    public float shakeRadius = 1.5f;
    
    [Header("Shake Settings")]
    public float shakeDuration = 0.3f;
    public float shakeIntensity = 0.1f;
    public float shakeSpeed = 20f;
    
    [Header("Visual")]
    public bool scaleShake = true;
    public bool rotationShake = true;
    public float maxRotation = 10f;
    
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private bool isShaking = false;
    
    // Track whether player was already inside radius
    private bool playerWasInside = false;
    private Transform playerTransform;

    void Start()
    {
        originalScale = transform.localScale;
        originalRotation = transform.localRotation;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform == null) return;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool playerIsInside = distance <= shakeRadius;
        
        // Only shake when player ENTERS the radius (was outside, now inside)
        if (playerIsInside && !playerWasInside && !isShaking)
        {
            StartCoroutine(Shake());
        }
        
        // Update state
        playerWasInside = playerIsInside;
    }

    IEnumerator Shake()
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;
            float currentIntensity = shakeIntensity * (1f - progress);

            if (rotationShake)
            {
                float angle = Mathf.Sin(Time.time * shakeSpeed) * maxRotation * (1f - progress);
                transform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, angle);
            }

            if (scaleShake)
            {
                float scaleOffset = Mathf.Sin(Time.time * shakeSpeed * 2f) * currentIntensity;
                transform.localScale = originalScale + new Vector3(scaleOffset, -scaleOffset * 0.5f, 0f);
            }

            yield return null;
        }

        // Always reset to original
        transform.localScale = originalScale;
        transform.localRotation = originalRotation;
        isShaking = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, shakeRadius);
    }
}