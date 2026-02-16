using UnityEngine;

/// <summary>
/// Makes an item pickupable in the game world
/// Uses ItemPopupManager for notifications!
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData item;
    public int amount = 1;
    
    [Header("Pickup Settings")]
    public float pickupRadius = 1f;
    public bool autoPickup = true;
    public KeyCode pickupKey = KeyCode.E;
    
    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public bool bobAnimation = true;
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;
    
    [Header("Audio")]
    public AudioClip pickupSound;
    
    [Header("Notification")]
    public bool showNotification = true;
    
    [Header("Grace Period")]
    public float pickupGracePeriod = 0f; // Can't be picked up for this many seconds
    
    private bool playerNearby = false;
    private Vector3 startPosition;
    private float bobTimer = 0f;
    private float spawnTime;
    private bool canBePickedUp = true;

    void Start()
    {
        // Get sprite renderer if not assigned
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Set sprite from item
        if (spriteRenderer != null && item != null && item.icon != null)
        {
            spriteRenderer.sprite = item.icon;
        }
        
        // Setup collider as trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
        
        startPosition = transform.position;
        
        // Random bob offset
        bobTimer = Random.Range(0f, Mathf.PI * 2f);
        
        // Record spawn time
        spawnTime = Time.time;
        
        // Start grace period if needed
        if (pickupGracePeriod > 0f)
        {
            canBePickedUp = false;
            StartCoroutine(EnablePickupAfterDelay(pickupGracePeriod));
        }
    }
    
    /// <summary>
    /// Set pickup delay (called by Tree/Rock when spawning items)
    /// </summary>
    public void SetPickupDelay(float delay)
    {
        pickupGracePeriod = delay;
        canBePickedUp = false;
        StartCoroutine(EnablePickupAfterDelay(delay));
    }
    
    System.Collections.IEnumerator EnablePickupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canBePickedUp = true;
        Debug.Log($"✓ {item?.itemName} can now be picked up");
    }

    void Update()
    {
        // Bob animation
        if (bobAnimation)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float yOffset = Mathf.Sin(bobTimer) * bobHeight;
            transform.position = startPosition + Vector3.up * yOffset;
        }
        
        // Manual pickup with key
        if (playerNearby && !autoPickup && Input.GetKeyDown(pickupKey))
        {
            TryPickup();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            
            if (autoPickup)
            {
                TryPickup();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }

    void TryPickup()
    {
        // Check grace period
        if (!canBePickedUp)
        {
            Debug.Log($"⏳ {item?.itemName} is in grace period, can't pick up yet");
            return;
        }
        
        if (item == null)
        {
            Debug.LogWarning("ItemPickup has no item assigned!");
            Destroy(gameObject);
            return;
        }

        // Find player inventory
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
        if (inventory == null)
        {
            Debug.LogError("Player inventory not found!");
            return;
        }

        // Try to add to inventory
        bool success = inventory.AddItem(item, amount);
        
        if (success)
        {
            // Show notification using ItemPopupManager!
            if (showNotification && ItemPopupManager.Instance != null)
            {
                ItemPopupManager.Instance.ShowPickup(item, amount);
            }
            
            // Play pickup sound
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            
            Debug.Log($"✓ Picked up {amount}x {item.itemName}");
            
            // Destroy the pickup
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning($"Inventory full! Cannot pick up {item.itemName}");
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw pickup radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}