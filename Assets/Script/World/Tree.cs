using UnityEngine;
using System.Collections;

/// <summary>
/// Stardew Valley style tree chopping effect:
/// 1. Shake when hit
/// 2. Drop leaves particles
/// 3. Tree top falls left/right when chopped
/// 4. Dust particles on ground
/// 5. Wood items burst out
/// </summary>
public class Tree : MonoBehaviour, IDamageable
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

    [Header("Tree Parts")]
    public Transform treeTop; // The upper part that falls (leaves/crown)
    public Transform treeStump; // The bottom part that stays
    public SpriteRenderer treeTopRenderer;
    
    [Header("Hit Effect")]
    public float shakeIntensity = 0.2f;
    public float shakeDuration = 0.2f;
    public int shakeCount = 3;
    
    [Header("Fall Effect")]
    public float fallDuration = 100f;
    public float fallRotation = 90f; // How much tree rotates when falling
    public AnimationCurve fallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Particles")]
    public ParticleSystem leavesParticles; // Leaves when hit
    public ParticleSystem dustParticles; // Dust when tree falls
    public int leavesPerHit = 10;
    
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isDying = false;

    void Awake()
    {
        hp = maxHP;
        
        if (treeTop != null)
        {
            originalPosition = treeTop.localPosition;
            originalRotation = treeTop.localRotation;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDying) return;
        
        hp -= amount;
        Debug.Log($"🌲 Tree HP: {hp}/{maxHP}");

        // Shake and drop leaves on hit
        StartCoroutine(ShakeOnHit());
        SpawnLeaves(leavesPerHit);

        if (hp <= 0)
        {
            Die();
        }
    }

    IEnumerator ShakeOnHit()
    {
        if (treeTop == null) yield break;
        
        for (int i = 0; i < shakeCount; i++)
        {
            float elapsed = 0f;
            float halfDuration = shakeDuration / 2f;
            
            // Shake right
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float offset = Mathf.Lerp(0f, shakeIntensity, elapsed / halfDuration);
                treeTop.localPosition = originalPosition + Vector3.right * offset;
                yield return null;
            }
            
            elapsed = 0f;
            
            // Shake left
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float offset = Mathf.Lerp(shakeIntensity, -shakeIntensity, elapsed / halfDuration);
                treeTop.localPosition = originalPosition + Vector3.right * offset;
                yield return null;
            }
        }
        
        // Return to center
        treeTop.localPosition = originalPosition;
    }

    void SpawnLeaves(int count)
    {
        if (leavesParticles != null && treeTop != null)
        {
            leavesParticles.transform.position = treeTop.position;
            leavesParticles.Emit(count);
            Debug.Log($"🍃 Spawned {count} leaves");
        }
    }

    void Die()
    {
        if (isDying) return;
        isDying = true;
        
        Debug.Log("🌲 TREE DYING - Starting fall sequence");
        AudioManager.Instance.PlaySFX(AudioManager.Instance.axeBreakSound);
        
        // Start the full death sequence
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // Step 1: Big leaf burst
        SpawnLeaves(leavesPerHit * 3);
        
        // Step 2: Tree top falls
        if (treeTop != null)
        {
            yield return StartCoroutine(TreeFallAnimation());
        }
        
        // Step 3: Dust on ground
        if (dustParticles != null)
        {
            dustParticles.transform.position = transform.position;
            dustParticles.Play();
        }
        
        // Step 4: Wood burst
        yield return new WaitForSeconds(0.2f); // Small delay for effect
        SpawnWoodBurst();
        
        // Step 5: Cleanup
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }

    IEnumerator TreeFallAnimation()
    {
        // Randomly fall left or right
        float direction = Random.value > 0.5f ? 1f : -1f;
        
        Vector3 startPos = treeTop.localPosition;
        Quaternion startRot = treeTop.localRotation;
        
        // Target position (falls to the side and down)
        Vector3 targetPos = startPos + new Vector3(direction * 1.5f, -0.5f, 0f);
        Quaternion targetRot = Quaternion.Euler(0f, 0f, fallRotation * direction);
        
        // Fade out
        Color startColor = treeTopRenderer != null ? treeTopRenderer.color : Color.white;
        
        float elapsed = 0f;
        
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            float curveT = fallCurve.Evaluate(t);
            
            // Position and rotation
            treeTop.localPosition = Vector3.Lerp(startPos, targetPos, curveT);
            treeTop.localRotation = Quaternion.Lerp(startRot, targetRot, curveT);
            
            // Fade out
            if (treeTopRenderer != null)
            {
                Color fadeColor = startColor;
                fadeColor.a = 1f - (t * 0.7f); // Fade to 30% opacity
                treeTopRenderer.color = fadeColor;
            }
            
            yield return null;
        }
        
        Debug.Log($"✓ Tree fell {(direction > 0 ? "right" : "left")}");
    }

    void SpawnWoodBurst()
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
                rb.drag = 2f;
                rb.angularDrag = 2f;
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
        
        Debug.Log($"🌲 Spawned {count} wood items");
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