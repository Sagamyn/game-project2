using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData item;
    public int amount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerInventory inventory =
            other.GetComponent<PlayerInventory>() ??
            other.GetComponentInParent<PlayerInventory>();

        if (inventory == null || item == null)
            return;

        inventory.AddItem(item, amount);

        Debug.Log($"Picked up {item.itemName} x{amount}");

        Destroy(gameObject);
    }
}
