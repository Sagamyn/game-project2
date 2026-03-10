using UnityEngine;
using System.Collections;

public class Rock : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHP = 3;
    int hp;

    [Header("Drop")]
    public ResourceItem dropItem;
    public GameObject dropPrefab;
    public int minDrop = 1;
    public int maxDrop = 3;

    [Header("Burst Settings")]
    public float burstForce = 25f;
    public float minUpwardForce = 5f;
    public float burstTorque = 30f;
    public float despawnTime = 10f;

    [Header("Physics")]
    public float gravityDelay = 0.8f;
    public float pickupDelay = 0.5f;

    void Awake()
    {
        hp = maxHP;
    }

    public void TakeDamage(int amount)
    {
        hp -= amount;

        if (hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.rockBreakSound);
        SpawnBurst();
        Destroy(gameObject);
    }

    void SpawnBurst()
    {
        if (dropPrefab == null)
        {
            Debug.LogError("❌ DROP PREFAB IS NULL!");
            return;
        }

        int count = Random.Range(minDrop, maxDrop + 1);
        Vector3 spawnCenter = transform.position + Vector3.up * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i + Random.Range(-30f, 30f);
            Vector2 spawnOffset = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ) * 0.5f;

            Vector3 spawnPos = spawnCenter + new Vector3(spawnOffset.x, spawnOffset.y, 0f);
            GameObject drop = Instantiate(dropPrefab, spawnPos, Quaternion.identity);

            ItemPickup ip = drop.GetComponent<ItemPickup>();
            if (ip != null)
            {
                ip.item = dropItem;
                ip.amount = 1;
                ip.SetPickupDelay(pickupDelay);
            }

            Rigidbody2D rb = drop.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.simulated = true;
                rb.gravityScale = 0f;
                rb.drag = 3f;
                rb.angularDrag = 8f;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;

                Collider2D col = drop.GetComponent<Collider2D>();
                if (col != null)
                {
                    col.enabled = false;
                    StartCoroutine(EnableColliderAfterDelay(col, 0.2f));
                }

                float burstAngle = (360f / count) * i + Random.Range(-30f, 30f);
                Vector2 burstDirection = new Vector2(
                    Mathf.Cos(burstAngle * Mathf.Deg2Rad),
                    Mathf.Sin(burstAngle * Mathf.Deg2Rad)
                );

                if (burstDirection.y < 0.3f)
                    burstDirection.y = 0.3f + Random.Range(0f, 0.5f);

                burstDirection = burstDirection.normalized;

                Vector2 finalForce = burstDirection * burstForce;
                finalForce.y += minUpwardForce;

                rb.AddForce(finalForce, ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-burstTorque, burstTorque), ForceMode2D.Impulse);
            }

            SpriteRenderer sr = drop.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sortingOrder = 1000;

            StartCoroutine(EnableGravityAfterDelay(drop, gravityDelay));
            Destroy(drop, despawnTime);
        }

        Debug.Log($"🪨 Spawned {count} rock items");
    }

    IEnumerator EnableColliderAfterDelay(Collider2D col, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (col != null)
            col.enabled = true;
    }

    IEnumerator EnableGravityAfterDelay(GameObject dropObject, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (dropObject != null)
        {
            Rigidbody2D rb = dropObject.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.gravityScale = 1.5f;
        }
    }
}