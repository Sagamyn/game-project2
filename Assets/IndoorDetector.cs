using UnityEngine;

/// <summary>
/// Detects when player enters/exits indoor areas
/// Automatically notifies WeatherManager to adjust rain effects
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class IndoorDetector : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Is this an indoor area?")]
    public bool isIndoor = true;
    
    [Header("Visual Feedback")]
    [Tooltip("Show debug messages")]
    public bool showDebug = true;
    
    [Header("Optional: Custom Effects")]
    [Tooltip("Disable specific effects when entering this area")]
    public bool disableRainParticles = true;
    public bool muteRainSound = false;
    
    private WeatherManager weatherManager;

    void Start()
    {
        // Make sure this is a trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        
        // Find weather manager
        weatherManager = FindObjectOfType<WeatherManager>();
        if (weatherManager == null && showDebug)
        {
            Debug.LogWarning("WeatherManager not found! Indoor detection won't affect weather.");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (isIndoor)
            {
                PlayerEnteredIndoors();
            }
            else
            {
                PlayerExitedIndoors();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (isIndoor)
            {
                PlayerExitedIndoors();
            }
            else
            {
                PlayerEnteredIndoors();
            }
        }
    }

    void PlayerEnteredIndoors()
    {
        if (showDebug)
        {
            Debug.Log($"🏠 Player entered indoors: {gameObject.name}");
        }
        
        if (weatherManager != null)
        {
            weatherManager.PlayerEnteredIndoors();
        }
    }

    void PlayerExitedIndoors()
    {
        if (showDebug)
        {
            Debug.Log($"🌳 Player exited to outdoors: {gameObject.name}");
        }
        
        if (weatherManager != null)
        {
            weatherManager.PlayerExitedIndoors();
        }
    }

    void OnDrawGizmos()
    {
        // Draw visualization of the indoor area
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = isIndoor ? new Color(1f, 0.5f, 0f, 0.3f) : new Color(0f, 1f, 0f, 0.3f);
            
            if (col is BoxCollider2D boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCol.offset, boxCol.size);
            }
            else if (col is CircleCollider2D circleCol)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)circleCol.offset, circleCol.radius);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw label
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Vector3 labelPos = transform.position;
            if (col is BoxCollider2D boxCol)
            {
                labelPos += (Vector3)boxCol.offset;
            }
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(labelPos, isIndoor ? "🏠 INDOOR" : "🌳 OUTDOOR");
            #endif
        }
    }
}