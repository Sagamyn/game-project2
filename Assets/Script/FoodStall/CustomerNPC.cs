using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CustomerNPC : MonoBehaviour
{
    [Header("Customer Info")]
    public string customerName;
    public Sprite customerSprite;

    [Header("Order")]
    public CustomerOrder currentOrder;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public GameObject thoughtBubble;
    public Image orderedFoodIcon;

    [Header("Movement")]
    public Transform seatPosition;
    public float moveSpeed = 2f;

    private Restaurant restaurant;
    private bool isSeated = false;

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
        if (thoughtBubble != null)
            thoughtBubble.SetActive(true);

        if (orderedFoodIcon != null && currentOrder.orderedFood != null)
            orderedFoodIcon.sprite = currentOrder.orderedFood.icon;
    }

    IEnumerator PatienceTimer()
    {
        while (currentOrder.status == CustomerOrder.OrderStatus.Waiting)
        {
            if (currentOrder.IsExpired())
            {
                // Customer leaves angry
                currentOrder.status = CustomerOrder.OrderStatus.Failed;
                Debug.Log($"{customerName} left angry!");
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
        if (thoughtBubble != null)
            thoughtBubble.SetActive(false);

        // Pay and leave
        StartCoroutine(PayAndLeave());
    }

    IEnumerator PayAndLeave()
    {
        yield return new WaitForSeconds(2f); // Eating time

        // Notify restaurant
        restaurant.OnCustomerLeft(this, true);

        Leave();
    }

    void Leave()
    {
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
    }
}