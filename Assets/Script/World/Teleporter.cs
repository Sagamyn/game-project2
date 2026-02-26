using UnityEngine;
using System.Collections;


[RequireComponent(typeof(BoxCollider2D))]
public class Teleporter : MonoBehaviour
{
    [Header("Teleport Destination")]
    [Tooltip("Where the player teleports to")]
    public Transform destinationPoint;

    [Header("OR Use Coordinates")]
    [Tooltip("If no destination point, use these coordinates")]
    public bool useCoordinates = false;
    public Vector2 targetPosition = Vector2.zero;

    [Header("Player Settings")]
    [Tooltip("Direction player should face after teleport")]
    public TeleportDirection facingDirection = TeleportDirection.Down;

    [Header("Walk-In Animation")]
    [Tooltip("Should player walk in after teleporting?")]
    public bool walkInAfterTeleport = true;
    public float walkInDistance = 1f;
    public float walkInSpeed = 3f;

    [Header("Lock Player")]
    [Tooltip("Disable player control during teleport")]
    public bool lockPlayerDuringTeleport = true;
    public float lockDuration = 0.3f;

    [Header("Fade Transition")]
    [Tooltip("Fade screen to black during teleport")]
    public bool useFadeTransition = true;
    public float fadeOutSpeed = 3f;
    public float fadeInSpeed = 3f;
    public Color fadeColor = Color.black;

    [Header("Visual")]
    public bool showGizmos = true;
    public Color gizmoColor = new Color(0f, 1f, 1f, 0.3f);

    private BoxCollider2D teleportCollider;
    private bool isTeleporting = false;
    private TeleportFadeManager fadeManager;

    void Awake()
    {
        teleportCollider = GetComponent<BoxCollider2D>();
        teleportCollider.isTrigger = true;

        // Find or create fade manager
        fadeManager = FindObjectOfType<TeleportFadeManager>();
        if (fadeManager == null && useFadeTransition)
        {
            Debug.LogWarning("No TeleportFadeManager found! Create one for fade transitions.");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isTeleporting) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player entered teleporter: {gameObject.name}");
            StartTeleport(other.gameObject);
        }
    }

    void StartTeleport(GameObject player)
    {
        Vector3 targetPos = GetTargetPosition();

        if (targetPos == Vector3.zero && destinationPoint == null && !useCoordinates)
        {
            Debug.LogError($"Teleporter '{gameObject.name}' has no destination set!");
            return;
        }

        isTeleporting = true;
        StartCoroutine(TeleportCoroutine(player, targetPos));
    }

    IEnumerator TeleportCoroutine(GameObject player, Vector3 targetPos)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        // Lock player movement
        if (lockPlayerDuringTeleport && movement != null)
        {
            movement.enabled = false;
        }

        // Fade out
        if (useFadeTransition && fadeManager != null)
        {
            yield return StartCoroutine(fadeManager.FadeOut(fadeOutSpeed, fadeColor));
        }
        else
        {
            // Wait a moment if no fade
            yield return new WaitForSeconds(lockDuration);
        }

        // Teleport player
        if (walkInAfterTeleport)
        {
            // Calculate walk-in start position
            Vector3 walkInStart = targetPos - GetDirectionVector() * walkInDistance;
            player.transform.position = walkInStart;

            // Fade in first
            if (useFadeTransition && fadeManager != null)
            {
                yield return StartCoroutine(fadeManager.FadeIn(fadeInSpeed));
            }

            // Then walk player in
            yield return StartCoroutine(WalkPlayerIn(player, targetPos));
        }
        else
        {
            // Instant teleport
            player.transform.position = targetPos;

            // Fade in
            if (useFadeTransition && fadeManager != null)
            {
                yield return StartCoroutine(fadeManager.FadeIn(fadeInSpeed));
            }
        }

        // Re-enable player movement
        if (movement != null)
        {
            movement.enabled = true;
        }

        isTeleporting = false;
        Debug.Log($"Teleport complete to {targetPos}");
    }

    IEnumerator WalkPlayerIn(GameObject player, Vector3 targetPos)
    {
        Vector3 direction = GetDirectionVector();

        while (Vector3.Distance(player.transform.position, targetPos) > 0.1f)
        {
            player.transform.position += direction * walkInSpeed * Time.deltaTime;
            yield return null;
        }

        player.transform.position = targetPos;
    }

    Vector3 GetTargetPosition()
    {
        if (destinationPoint != null)
        {
            return destinationPoint.position;
        }
        else if (useCoordinates)
        {
            return new Vector3(targetPosition.x, targetPosition.y, 0);
        }
        else
        {
            return Vector3.zero;
        }
    }

    Vector3 GetDirectionVector()
    {
        return facingDirection switch
        {
            TeleportDirection.Up => Vector3.up,
            TeleportDirection.Down => Vector3.down,
            TeleportDirection.Left => Vector3.left,
            TeleportDirection.Right => Vector3.right,
            _ => Vector3.down
        };
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        // Draw teleporter area
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);

        // Draw border
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
        DrawBorder(col.bounds);

        // Draw arrow to destination
        Vector3 targetPos = GetTargetPosition();
        if (targetPos != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, targetPos);
            
            // Draw destination marker
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPos, 0.5f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 targetPos = GetTargetPosition();
        if (targetPos == Vector3.zero) return;

#if UNITY_EDITOR
        // Draw label
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.cyan;
        style.fontSize = 12;
        style.fontStyle = FontStyle.Bold;

        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 1f,
            $"Teleporter\n→ ({targetPos.x}, {targetPos.y})",
            style
        );

        // Draw destination label
        style.normal.textColor = Color.green;
        UnityEditor.Handles.Label(
            targetPos + Vector3.up * 1f,
            $"Destination\nFacing: {facingDirection}",
            style
        );
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
}

public enum TeleportDirection
{
    Up,
    Down,
    Left,
    Right
}