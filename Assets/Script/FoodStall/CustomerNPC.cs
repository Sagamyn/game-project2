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
    public GameObject thoughtBubbleObject;
    public SpriteRenderer thoughtBubbleBackground;
    public SpriteRenderer orderedFoodIcon;
    public Vector3 bubbleOffset = new Vector3(0, 1.5f, 0);

    [Header("Movement")]
    public Transform seatPosition;
    public float moveSpeed = 2f;

    [Header("Attack Settings")]
    public int attackDamage = 15;

    private Restaurant restaurant;
    private bool isSeated = false;
    private bool isLeaving = false;

    public void Initialize(Restaurant restaurantRef, MenuItem menuItem, Transform seat)
    {
        restaurant = restaurantRef;
        seatPosition = seat;

        currentOrder = new CustomerOrder
        {
            orderId = Random.Range(1000, 9999),
            orderedFood = menuItem.food,
            price = menuItem.price,
            orderTime = Time.time,
            patience = Random.Range(45f, 90f),
            status = CustomerOrder.OrderStatus.Waiting
        };

        if (string.IsNullOrEmpty(customerName))
            customerName = $"Customer {currentOrder.orderId}";

        Debug.Log($"{customerName} ordered {currentOrder.orderedFood.itemName} for ${currentOrder.price}");

        if (thoughtBubbleObject != null)
            thoughtBubbleObject.SetActive(false);

        StartCoroutine(MoveToSeat());
    }

    IEnumerator MoveToSeat()
    {
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
        StartCoroutine(PatienceTimer());
    }

    void ShowOrder()
    {
        if (thoughtBubbleObject != null)
        {
            thoughtBubbleObject.SetActive(true);

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
                currentOrder.status = CustomerOrder.OrderStatus.Failed;
                Debug.Log($"{customerName} is ANGRY and attacks!");

                // Turn red to show anger visually
                if (spriteRenderer != null)
                    spriteRenderer.color = Color.red;

                // Hit the player directly — no range check needed
                PlayerHealth.Instance?.TakeDamage(attackDamage);

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

        if (thoughtBubbleObject != null)
            thoughtBubbleObject.SetActive(false);

        StartCoroutine(PayAndLeave());
    }

    IEnumerator PayAndLeave()
    {
        yield return new WaitForSeconds(2f);

        if (restaurant != null)
            restaurant.OnCustomerLeft(this, true);

        Leave();
    }

    public void LeaveImmediately()
    {
        if (isLeaving) return;

        isLeaving = true;

        Debug.Log($"🚪 {customerName} is leaving due to restaurant closure");

        StopAllCoroutines();

        if (currentOrder != null && currentOrder.status == CustomerOrder.OrderStatus.Waiting)
            currentOrder.status = CustomerOrder.OrderStatus.Failed;

        if (thoughtBubbleObject != null)
            thoughtBubbleObject.SetActive(false);

        StartCoroutine(MoveOut());
    }

    void Leave()
    {
        if (isLeaving) return;

        isLeaving = true;
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + bubbleOffset, 0.3f);
    }
}