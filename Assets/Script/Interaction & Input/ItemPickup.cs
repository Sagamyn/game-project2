using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    public ItemData item;
    public int amount = 1;

    [Header("Pickup Delay")]
    public float pickupDelay = 1f;

    bool canPickup = false;
    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;

        // Disable pickup at spawn
        canPickup = false;
    }

    void Start()
    {
        // Enable pickup after delay
        Invoke(nameof(EnablePickup), pickupDelay);
    }

    void EnablePickup()
    {
        canPickup = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!canPickup)
            return;

        PlayerInventory inventory =
            other.GetComponent<PlayerInventory>();

        if (inventory == null)
            return;

        inventory.AddItem(item, amount);
        Destroy(gameObject);
    }
}
