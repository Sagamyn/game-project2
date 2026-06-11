using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    public TextMeshProUGUI waveText;
    private DayNightManager dayNightManager;

    void Start()
    {
        dayNightManager = FindObjectOfType<DayNightManager>();
        
        if (dayNightManager != null)
        {
            dayNightManager.OnNewDay += UpdateWaveText;
            UpdateWaveText(dayNightManager.currentDay);
        }
    }

    void OnDestroy()
    {
        if (dayNightManager != null)
        {
            dayNightManager.OnNewDay -= UpdateWaveText;
        }
    }

    private void UpdateWaveText(int currentDay)
    {
        if (waveText != null)
        {
            if (currentDay == 0)
            {
                waveText.text = "WAVE - 0 (TUTORIAL)";
            }
            else
            {
                waveText.text = $"WAVE - {currentDay}";
            }
        }
    }
}
