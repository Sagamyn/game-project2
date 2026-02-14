using UnityEngine;

/// <summary>
/// Manages weather effects (rain, clear, etc.)
/// Integrates with farming system for automatic watering
/// </summary>
public class WeatherManager : MonoBehaviour
{
    [Header("Weather State")]
    public WeatherType currentWeather = WeatherType.Clear;
    
    [Header("Rain System")]
    [Tooltip("Your rain particle system prefab or object in scene")]
    public ParticleSystem rainParticles;
    
    [Tooltip("Should rain start automatically?")]
    public bool autoStartRain = false;
    
    [Header("Rain Audio")]
    public AudioClip rainSound;
    public AudioSource rainAudioSource;
    [Range(0f, 1f)]
    public float rainVolume = 0.5f;
    
    [Header("Weather Duration")]
    [Tooltip("Weather lasts entire day")]
    public bool weatherChangesDaily = true;
    
    [Tooltip("If false, use time-based weather (old system)")]
    public bool useTimeBased = false;
    
    [Header("Time-Based Settings (if useTimeBased = true)")]
    [Tooltip("How long rain lasts (in seconds)")]
    public float minRainDuration = 30f;
    public float maxRainDuration = 120f;
    
    [Tooltip("Time between weather changes (in seconds)")]
    public float minClearDuration = 60f;
    public float maxClearDuration = 300f;
    
    [Header("Automatic Weather")]
    [Tooltip("Weather changes randomly")]
    public bool enableRandomWeather = true;
    
    [Tooltip("Chance of rain each new day (0-1)")]
    [Range(0f, 1f)]
    public float rainChance = 0.3f; // 30% chance
    
    [Header("Farming Integration")]
    [Tooltip("Rain automatically waters crops")]
    public bool rainWatersCrops = true;
    
    [Tooltip("How often rain waters crops (seconds)")]
    public float wateringInterval = 2f;
    
    [Header("Indoor/Outdoor")]
    [Tooltip("Hide rain particles when player is indoors")]
    public bool hideRainIndoors = true;
    
    [Tooltip("Keep rain sound when indoors")]
    public bool keepSoundIndoors = true;
    
    [Tooltip("Reduce sound volume indoors")]
    [Range(0f, 1f)]
    public float indoorSoundVolume = 0.3f;
    
    [Header("Visual Effects")]
    [Tooltip("Darken screen during rain")]
    public bool darkenDuringRain = true;
    
    [Range(0f, 0.5f)]
    public float rainDarkenAmount = 0.2f;
    
    public Camera mainCamera;
    private Color normalCameraColor;
    
    // Events (using full namespace since we removed 'using System')
    public event System.Action<WeatherType> OnWeatherChanged;
    
    private float weatherTimer = 0f;
    private float nextWeatherChangeTime = 0f;
    private float cropWateringTimer = 0f;
    private bool playerIsIndoors = false;
    private int currentDay = 0;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (mainCamera != null)
        {
            normalCameraColor = mainCamera.backgroundColor;
        }
        
        // Setup rain audio
        if (rainSound != null && rainAudioSource == null)
        {
            rainAudioSource = gameObject.AddComponent<AudioSource>();
            rainAudioSource.clip = rainSound;
            rainAudioSource.loop = true;
            rainAudioSource.volume = rainVolume;
            rainAudioSource.playOnAwake = false;
        }
        
        // Register with day/night manager for day-based weather
        DayNightManager dayManager = FindObjectOfType<DayNightManager>();
        if (dayManager != null && weatherChangesDaily)
        {
            dayManager.OnNewDay += OnNewDay;
            currentDay = dayManager.currentDay;
            Debug.Log("WeatherManager registered for daily weather changes");
            
            // Set initial weather for current day
            if (enableRandomWeather)
            {
                DecideWeatherForDay();
            }
        }
        
        // Initial weather setup
        if (autoStartRain)
        {
            StartRain();
        }
        else
        {
            StopRain();
        }
        
