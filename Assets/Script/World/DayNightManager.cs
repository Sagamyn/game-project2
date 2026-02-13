using UnityEngine;
using UnityEngine.Events;
using System;

/// <summary>
/// Manages day/night cycle
/// Resources regenerate when new day starts
/// </summary>
public class DayNightManager : MonoBehaviour
{
    [Header("Day Settings")]
    [Tooltip("Current day number")]
    public int currentDay = 1;
    
    [Tooltip("How long is one full day (in real seconds)")]
    public float dayLengthInSeconds = 120f; // 2 minutes = 1 day
    
    [Header("Time of Day")]
    [Tooltip("Current time (0-24 hours)")]
    [Range(0f, 24f)]
    public float currentTime = 6f; // Start at 6 AM
    
    [Header("Auto Advance")]
    [Tooltip("Should time automatically advance?")]
    public bool autoAdvanceTime = true;
    
    [Header("Day/Night Visual")]
    [Tooltip("Change background color for day/night")]
    public bool changeLighting = true;
    public Color dayColor = new Color(1f, 1f, 1f, 1f);
    public Color nightColor = new Color(0.3f, 0.3f, 0.5f, 1f);
    public Camera mainCamera;
    
    [Header("Events")]
    public UnityEvent onDayStart;
    public UnityEvent onNightStart;
    
    [Header("Sleep Settings")]
    [Tooltip("Force player to sleep at this time")]
    public bool requireSleep = true;
    public float sleepTime = 23f; // 11 PM
    
    [Tooltip("Freeze time when sleep is required")]
    public bool freezeTimeAtSleepHour = true;
    
    private bool waitingForSleep = false;
    private bool timeIsFrozen = false;
    
    // Events
    public event System.Action<int> OnNewDay;
    public event System.Action OnSleepRequired; // New event for sleep requirement
    
    private int lastDay = 0;
    private bool wasNight = false;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        lastDay = currentDay;
        UpdateLighting();
    }

    void Update()
    {
        if (autoAdvanceTime)
        {
            AdvanceTime(Time.deltaTime);
        }
        
        if (changeLighting)
        {
            UpdateLighting();
        }
        
        CheckForNewDay();
        CheckForDayNightTransition();
    }

    void AdvanceTime(float deltaTime)
    {
        // Check if time should freeze
        if (freezeTimeAtSleepHour && requireSleep && currentTime >= sleepTime && !timeIsFrozen)
        {
            currentTime = sleepTime; // Lock at sleep time
            timeIsFrozen = true;
            waitingForSleep = true;
            
            Debug.Log($"⏰ Time frozen at {sleepTime}:00 - Player must sleep!");
            OnSleepRequired?.Invoke();
            return;
        }
        
        // Don't advance time if frozen
        if (timeIsFrozen) return;
        
        // Convert real seconds to game hours
        float hoursPerSecond = 24f / dayLengthInSeconds;
        currentTime += hoursPerSecond * deltaTime;
        
        // Wrap to next day at midnight (BUT don't increment day yet - sleep does that)
        if (currentTime >= 24f)
        {
            currentTime = 0f;
            // Don't increment day here if sleep is required
            if (!requireSleep)
            {
                currentDay++;
            }
        }
    }

    void CheckForNewDay()
    {
        // Only trigger event when day actually changes
        // Don't increment here - that's done in sleep or time wrap
        if (currentDay > lastDay)
        {
            Debug.Log($"=== NEW DAY: {currentDay} ===");
            
            // Trigger event for resource regeneration
            OnNewDay?.Invoke(currentDay);
            
            // Unity event
            onDayStart?.Invoke();
            
            lastDay = currentDay;
        }
    }

    void CheckForDayNightTransition()
    {
        bool isNight = IsNightTime();
        
        if (isNight && !wasNight)
        {
            // Just became night
            Debug.Log("Night time started");
            onNightStart?.Invoke();
        }
        
        wasNight = isNight;
    }

    void UpdateLighting()
    {
        if (mainCamera == null) return;
        
        // Interpolate between day and night colors based on time
        float t = GetDayNightBlend();
        mainCamera.backgroundColor = Color.Lerp(dayColor, nightColor, t);
    }

    float GetDayNightBlend()
    {
        // Day: 6 AM to 6 PM (6-18)
        // Night: 6 PM to 6 AM (18-24, 0-6)
        
        if (currentTime >= 6f && currentTime <= 18f)
        {
            // Daytime - no night blend
            return 0f;
        }
        else if (currentTime > 18f && currentTime <= 21f)
        {
            // Evening transition (18:00 to 21:00)
            return (currentTime - 18f) / 3f;
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

    bool IsNightTime()
    {
        return currentTime >= 20f || currentTime < 6f;
    }

    // Manual controls
    [ContextMenu("Advance to Next Day")]
    public void AdvanceToNextDay()
    {
        currentDay++;
        currentTime = 6f; // Set to 6 AM
        
        Debug.Log($"Advanced to Day {currentDay}");
        OnNewDay?.Invoke(currentDay);
        onDayStart?.Invoke();
    }

    [ContextMenu("Set to Morning")]
    public void SetToMorning()
    {
        currentTime = 6f;
    }

    [ContextMenu("Set to Noon")]
    public void SetToNoon()
    {
        currentTime = 12f;
    }

    [ContextMenu("Set to Night")]
    public void SetToNight()
    {
        currentTime = 20f;
    }

    // Called when player goes to sleep
    public void PlayerGoesToSleep()
    {
        Debug.Log("💤 Player is sleeping...");
        
        // Advance to next day
        currentDay++;
        currentTime = 6f; // Wake up at 6 AM
        
        // Unfreeze time
        timeIsFrozen = false;
        waitingForSleep = false;
        
        Debug.Log($"☀️ Good morning! It's Day {currentDay}");
        
        // Trigger new day event
        OnNewDay?.Invoke(currentDay);
        onDayStart?.Invoke();
    }

    // Public getters
    public bool IsDaytime()
    {
        return currentTime >= 6f && currentTime < 18f;
    }

    public string GetTimeString()
    {
        int hours = Mathf.FloorToInt(currentTime);
        int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);
        
        return $"{hours:00}:{minutes:00}";
    }

    public string GetDayString()
    {
        return $"Day {currentDay}";
    }

    void OnGUI()
    {
        // Debug display in top-right
        if (!Application.isPlaying) return;
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperRight;
        
        string timeText = $"{GetDayString()}\n{GetTimeString()}\n{(IsDaytime() ? "☀️ Day" : "🌙 Night")}";
        
        if (waitingForSleep)
        {
            timeText += "\n⏰ TIME TO SLEEP!";
        }
        
        GUI.Label(new Rect(Screen.width - 200, 10, 190, 120), timeText, style);
    }
}