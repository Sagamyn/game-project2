using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthSlider;
    public Image fillImage;
    public TextMeshProUGUI healthText;

    [Header("Colors")]
    public Color fullColor = Color.green;
    public Color midColor = Color.yellow;
    public Color lowColor = Color.red;

    void Start()
    {
        if (PlayerHealth.Instance == null) return;

        PlayerHealth.Instance.OnHealthChanged += UpdateUI;
        PlayerHealth.Instance.OnPlayerDied += OnGameOver;

        UpdateUI(PlayerHealth.Instance.currentHealth, 
                 PlayerHealth.Instance.maxHealth);
    }

    void OnDestroy()
    {
        if (PlayerHealth.Instance == null) return;
        PlayerHealth.Instance.OnHealthChanged -= UpdateUI;
        PlayerHealth.Instance.OnPlayerDied -= OnGameOver;
    }

    void UpdateUI(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        if (healthText != null)
            healthText.text = $"{current} / {max}";

        if (fillImage != null)
        {
            float pct = (float)current / max;
            fillImage.color = pct > 0.6f ? fullColor :
                              pct > 0.3f ? midColor : lowColor;
        }
    }

    void OnGameOver()
    {
        Debug.Log("Game Over!");
        // Hook to your GameManager later
    }
}