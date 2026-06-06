using UnityEngine;

[CreateAssetMenu(fileName = "NewCustomer", menuName = "FoodStall/Customer Data")]
public class CustomerData : ScriptableObject
{
    [Header("Identity")]
    public string customerName = "Customer";

    [Header("Art")]
    public Sprite idleSprite;    // Default/waiting
    public Sprite happySprite;   // When served correctly
    public Sprite angrySprite;   // When patience runs out or wrong food

    [Header("Order")]
    public ItemData[] possibleOrders; // Random order picked from this list

    [Header("Patience")]
    public float patience = 15f;

    [Header("Reward")]
    public int payAmount = 50;
}