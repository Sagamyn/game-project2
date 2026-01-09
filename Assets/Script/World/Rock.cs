using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class Rock : MonoBehaviour, IDamageable
{
    public int maxHP = 3;
    int hp;

    public ResourceItem dropItem;
    public GameObject dropPrefab;
    public int minDrop = 1;
    public int maxDrop = 3;

    public float burstForce = 3f;
    public float burstTorque = 6f;
    public float despawnTime = 10f;

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
            ip.item = dropItem;
            ip.amount = 1;
         Rigidbody2D rb = drop.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.drag = 5f;
                rb.angularDrag = 5f;

                Vector2 dir = Random.insideUnitCircle;
                dir.y = Mathf.Abs(dir.y);

                rb.AddForce(
                    dir.normalized * burstForce,
                    ForceMode2D.Impulse
                );

                rb.AddTorque(
                    Random.Range(-burstTorque, burstTorque),
                    ForceMode2D.Impulse
                );
            }

            Destroy(drop, despawnTime);
        }
    }
}
