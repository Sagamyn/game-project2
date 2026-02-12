using UnityEngine;

/// <summary>
/// Defines camera boundary zones
/// Camera bounds change when player enters different zones
/// Perfect for multi-area maps with teleporters
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class CameraZone : MonoBehaviour
{
    [Header("Zone Boundaries")]
    [Tooltip("Auto-calculate from this collider")]
    public bool autoSetFromCollider = true;
    
    [Tooltip("Camera won't go past these boundaries")]
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -10f;
    public float maxY = 10f;

    [Header("Transition")]
    [Tooltip("Smooth transition to new bounds")]
    public bool smoothTransition = true;
    public float transitionSpeed = 5f;

    [Header("Debug")]
    public Color zoneColor = new Color(1f, 1f, 0f, 0.2f);
    public bool showGizmos = true;

    private BoxCollider2D zoneCollider;

    void Awake()
    {
        zoneCollider = GetComponent<BoxCollider2D>();
        zoneCollider.isTrigger = true;

        if (autoSetFromCollider)
        {
            CalculateBoundsFromCollider();
        }
    }

    void CalculateBoundsFromCollider()
    {
        Bounds bounds = zoneCollider.bounds;
        minX = bounds.min.x;
        maxX = bounds.max.x;
        minY = bounds.min.y;
        maxY = bounds.max.y;

        Debug.Log($"[CameraZone] '{gameObject.name}' bounds: ({minX}, {minY}) to ({maxX}, {maxY})");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player entered camera zone: {gameObject.name}");
            
            // Find camera controller and update bounds
            CameraFollow camFollow = FindObjectOfType<CameraFollow>();
            if (camFollow != null)
            {
                if (smoothTransition)
                {
                    camFollow.SetBoundariesSmooth(minX, maxX, minY, maxY, transitionSpeed);
                }
                else
                {
                    camFollow.SetBoundaries(minX, maxX, minY, maxY);
                }
            }
            else
            {
                Debug.LogWarning("No CameraFollow found in scene!");
            }
        }
    }

    [ContextMenu("Recalculate Bounds")]
    void EditorRecalculateBounds()
    {
        if (zoneCollider == null)
            zoneCollider = GetComponent<BoxCollider2D>();
        
        CalculateBoundsFromCollider();
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (zoneCollider == null)
            zoneCollider = GetComponent<BoxCollider2D>();

        if (zoneCollider != null)
        {
            // Draw zone area
            Gizmos.color = zoneColor;
            Gizmos.DrawCube(zoneCollider.bounds.center, zoneCollider.bounds.size);

            // Draw border
            Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 1f);
            DrawBorder(zoneCollider.bounds);
        }

        // Draw camera boundaries
        if (Application.isPlaying || autoSetFromCollider)
        {
            Gizmos.color = Color.yellow;
            DrawCameraBounds();
        }
    }

    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        // Draw label
        Vector3 center = zoneCollider != null ? zoneCollider.bounds.center : transform.position;
        
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.yellow;
        style.fontSize = 12;
        style.fontStyle = FontStyle.Bold;

        string label = $"Camera Zone: {gameObject.name}\n";
        label += $"Bounds: ({minX:F1}, {minY:F1}) to ({maxX:F1}, {maxY:F1})";
        
        UnityEditor.Handles.Label(center + Vector3.up * 2f, label, style);
#endif
    }

    void DrawBorder(Bounds bounds)
    {
        Vector3 topLeft = new Vector3(bounds.min.x, bounds.max.y, 0);
        Vector3 topRight = new Vector3(bounds.max.x, bounds.max.y, 0);
        Vector3 bottomLeft = new Vector3(bounds.min.x, bounds.min.y, 0);
        Vector3 bottomRight = new Vector3(bounds.max.x, bounds.min.y, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }

    void DrawCameraBounds()
    {
        Vector3 topLeft = new Vector3(minX, maxY, 0);
        Vector3 topRight = new Vector3(maxX, maxY, 0);
        Vector3 bottomLeft = new Vector3(minX, minY, 0);
        Vector3 bottomRight = new Vector3(maxX, minY, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}