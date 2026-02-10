using UnityEngine;
using System.Collections;

public class CustomerNPC : MonoBehaviour
{
    [Header("Customer Info")]
    public string customerName;
    public Sprite customerSprite;

    [Header("Order")]
    public CustomerOrder currentOrder;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    
    [Header("Thought Bubble - World Space")]
    public GameObject thoughtBubbleObject; // Parent GameObject for the bubble
    public SpriteRenderer thoughtBubbleBackground; // Bubble background sprite
    public SpriteRenderer orderedFoodIcon; // The food icon sprite
    public Vector3 bubbleOffset = new Vector3(0, 1.5f, 0); // Offset above customer's head

    [Header("Movement")]
    public Transform seatPosition;
    public float moveSpeed = 2f;

    private Restaurant restaurant;
    private bool isSeated = false;
    private bool isLeaving = false;

    public void Initialize(Restaurant restaurantRef, MenuItem menuItem, Transform seat)
    {
        restaurant = restaurantRef;
        seatPosition = seat;

        // Create order
        currentOrder = new CustomerOrder
        {
            orderId = Random.Range(1000, 9999),
            orderedFood = menuItem.food,
            price = menuItem.price,
            orderTime = Time.time,
            patience = Random.Range(45f, 90f),
            status = CustomerOrder.OrderStatus.Waiting
        };

        // Random name if not set
        if (string.IsNullOrEmpty(customerName))
            customerName = $"Customer {currentOrder.orderId}";

        Debug.Log($"{customerName} ordered {currentOrder.orderedFood.itemName} for ${currentOrder.price}");

        // Hide bubble initially
        if (thoughtBubbleObject != null)
            thoughtBubbleObject.SetActive(false);

        // Move to seat
        StartCoroutine(MoveToSeat());
    }

    IEnumerator MoveToSeat()
    {
        // Move to seat position
        while (Vector3.Distance(transform.position, seatPosition.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                seatPosition.position,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        isSeated = true;
        ShowOrder();

        // Start patience timer
        StartCoroutine(PatienceTimer());
    }

    void ShowOrder()
    {
        // Show thought bubble
        if (thoughtBubbleObject != null)
        {
            thoughtBubbleObject.SetActive(true);
            
            // Set food icon
            if (orderedFoodIcon != null && currentOrder.orderedFood != null)
            {
                orderedFoodIcon.sprite = currentOrder.orderedFood.icon;
                orderedFoodIcon.enabled = true;
            }
            
            Debug.Log($"✓ Showing thought bubble for {customerName}");
        }
        else
        {
            Debug.LogWarning($"⚠ ThoughtBubbleObject is NULL on {customerName}!");
        }
    }

    void Update()
    {
        // Keep thought bubble above customer's head
        if (thoughtBubbleObject != null && thoughtBubbleObject.activeSelf)
        {
            thoughtBubbleObject.transform.position = transform.position + bubbleOffset;
        }
    }

    IEnumerator PatienceTimer()
    {
        while (currentOrder.status == CustomerOrder.OrderStatus.Waiting && !isLeaving)
        {
            if (currentOrder.IsExpired())
            {
                // Customer leaves angry
                currentOrder.status = CustomerOrder.OrderStatus.Failed;
                Debug.Log($"{customerName} left angry!");
                
                if (restaurant != null)
                    restaurant.OnCustomerLeft(this, false);
                    
                Leave();
                yield break;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public void ReceiveFood(ItemData food)
    {
        if (currentOrder.orderedFood != food)
        {
            Debug.LogWarning("Wrong food!");
            return;
        }

        currentOrder.status = CustomerOrder.OrderStatus.Completed;

        Debug.Log($"{customerName} received {food.itemName}!");

        // Hide thought bubble
        if (thoughtBubbleObject != null)
            thoughtBubbleObject.SetActive(false);

        // Pay and leave
        StartCoroutine(PayAndLeave());
    }

    IEnumerator PayAndLeave()
    {
        yield return new WaitForSeconds(2f); // Eating time

        // Notify restaurant
        if (restaurant != null)
            restaurant.OnCustomerLeft(this, true);

        Leave();
    }

    // Called when restaurant closes mid-session
    public void LeaveImmediately()
    {
        if (isLeaving) return; // Prevent multiple calls
        
        isLeaving = true;
        
        Debug.Log($"🚪 {customerName} is leaving due to restaurant closure");
        
        // Stop all coroutines (including patience timer)
        StopAllCoroutines();
        
        // Update order status
        if (currentOrder != null && currentOrder.status == CustomerOrder.OrderStatus.Waiting)
        {
            currentOrder.status = CustomerOrder.OrderStatus.Failed;
        }
        
        // Hide thought bubble
        if (thoughtBubbleObject != null)
            thoughtBubbleObject.SetActive(false);
        
        // Start leaving immediately
        StartCoroutine(MoveOut());
    }

    void Leave()
    {
        if (isLeaving) return; // Already leaving
        
        isLeaving = true;
        
        // Move out of restaurant
        StartCoroutine(MoveOut());
    }

    IEnumerator MoveOut()
    {
        Vector3 exitPosition = transform.position + Vector3.right * 10f;

        while (Vector3.Distance(transform.position, exitPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                exitPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        if (seatPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, seatPosition.position);
        }

        // Draw bubble position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + bubbleOffset, 0.3f);
    }
}