using UnityEngine;
using System;

[Serializable]
public class CustomerOrder
{
    public int orderId;
    public ItemData orderedFood;
    public int price;
    public float orderTime;
    public float patience = 60f; // Seconds before customer leaves
    public OrderStatus status = OrderStatus.Waiting;

    public enum OrderStatus
    {
        Waiting,    // Waiting for food to be cooked
        Ready,      // Food is ready, waiting for delivery
        Completed,  // Order delivered
        Failed      // Customer left (ran out of patience)
    }

    public float TimeRemaining()
    {
        return Mathf.Max(0, patience - (Time.time - orderTime));
    }

    public float PatiencePercent()
    {
        return TimeRemaining() / patience;
    }

    public bool IsExpired()
    {
        return TimeRemaining() <= 0;
    }
}