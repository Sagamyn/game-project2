using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Controls 2D lighting for day/night cycle
/// Works with Unity's 2D Lighting system
/// </summary>
public class LightingManager : MonoBehaviour
{
    [Header("Global Light (2D Light)")]
    [Tooltip("The main 2D Global Light in your scene")]
    public Light2D globalLight;
    
    [Header("Day/Night Colors")]
    public Color dayLightColor = new Color(1f, 0.95f, 0.8f); // Warm daylight
    public Color nightLightColor = new Color(0.2f, 0.2f, 0.4f); // Cool night
    
    [Header("Day/Night Intensity")]
    [Range(0f, 2f)]
    public float dayIntensity = 1f; // Full brightness
    
    [Range(0f, 2f)]
    public float nightIntensity = 0.3f; // Dim at night
    
    [Header("Transition")]
    public bool smoothTransition = true;
    public float transitionSpeed = 1f;
    
    [Header("Optional: Camera Background")]
    public bool changeCameraBackground = true;
    public Camera mainCamera;
    public Color daySkyColor = new Color(0.53f, 0.81f, 0.92f); // Light blue
    public Color nightSkyColor = new Color(0.1f, 0.1f, 0.2f); // Dark blue
    
    private DayNightManager dayNightManager;
    
    void Start()
    {
        // Find Day/Night Manager
        dayNightManager = FindObjectOfType<DayNightManager>();
        
        if (dayNightManager == null)
        {
            Debug.LogWarning("No DayNightManager found! LightingManager needs it.");
        }
        
        // Find Global Light if not assigned
        if (globalLight == null)
        {
            globalLight = FindObjectOfType<Light2D>();
            if (globalLight != null)
            {
                Debug.Log("✓ Auto-found Global Light: " + globalLight.name);
            }
        }
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // Initial setup
        UpdateLighting();
    }
    
    void Update()
    {
        UpdateLighting();
    }
    
    void UpdateLighting()
    {
        if (dayNightManager == null) return;
        
        // Get blend factor (0 = day, 1 = night)
        float t = GetDayNightBlend();
        
        // Update Global Light
        if (globalLight != null)
        {
            if (smoothTransition)
            {
                globalLight.color = Color.Lerp(
                    globalLight.color,
                    Color.Lerp(dayLightColor, nightLightColor, t),
                    Time.deltaTime * transitionSpeed
                );
                
                globalLight.intensity = Mathf.Lerp(
                    globalLight.intensity,
                    Mathf.Lerp(dayIntensity, nightIntensity, t),
                    Time.deltaTime * transitionSpeed
                );
            }
            else
            {
                globalLight.color = Color.Lerp(dayLightColor, nightLightColor, t);
                globalLight.intensity = Mathf.Lerp(dayIntensity, nightIntensity, t);
            }
        }
        
        // Update Camera Background
        if (changeCameraBackground && mainCamera != null)
        {
            if (smoothTransition)
            {
                mainCamera.backgroundColor = Color.Lerp(
                    mainCamera.backgroundColor,
                    Color.Lerp(daySkyColor, nightSkyColor, t),
                    Time.deltaTime * transitionSpeed
                );
            }
            else
            {
                mainCamera.backgroundColor = Color.Lerp(daySkyColor, nightSkyColor, t);
            }
        }
    }
    
    float GetDayNightBlend()
    {
        if (dayNightManager == null) return 0f;
        
        float currentTime = dayNightManager.currentTime;
        
        // Day: 6 AM to 6 PM (6-18) → Blend = 0
        // Night: 6 PM to 6 AM (18-24, 0-6) → Blend = 1
        
        if (currentTime >= 6f && currentTime <= 18f)
        {
            // Daytime - transition from morning to evening
            if (currentTime >= 6f && currentTime <= 8f)
            {
                // Sunrise (6:00 to 8:00)
                return 1f - ((currentTime - 6f) / 2f);
            }
            else if (currentTime >= 16f && currentTime <= 18f)
            {
                // Sunset starting (16:00 to 18:00)
                return (currentTime - 16f) / 2f;
            }
            else
            {
                // Full day
                return 0f;
            }
        }
        else if (currentTime > 18f && currentTime <= 21f)
        {
            // Evening transition (18:00 to 21:00)
            return 0.5f + ((currentTime - 18f) / 6f);
        }
        else if (currentTime > 21f || currentTime < 3f)
        {
            // Full night (21:00 to 03:00)
            return 1f;
        }
        else
        {
            // Morning transition (03:00 to 06:00)
            return 1f - ((currentTime - 3f) / 3f);
        }
    }
    
    [ContextMenu("Set to Day")]
    public void SetToDay()
    {
        if (globalLight != null)
        {
            globalLight.color = dayLightColor;
            globalLight.intensity = dayIntensity;
        }
        if (changeCameraBackground && mainCamera != null)
        {
            mainCamera.backgroundColor = daySkyColor;
        }
    }
    
    [ContextMenu("Set to Night")]
    public void SetToNight()
    {
        if (globalLight != null)
        {
            globalLight.color = nightLightColor;
            globalLight.intensity = nightIntensity;
        }
        if (changeCameraBackground && mainCamera != null)
        {
            mainCamera.backgroundColor = nightSkyColor;
        }
    }
}