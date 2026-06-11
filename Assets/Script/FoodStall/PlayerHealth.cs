using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;

    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Invincibility")]
    public float invincibilityDuration = 1f;
    private float invincibilityTimer;
    private bool isInvincible;

    [Header("Damage Flash")]
    public Image damageFlash;

    public float flashDuration = 0.15f;
    public float flashAlpha = 0.7f;
    public event Action<int, int> OnHealthChanged; // current, max
    public event Action OnPlayerDied;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
                isInvincible = false;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        StartCoroutine(FlashDamage());
        CameraShake.Instance?.TriggerShake(0.5f, 0.5f);
        
        // Show Floating Text Damage if available
        FloatingTextManager.Instance?.ShowText($"-{amount} HP", transform.position + Vector3.up * 1.5f, Color.red);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        Debug.Log($"Player took {amount} damage! HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetMaxHealth(int newMax, bool healToFull = false)
    {
        maxHealth = newMax;
        if (healToFull) currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        Debug.Log("Player died! Game Over.");
        OnPlayerDied?.Invoke();
        // GameManager will handle the actual game over
    }

    IEnumerator FlashDamage()
{
    if (damageFlash == null)
        yield break;

    Color c = damageFlash.color;

    c.a = flashAlpha;
    damageFlash.color = c;

    float timer = 0f;

    while (timer < flashDuration)
    {
        timer += Time.deltaTime;

        c.a = Mathf.Lerp(
            flashAlpha,
            0f,
            timer / flashDuration
        );

        damageFlash.color = c;

        yield return null;
    }

    c.a = 0f;
    damageFlash.color = c;
}
}