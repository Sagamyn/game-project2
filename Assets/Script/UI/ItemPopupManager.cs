using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPopupManager : MonoBehaviour
{
public static ItemPopupManager Instance;

    [Header("Notification Prefab")]
    public GameObject notificationPrefab;
    public Transform notificationContainer;
    
    [Header("Settings")]
    public float displayDuration = 2f;
    public float fadeOutDuration = 0.5f;
    public float spacing = 10f;
    public int maxNotifications = 5;
    
    [Header("Animation")]
    public bool useSlideIn = true;
    public float slideDistance = 50f;
    public float slideSpeed = 0.3f;
    
    [Header("Stacking")]
    public bool stackSameItems = true;
    public float stackTimeWindow = 1f; // Combine pickups within 1 second
    
    [Header("Audio")]
    public AudioClip pickupSound;
    private AudioSource audioSource;

    private List<ItemPopup> activeNotifications = new List<ItemPopup>();
    private Dictionary<ItemData, ItemPopup> recentPickups = new Dictionary<ItemData, ItemPopup>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Setup audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    /// <summary>
    /// Show a pickup notification
    /// </summary>
    public void ShowPickup(ItemData item, int amount)
    {
        if (item == null)
        {
            Debug.LogWarning("Cannot show pickup notification for null item!");
            return;
        }

        // Check if we should stack with recent pickup
        if (stackSameItems && recentPickups.ContainsKey(item))
        {
            ItemPopup existingNotif = recentPickups[item];
            if (existingNotif != null && existingNotif.gameObject != null)
            {
                // Stack with existing notification
                existingNotif.AddAmount(amount);
                existingNotif.ResetTimer();
                Debug.Log($"✓ Stacked {amount}x {item.itemName} with existing notification");
                return;
            }
            else
            {
                // Notification expired, remove from dictionary
                recentPickups.Remove(item);
            }
        }

        // Create new notification
        CreateNotification(item, amount);
        
        // Play sound
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }

    void CreateNotification(ItemData item, int amount)
    {
        if (notificationPrefab == null)
        {
            Debug.LogError("Notification prefab not assigned!");
            return;
        }

        if (notificationContainer == null)
        {
            Debug.LogError("Notification container not assigned!");
            return;
        }

        // Check max notifications
        if (activeNotifications.Count >= maxNotifications)
        {
            // Remove oldest
            RemoveNotification(activeNotifications[0]);
        }

        // Instantiate notification
        GameObject notifObj = Instantiate(notificationPrefab, notificationContainer);
        ItemPopup notification = notifObj.GetComponent<ItemPopup>();
        
        if (notification == null)
        {
            Debug.LogError("Notification prefab doesn't have ItemPopup component!");
            Destroy(notifObj);
            return;
        }

        // Setup notification
        notification.Initialize(item, amount, this);
        activeNotifications.Add(notification);
        
        // Track for stacking
        if (stackSameItems)
        {
            recentPickups[item] = notification;
        }

        // Start animation
        if (useSlideIn)
        {
            StartCoroutine(SlideInAnimation(notification));
        }

        // Auto-remove after duration
        StartCoroutine(AutoRemoveNotification(notification, displayDuration));
        
        Debug.Log($"✓ Created pickup notification: +{amount} {item.itemName}");
    }

    IEnumerator SlideInAnimation(ItemPopup notification)
    {
        RectTransform rect = notification.GetComponent<RectTransform>();
        if (rect == null) yield break;

        Vector2 targetPos = rect.anchoredPosition;
        Vector2 startPos = targetPos + Vector2.right * slideDistance;
        
        rect.anchoredPosition = startPos;

        float elapsed = 0f;
        while (elapsed < slideSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideSpeed;
            t = Mathf.SmoothStep(0, 1, t); // Smooth easing
            
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        rect.anchoredPosition = targetPos;
    }

    IEnumerator AutoRemoveNotification(ItemPopup notification, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (notification != null && notification.gameObject != null)
        {
            RemoveNotification(notification);
        }
    }

    public void RemoveNotification(ItemPopup notification)
    {
        if (notification == null) return;

        activeNotifications.Remove(notification);
        
        // Remove from recent pickups
        if (recentPickups.ContainsValue(notification))
        {
            ItemData keyToRemove = null;
            foreach (var kvp in recentPickups)
            {
                if (kvp.Value == notification)
                {
                    keyToRemove = kvp.Key;
                    break;
                }
            }
            if (keyToRemove != null)
            {
                recentPickups.Remove(keyToRemove);
            }
        }

        // Fade out and destroy
        StartCoroutine(FadeOutAndDestroy(notification));
    }

    IEnumerator FadeOutAndDestroy(ItemPopup notification)
    {
        CanvasGroup canvasGroup = notification.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = notification.gameObject.AddComponent<CanvasGroup>();
        }

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - (elapsed / fadeOutDuration);
            yield return null;
        }

        Destroy(notification.gameObject);
    }

    // Clean up old entries in recent pickups
    void Update()
    {
        // Clean up null references
        List<ItemData> toRemove = new List<ItemData>();
        foreach (var kvp in recentPickups)
        {
            if (kvp.Value == null || kvp.Value.gameObject == null)
            {
                toRemove.Add(kvp.Key);
            }
        }
        
        foreach (var key in toRemove)
        {
            recentPickups.Remove(key);
        }
    }
}
