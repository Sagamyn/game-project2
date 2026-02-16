using UnityEngine;

public class Rock : MonoBehaviour, IDamageable
{
    public int maxHP = 3;
    int hp;

    [Header("Drop")]
    public ResourceItem dropItem;
    public GameObject dropPrefab;
    public int minDrop = 1;
    public int maxDrop = 3;

    [Header("Burst")]
    public float burstForce = 3f;
    public float burstTorque = 6f;
    public float despawnTime = 10f;
    
    [Header("Physics")]
    public float gravityDelay = 0.3f; // Time before gravity kicks in
    public float pickupDelay = 0.5f; // Grace period before can be picked up

    void Awake()
    {
        hp = maxHP;
    }

    public void TakeDamage(int amount)
    {
        hp -= amount;

        if (hp <= 0)
            Die();
    }

    void Die()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.rockBreakSound);
        SpawnBurst();
        Destroy(gameObject);
    }

    void SpawnBurst()
    {
        int count = Random.Range(minDrop, maxDrop + 1);

        for (int i = 0; i < count; i++)
        {
            GameObject drop = Instantiate(dropPrefab, transform.position, Quaternion.identity);

            ItemPickup ip = drop.GetComponent<ItemPickup>();
            if (ip != null)
            {
                ip.item = dropItem;
                ip.amount = 1;
                
                // Add pickup grace period (can't pick up for 0.5 seconds)
                ip.SetPickupDelay(pickupDelay);
            }

            Rigidbody2D rb = drop.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f; // No gravity initially (floats)
                rb.drag = 5f;
                rb.angularDrag = 5f;

                Vector2 dir = Random.insideUnitCircle;
                dir.y = Mathf.Abs(dir.y); // Always burst upward

                rb.AddForce(
                    dir.normalized * burstForce,
                    ForceMode2D.Impulse
                );

                rb.AddTorque(
                    Random.Range(-burstTorque, burstTorque),
                    ForceMode2D.Impulse
                );
                
                // Enable gravity after delay for natural fall
                StartCoroutine(EnableGravityAfterDelay(rb, gravityDelay));
            }

            Destroy(drop, despawnTime);
        }
    }
    
    System.Collections.IEnumerator EnableGravityAfterDelay(Rigidbody2D rb, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rb != null)
        {
            rb.gravityScale = 1f; // Natural fall
        }
    }
}