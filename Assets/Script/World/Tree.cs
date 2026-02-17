using UnityEngine;
using System.Collections;

public class Tree : MonoBehaviour, IDamageable
{
    public int maxHP = 3;
    int hp;

    [Header("Drop")]
    public ResourceItem dropItem;
    public GameObject dropPrefab;
    public int minDrop = 1;
    public int maxDrop = 3;

    [Header("Burst Settings - INCREASE THESE!")]
    public float burstForce = 15f; // MUCH STRONGER! (was 3)
    public float upwardBias = 2f; // Extra upward force
    public float burstTorque = 100f;
    public float despawnTime = 10f;
    
    [Header("Physics")]
    public float gravityDelay = 0.5f; // Longer float time so you can SEE it
    public float pickupDelay = 0.5f;
    
    [Header("Visual Fix")]
    public float spawnHeight = 1f; // Spawn higher above tree

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
        AudioManager.Instance.PlaySFX(AudioManager.Instance.axeBreakSound);
        SpawnBurst();
        Destroy(gameObject);
    }

    void SpawnBurst()
    {
        int count = Random.Range(minDrop, maxDrop + 1);
        
        // Spawn HIGHER above tree so you can see the burst!
        Vector3 spawnCenter = transform.position + Vector3.up * spawnHeight;

        for (int i = 0; i < count; i++)
        {
            // Spread items in a circle pattern
            float angle = (360f / count) * i + Random.Range(-30f, 30f);
            Vector2 spawnOffset = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ) * 0.5f;
            
            Vector3 spawnPos = spawnCenter + new Vector3(spawnOffset.x, spawnOffset.y, 0f);
            GameObject drop = Instantiate(dropPrefab, spawnPos, Quaternion.identity);

            // Setup pickup
            ItemPickup ip = drop.GetComponent<ItemPickup>();
            if (ip != null)
            {
                ip.item = dropItem;
                ip.amount = 1;
                ip.SetPickupDelay(pickupDelay);
            }

            // Setup physics
            Rigidbody2D rb = drop.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.simulated = true;
                rb.gravityScale = 0f; // Float initially
                rb.drag = 3f; // Less drag = flies farther
                rb.angularDrag = 3f;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                // STRONG burst direction with UPWARD BIAS!
                float burstAngle = (360f / count) * i + Random.Range(-30f, 30f);
                Vector2 dir = new Vector2(
                    Mathf.Cos(burstAngle * Mathf.Deg2Rad),
                    Mathf.Sin(burstAngle * Mathf.Deg2Rad)
                );
                
                // Force upward component to be strong
                dir.y = Mathf.Abs(dir.y) + upwardBias; // Extra up!
                dir = dir.normalized;
                
                // Apply STRONG force
                Vector2 force = dir * burstForce;
                rb.AddForce(force, ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-burstTorque, burstTorque), ForceMode2D.Impulse);
                
                Debug.Log($"🌲 BURST! Force: {force}, Velocity: {rb.velocity}");
            }



            
            // Gravity delay
            StartCoroutine(EnableGravityAfterDelay(drop, gravityDelay));
            
            Destroy(drop, despawnTime);
        }
        
        Debug.Log($"✓ Spawned {count} items with STRONG burst force {burstForce}!");
    }
    
    IEnumerator EnableGravityAfterDelay(GameObject dropObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (dropObject != null)
        {
            Rigidbody2D rb = dropObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 1f;
                Debug.Log($"✓ Gravity enabled - item should fall now");
            }
        }
    }
}