        // Time-based weather (old system)
        if (enableRandomWeather && useTimeBased && !weatherChangesDaily)
        {
            ScheduleNextWeatherChange();
        }
    }

    void Update()
    {
        // Time-based random weather (old system)
        if (enableRandomWeather && useTimeBased && !weatherChangesDaily)
        {
            weatherTimer += Time.deltaTime;
            
            if (weatherTimer >= nextWeatherChangeTime)
            {
                ChangeWeatherRandomly();
                ScheduleNextWeatherChange();
                weatherTimer = 0f;
            }
        }
        
        // Water crops during rain
        if (currentWeather == WeatherType.Rain && rainWatersCrops)
        {
            cropWateringTimer += Time.deltaTime;
            
            if (cropWateringTimer >= wateringInterval)
            {
                WaterAllCropsInRain();
                cropWateringTimer = 0f;
            }
        }
    }

    void ScheduleNextWeatherChange()
    {
        if (currentWeather == WeatherType.Rain)
        {
            nextWeatherChangeTime = UnityEngine.Random.Range(minRainDuration, maxRainDuration);
        }
        else
        {
            nextWeatherChangeTime = UnityEngine.Random.Range(minClearDuration, maxClearDuration);
        }
        
        Debug.Log($"Next weather change in {nextWeatherChangeTime:F0} seconds");
    }

    // Called when new day starts (day-based weather)
    void OnNewDay(int day)
    {
        // Prevent processing same day twice
        if (currentDay == day)
        {
            Debug.Log($"Weather: Day {day} already processed, skipping");
            return;
        }
        
        currentDay = day;
        
        if (weatherChangesDaily && enableRandomWeather)
        {
            DecideWeatherForDay();
        }
    }

    // Decide weather for the new day
    void DecideWeatherForDay()
    {
        float roll = UnityEngine.Random.value;
        
        if (roll <= rainChance)
        {
            SetWeather(WeatherType.Rain);
            Debug.Log($"🌧️ Day {currentDay}: Rain forecasted!");
        }
        else
        {
            SetWeather(WeatherType.Clear);
            Debug.Log($"☀️ Day {currentDay}: Clear weather!");
        }
    }

    void ChangeWeatherRandomly()
    {
        if (currentWeather == WeatherType.Rain)
        {
            // Rain is ending
            SetWeather(WeatherType.Clear);
        }
        else
        {
            // Check if it should start raining
            if (Random.value <= rainChance)
            {
                SetWeather(WeatherType.Rain);
            }
            else
            {
                SetWeather(WeatherType.Clear);
            }
        }
    }

    public void SetWeather(WeatherType newWeather)
    {
        if (currentWeather == newWeather) return;
        
        currentWeather = newWeather;
        
        switch (newWeather)
        {
            case WeatherType.Clear:
                StopRain();
                break;
                
            case WeatherType.Rain:
                StartRain();
                break;
        }
        
        OnWeatherChanged?.Invoke(newWeather);
        Debug.Log($"Weather changed to: {newWeather}");
    }

    void StartRain()
    {
        // Enable rain particles (unless player is indoors)
        if (rainParticles != null)
        {
            rainParticles.gameObject.SetActive(true);
            
            if (!rainParticles.isPlaying)
            {
                rainParticles.Play();
            }
            
            // Hide if player is indoors
            if (playerIsIndoors && hideRainIndoors)
            {
                rainParticles.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("Rain particle system not assigned!");
        }
        
        // Play rain sound
        if (rainAudioSource != null && rainSound != null)
        {
            rainAudioSource.Play();
            
            // Adjust volume based on indoor/outdoor
            if (playerIsIndoors && keepSoundIndoors)
            {
                rainAudioSource.volume = rainVolume * indoorSoundVolume;
            }
            else
            {
                rainAudioSource.volume = rainVolume;
            }
        }
        
        // Darken camera
        if (darkenDuringRain && mainCamera != null)
        {
            Color darkerColor = normalCameraColor;
            darkerColor.r -= rainDarkenAmount;
            darkerColor.g -= rainDarkenAmount;
            darkerColor.b -= rainDarkenAmount;
            mainCamera.backgroundColor = darkerColor;
        }
        
        Debug.Log("🌧️ Rain started!");
    }

    void StopRain()
    {
        // Disable rain particles
        if (rainParticles != null)
        {
            rainParticles.Stop();
            // Optional: Keep it active but stopped, or deactivate completely
            // rainParticles.gameObject.SetActive(false);
        }
        
        // Stop rain sound
        if (rainAudioSource != null)
        {
            rainAudioSource.Stop();
        }
        
        // Restore camera color
        if (darkenDuringRain && mainCamera != null)
        {
            mainCamera.backgroundColor = normalCameraColor;
        }
        
        Debug.Log("☀️ Rain stopped!");
    }

    void WaterAllCropsInRain()
    {
        // Find all PlayerFarming instances (if you have multiple zones)
        PlayerFarming[] farmingSystems = FindObjectsOfType<PlayerFarming>();
        
        int cropsWatered = 0;
        
        foreach (PlayerFarming farming in farmingSystems)
        {
            // Water all crops in this farming system
            cropsWatered += farming.WaterAllCropsInRain();
        }
        
        if (cropsWatered > 0)
        {
            Debug.Log($"☔ Rain watered {cropsWatered} crops");
        }
    }

    // Manual controls
    [ContextMenu("Start Rain")]
    public void ManualStartRain()
    {
        SetWeather(WeatherType.Rain);
    }

    [ContextMenu("Stop Rain")]
    public void ManualStopRain()
    {
        SetWeather(WeatherType.Clear);
    }

    [ContextMenu("Toggle Rain")]
    public void ToggleRain()
    {
        if (currentWeather == WeatherType.Rain)
        {
            SetWeather(WeatherType.Clear);
        }
        else
        {
            SetWeather(WeatherType.Rain);
        }
    }

    // Public getters
    public bool IsRaining()
    {
        return currentWeather == WeatherType.Rain;
    }

    public string GetWeatherString()
    {
        return currentWeather switch
        {
            WeatherType.Clear => "☀️ Clear",
            WeatherType.Rain => "🌧️ Raining",
            _ => "?"
        };
    }

    // Called when player enters building
    public void PlayerEnteredIndoors()
    {
        playerIsIndoors = true;
        
        if (currentWeather == WeatherType.Rain)
        {
            // Hide rain particles
            if (hideRainIndoors && rainParticles != null)
            {
                rainParticles.gameObject.SetActive(false);
                Debug.Log("☔ Rain particles hidden (indoors)");
            }
            
            // Reduce rain sound
            if (keepSoundIndoors && rainAudioSource != null)
            {
                rainAudioSource.volume = rainVolume * indoorSoundVolume;
                Debug.Log($"🔊 Rain sound reduced to {indoorSoundVolume * 100}%");
            }
            else if (!keepSoundIndoors && rainAudioSource != null)
            {
                rainAudioSource.volume = 0f;
            }
        }
    }

    // Called when player exits building
    public void PlayerExitedIndoors()
    {
        playerIsIndoors = false;
        
        if (currentWeather == WeatherType.Rain)
        {
            // Show rain particles
            if (hideRainIndoors && rainParticles != null)
            {
                rainParticles.gameObject.SetActive(true);
                if (!rainParticles.isPlaying)
                {
                    rainParticles.Play();
                }
                Debug.Log("🌧️ Rain particles shown (outdoors)");
            }
            
            // Restore rain sound
            if (rainAudioSource != null)
            {
                rainAudioSource.volume = rainVolume;
                Debug.Log("🔊 Rain sound restored to full volume");
            }
        }
    }

    void OnGUI()
    {
        // Debug display
        if (!Application.isPlaying) return;
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperLeft;
        
        string weatherText = $"Weather: {GetWeatherString()}";
        
        if (enableRandomWeather)
        {
            float timeUntilChange = nextWeatherChangeTime - weatherTimer;
            weatherText += $"\nNext change: {timeUntilChange:F0}s";
        }
        
        GUI.Label(new Rect(10, 100, 300, 100), weatherText, style);
    }
}

public enum WeatherType
{
    Clear,
    Rain,
    // Future: Snow, Storm, Fog, etc.
}