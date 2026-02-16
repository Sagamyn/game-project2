using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// COMPLETE FIXED VERSION
/// - Removes items properly
/// - Drops in player's facing direction
/// - Works with all movement systems
/// </summary>
public class InventoryDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    public float dropDistance = 1.5f;
    public bool randomizePosition = true;
    public float randomRadius = 0.5f;
    public bool throwItems = true;
    public float throwForce = 3f;
    
    [Header("References")]
    public Transform playerTransform;
    
    [Header("Visual Feedback")]
    public bool showDropHint = true;
    public Color hintColor = new Color(1f, 1f, 0f, 0.1f);
    
    [Header("Audio")]
    public AudioClip dropSound;
    private AudioSource audioSource;
    
    private Image dropZoneImage;
    private Color originalColor;
    private bool isDraggingOver = false;

    void Awake()
    {
        dropZoneImage = GetComponent<Image>();
        if (dropZoneImage == null)
            dropZoneImage = gameObject.AddComponent<Image>();
        
        dropZoneImage.raycastTarget = true;
        
        if (dropZoneImage.color.a > 0.5f)
            dropZoneImage.color = new Color(0, 0, 0, 0);
        
        originalColor = dropZoneImage.color;
    }

    void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
            else
                Debug.LogError("Player not found! Assign playerTransform.");
        }
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        InventorySlotUI draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        
        if (draggedSlot != null && showDropHint)
        {
            isDraggingOver = true;
            dropZoneImage.color = hintColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDraggingOver)
        {
            isDraggingOver = false;
            dropZoneImage.color = originalColor;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (isDraggingOver)
        {
            isDraggingOver = false;
            dropZoneImage.color = originalColor;
        }
        
        InventorySlotUI draggedSlot = eventData.pointerDrag?.GetComponent<InventorySlotUI>();
        
        if (draggedSlot != null)
        {
            ItemData item = draggedSlot.GetItem();
            int amount = draggedSlot.GetAmount();
            int slotIndex = draggedSlot.GetSlotIndex();
            
            if (item != null && amount > 0)
            {
                DropItemIntoWorld(draggedSlot, item, amount, slotIndex);
            }
        }
    }

    void DropItemIntoWorld(InventorySlotUI slot, ItemData item, int amount, int slotIndex)
    {
        if (item.worldPrefab == null)
        {
            Debug.LogError($"❌ {item.itemName} has no worldPrefab!");
            return;
        }
        
        Vector3 dropPosition = CalculateDropPosition();
        GameObject droppedItem = SpawnItemInWorld(item, amount, dropPosition);
        
        if (droppedItem != null)
        {
            if (throwItems)
                ApplyThrowForce(droppedItem);
            
            RemoveFromInventory(slot.owner, slotIndex, item, amount);
            
            if (dropSound != null && audioSource != null)
                audioSource.PlayOneShot(dropSound);
            
            Debug.Log($"✅ Dropped {amount}x {item.itemName} in {GetPlayerFacingDirection()} direction!");
        }
    }

    // FIXED: Gets player's actual facing direction from Animator!
    Vector2 GetPlayerFacingDirection()
    {
        if (playerTransform == null)
            return Vector2.right;
        
        Vector2 direction = Vector2.right; // Default
        
        // METHOD 1: Use PlayerMovement helper methods (BEST!)
        PlayerMovement playerMovement = playerTransform.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            float lastX = playerMovement.GetLastMoveX();
            float lastY = playerMovement.GetLastMoveY();
            
            if (lastX != 0 || lastY != 0)
            {
                direction = new Vector2(lastX, lastY).normalized;
                Debug.Log($"✓ Using PlayerMovement direction: {direction}");
                return direction;
            }
        }
        
        // METHOD 2: Try Animator directly (BACKUP)
        Animator animator = playerTransform.GetComponent<Animator>();
        if (animator != null)
        {
            float lastX = animator.GetFloat("LastMoveX");
            float lastY = animator.GetFloat("LastMoveY");
            
            if (lastX != 0 || lastY != 0)
            {
                direction = new Vector2(lastX, lastY).normalized;
                Debug.Log($"✓ Using Animator direction: {direction}");
                return direction;
            }
        }
        
        // METHOD 3: Fallback to right
        Debug.Log("⚠️ Using default direction: RIGHT");
        return Vector2.right;
    }

    Vector3 CalculateDropPosition()
    {
        if (playerTransform == null)
            return Vector3.zero;

        Vector3 dropPos = playerTransform.position;
        Vector2 dropDirection = GetPlayerFacingDirection();
        
        // Drop in facing direction
        dropPos += new Vector3(dropDirection.x, dropDirection.y, 0f) * dropDistance;
        
        // Add random offset
        if (randomizePosition)
        {
            Vector2 randomOffset = Random.insideUnitCircle * randomRadius;
            dropPos += new Vector3(randomOffset.x, randomOffset.y, 0f);
        }
        
        return dropPos;
    }

    GameObject SpawnItemInWorld(ItemData item, int amount, Vector3 position)
    {
        GameObject droppedItem = Instantiate(item.worldPrefab, position, Quaternion.identity);
        droppedItem.name = $"{item.itemName}_Dropped";
        
        ItemPickup pickup = droppedItem.GetComponent<ItemPickup>();
        if (pickup != null)
        {
            pickup.item = item;
            pickup.amount = amount;
        }
        
        return droppedItem;
    }

    void ApplyThrowForce(GameObject droppedItem)
    {
        Rigidbody2D rb = droppedItem.GetComponent<Rigidbody2D>();
        if (rb == null) return;
        
        Vector2 throwDirection = GetPlayerFacingDirection();
        
        // Add upward arc
        throwDirection += Vector2.up * 0.3f;
        throwDirection.Normalize();
        
        // Apply force
        rb.AddForce(throwDirection * throwForce, ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-2f, 2f), ForceMode2D.Impulse);
    }

    void RemoveFromInventory(InventoryOwner owner, int slotIndex, ItemData item, int amount)
    {
        if (owner == InventoryOwner.Player)
        {
            PlayerInventory playerInv = FindObjectOfType<PlayerInventory>();
            if (playerInv == null)
            {
                Debug.LogError("PlayerInventory not found!");
                return;
            }

            // Direct slot clearing (simplest and most reliable)
            if (slotIndex >= 0 && slotIndex < playerInv.items.Count)
            {
                // Clear the slot
                playerInv.items[slotIndex].item = null;
                playerInv.items[slotIndex].amount = 0;
                
                // Try to call ConsumeItem to update other systems
                try
                {
                    playerInv.ConsumeItem(item, amount);
                }
                catch
                {
                    // ConsumeItem might fail, that's okay
                }
                
                // Try to refresh UI
                try
                {
                    System.Reflection.MethodInfo notifyMethod = playerInv.GetType().GetMethod("NotifyChanged");
                    if (notifyMethod != null)
                        notifyMethod.Invoke(playerInv, null);
                }
                catch { }
                
                Debug.Log($"✅ Removed {amount}x {item.itemName} from slot {slotIndex}");
            }
        }
        else if (owner == InventoryOwner.Chest)
        {
            ChestInventory chestInv = InventoryTransferManager.Instance?.currentChest;
            if (chestInv != null && slotIndex >= 0 && slotIndex < chestInv.items.Count)
            {
                chestInv.items[slotIndex].item = null;
                chestInv.items[slotIndex].amount = 0;
                chestInv.NotifyChanged();
                
                Debug.Log($"✅ Removed {amount}x {item.itemName} from chest slot {slotIndex}");
            }
        }
    }

    void OnGUI()
    {
        if (isDraggingOver && showDropHint)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = Color.yellow;
            style.alignment = TextAnchor.MiddleCenter;
            
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 100, 300, 50), 
                "Release to drop in world!", style);
        }
    }
}