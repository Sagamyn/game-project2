using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // The player

    [Header("Follow Settings")]
    public float smoothSpeed = 0.125f; // How smooth the camera follows (lower = smoother)
    public Vector3 offset = new Vector3(0, 0, -10); // Camera offset from player

    [Header("Boundary Limits")]
    public bool useBoundaries = true;
    public float minX = -10f; // Left boundary
    public float maxX = 10f;  // Right boundary
    public float minY = -10f; // Bottom boundary
    public float maxY = 10f;  // Top boundary

    [Header("Visual Debugging")]
    public bool showBoundaries = true;
    public Color boundaryColor = Color.red;

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollow: Target is not assigned!");
            return;
        }

        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;

        // Clamp position within boundaries
        if (useBoundaries)
        {
            // Get camera's orthographic size (half-height of what camera sees)
            Camera cam = GetComponent<Camera>();
            float cameraHalfHeight = cam.orthographicSize;
            float cameraHalfWidth = cameraHalfHeight * cam.aspect;

            // Clamp so camera doesn't show outside map boundaries
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX + cameraHalfWidth, maxX - cameraHalfWidth);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY + cameraHalfHeight, maxY - cameraHalfHeight);
        }

        // Smoothly move camera to desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Keep the Z offset (important for 2D)
        smoothedPosition.z = offset.z;

        transform.position = smoothedPosition;
    }

    // Draw boundaries in Scene view
    void OnDrawGizmos()
    {
        if (!showBoundaries || !useBoundaries) return;

        Gizmos.color = boundaryColor;

        // Draw boundary rectangle
        Vector3 topLeft = new Vector3(minX, maxY, 0);
        Vector3 topRight = new Vector3(maxX, maxY, 0);
        Vector3 bottomLeft = new Vector3(minX, minY, 0);
        Vector3 bottomRight = new Vector3(maxX, minY, 0);

        Gizmos.DrawLine(topLeft, topRight);     // Top
        Gizmos.DrawLine(topRight, bottomRight); // Right
        Gizmos.DrawLine(bottomRight, bottomLeft); // Bottom
        Gizmos.DrawLine(bottomLeft, topLeft);   // Left

        // Draw corner markers
        Gizmos.DrawWireSphere(topLeft, 0.5f);
        Gizmos.DrawWireSphere(topRight, 0.5f);
        Gizmos.DrawWireSphere(bottomLeft, 0.5f);
        Gizmos.DrawWireSphere(bottomRight, 0.5f);
    }

    // Helper function to set boundaries from code
    public void SetBoundaries(float minX, float maxX, float minY, float maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
        
        Debug.Log($"Camera boundaries set to: ({minX}, {minY}) to ({maxX}, {maxY})");
    }

    // Smooth transition to new boundaries (for camera zones)
    public void SetBoundariesSmooth(float newMinX, float newMaxX, float newMinY, float newMaxY, float speed)
    {
        StartCoroutine(SmoothBoundaryTransition(newMinX, newMaxX, newMinY, newMaxY, speed));
    }

    System.Collections.IEnumerator SmoothBoundaryTransition(float newMinX, float newMaxX, float newMinY, float newMaxY, float speed)
    {
        float startMinX = minX, startMaxX = maxX, startMinY = minY, startMaxY = maxY;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            
            minX = Mathf.Lerp(startMinX, newMinX, t);
            maxX = Mathf.Lerp(startMaxX, newMaxX, t);
            minY = Mathf.Lerp(startMinY, newMinY, t);
            maxY = Mathf.Lerp(startMaxY, newMaxY, t);
            
            yield return null;
        }

        // Set final values
        minX = newMinX;
        maxX = newMaxX;
        minY = newMinY;
        maxY = newMaxY;
        
        Debug.Log($"Camera boundaries smoothly transitioned to: ({minX}, {minY}) to ({maxX}, {maxY})");
    }

    // Helper to set boundaries based on a BoxCollider2D (useful for map boundaries)
    public void SetBoundariesFromCollider(BoxCollider2D mapBounds)
    {
        Bounds bounds = mapBounds.bounds;
        minX = bounds.min.x;
        maxX = bounds.max.x;
        minY = bounds.min.y;
        maxY = bounds.max.y;
    }
}