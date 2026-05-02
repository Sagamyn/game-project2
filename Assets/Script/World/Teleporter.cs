using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class Teleporter : MonoBehaviour
{
    // ── Destination ──────────────────────────────────────────────────────
    [Header("Teleport Destination")]
    [Tooltip("Where the player teleports to")]
    public Transform destinationPoint;

    [Header("OR Use Coordinates")]
    [Tooltip("If no destination point, use these coordinates")]
    public bool useCoordinates = false;
    public Vector2 targetPosition = Vector2.zero;

    // ── Interaction Mode ─────────────────────────────────────────────────
    [Header("Interaction Mode")]
    [Tooltip("OFF = auto-teleport on walk-in (original behaviour)\nON  = player must press E while in range (door mode)")]
    public bool requireInteraction = false;

    [Tooltip("Animator on the interaction-bubble child (optional). Needs a 'ShowBubble' bool.")]
    public Animator bubbleAnimator;

    // ── Player Settings ──────────────────────────────────────────────────
    [Header("Player Settings")]
    [Tooltip("Direction player should face after teleport")]
    public TeleportDirection facingDirection = TeleportDirection.Down;

    // ── Walk-In Animation ────────────────────────────────────────────────
    [Header("Walk-In Animation")]
    [Tooltip("Should player walk in after teleporting?")]
    public bool walkInAfterTeleport = true;
    public float walkInDistance = 1f;
    public float walkInSpeed = 3f;

    // ── Lock Player ──────────────────────────────────────────────────────
    [Header("Lock Player")]
    [Tooltip("Disable player control during teleport")]
    public bool lockPlayerDuringTeleport = true;
    public float lockDuration = 0.3f;

    // ── Fade Transition ──────────────────────────────────────────────────
    [Header("Fade Transition")]
    [Tooltip("Fade screen to black during teleport")]
    public bool useFadeTransition = true;
    public float fadeOutSpeed = 3f;
    public float fadeInSpeed = 3f;
    public Color fadeColor = Color.black;

    // ── Visual ───────────────────────────────────────────────────────────
    [Header("Visual")]
    public bool showGizmos = true;
    public Color gizmoColor = new Color(0f, 1f, 1f, 0.3f);

    // ── Private ──────────────────────────────────────────────────────────
    private BoxCollider2D teleportCollider;
    private bool isTeleporting = false;
    private TeleportFadeManager fadeManager;

    // Interaction-mode only: the player currently inside the trigger
    private GameObject playerInRange = null;

    // ─────────────────────────────────────────────────────────────────────
    void Awake()
    {
        teleportCollider = GetComponent<BoxCollider2D>();
        teleportCollider.isTrigger = true;

        fadeManager = FindObjectOfType<TeleportFadeManager>();
        if (fadeManager == null && useFadeTransition)
            Debug.LogWarning($"[Teleporter] '{gameObject.name}': No TeleportFadeManager found — fade disabled.");
    }

    // ─────────────────────────────────────────────────────────────────────
    void Update()
    {
        // Only needed in interaction (door) mode
        if (!requireInteraction) return;
        if (isTeleporting) return;
        if (playerInRange == null) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"[Teleporter] '{gameObject.name}': E pressed — starting teleport.");
            StartTeleport(playerInRange);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isTeleporting) return;

        if (requireInteraction)
        {
            // Door mode: register player and show prompt bubble
            playerInRange = other.gameObject;
            SetBubble(true);
            Debug.Log($"[Teleporter] '{gameObject.name}': Player in range — press E to use.");
        }
        else
        {
            // Original auto-trigger mode
            Debug.Log($"[Teleporter] '{gameObject.name}': Player entered — auto teleporting.");
            StartTeleport(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (requireInteraction && playerInRange == other.gameObject)
        {
            playerInRange = null;
            SetBubble(false);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    void StartTeleport(GameObject player)
    {
        Vector3 targetPos = GetTargetPosition();

        if (targetPos == Vector3.zero && destinationPoint == null && !useCoordinates)
        {
            Debug.LogError($"[Teleporter] '{gameObject.name}' has no destination set!");
            return;
        }

        isTeleporting = true;
        SetBubble(false); // hide prompt during teleport
        StartCoroutine(TeleportCoroutine(player, targetPos));
    }

    // ─────────────────────────────────────────────────────────────────────
    IEnumerator TeleportCoroutine(GameObject player, Vector3 targetPos)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        // Lock player movement
        if (lockPlayerDuringTeleport && movement != null)
            movement.enabled = false;

        // Fade out
        if (useFadeTransition && fadeManager != null)
            yield return StartCoroutine(fadeManager.FadeOut(fadeOutSpeed, fadeColor));
        else
            yield return new WaitForSeconds(lockDuration);

        // Move player
        if (walkInAfterTeleport)
        {
            Vector3 walkInStart = targetPos - GetDirectionVector() * walkInDistance;
            player.transform.position = walkInStart;

            if (useFadeTransition && fadeManager != null)
                yield return StartCoroutine(fadeManager.FadeIn(fadeInSpeed));

            yield return StartCoroutine(WalkPlayerIn(player, targetPos));
        }
        else
        {
            player.transform.position = targetPos;

            if (useFadeTransition && fadeManager != null)
                yield return StartCoroutine(fadeManager.FadeIn(fadeInSpeed));
        }

        // Re-enable player
        if (movement != null)
            movement.enabled = true;

        isTeleporting = false;
        Debug.Log($"[Teleporter] '{gameObject.name}': Teleport complete → {targetPos}");
    }

    // ─────────────────────────────────────────────────────────────────────
    IEnumerator WalkPlayerIn(GameObject player, Vector3 targetPos)
    {
        Vector3 direction = GetDirectionVector();

        while (Vector3.Distance(player.transform.position, targetPos) > 0.1f)
        {
            player.transform.position = Vector3.MoveTowards(
                player.transform.position,
                targetPos,
                walkInSpeed * Time.deltaTime
            );
            yield return null;
        }

        player.transform.position = targetPos;
    }

    // ─────────────────────────────────────────────────────────────────────
    void SetBubble(bool show)
    {
        if (bubbleAnimator != null)
            bubbleAnimator.SetBool("ShowBubble", show);
    }

    // ─────────────────────────────────────────────────────────────────────
    Vector3 GetTargetPosition()
    {
        if (destinationPoint != null)
            return destinationPoint.position;
        else if (useCoordinates)
            return new Vector3(targetPosition.x, targetPosition.y, 0);
        else
            return Vector3.zero;
    }

    Vector3 GetDirectionVector()
    {
        return facingDirection switch
        {
            TeleportDirection.Up    => Vector3.up,
            TeleportDirection.Down  => Vector3.down,
            TeleportDirection.Left  => Vector3.left,
            TeleportDirection.Right => Vector3.right,
            _                      => Vector3.down
        };
    }

    // ─────────────────────────────────────────────────────────────────────
    // Gizmos
    // ─────────────────────────────────────────────────────────────────────
    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        // Colour changes based on mode so you can tell them apart in the scene
        Color baseColor = requireInteraction
            ? new Color(1f, 0.6f, 0f, 0.25f)   // orange = door / interaction mode
            : gizmoColor;                        // cyan   = auto-trigger mode

        Gizmos.color = baseColor;
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);

        Gizmos.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
        DrawBorder(col.bounds);

        Vector3 targetPos = GetTargetPosition();
        if (targetPos != Vector3.zero)
        {
            Gizmos.color = requireInteraction ? Color.yellow : Color.cyan;
            Gizmos.DrawLine(transform.position, targetPos);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPos, 0.5f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 targetPos = GetTargetPosition();
        if (targetPos == Vector3.zero) return;

#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.fontSize   = 12;
        style.fontStyle  = FontStyle.Bold;

        style.normal.textColor = requireInteraction ? Color.yellow : Color.cyan;
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 1f,
            requireInteraction
                ? $"Door Teleporter (E)\n→ ({targetPos.x:F1}, {targetPos.y:F1})"
                : $"Teleporter (Auto)\n→ ({targetPos.x:F1}, {targetPos.y:F1})",
            style
        );

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
        Vector3 topLeft     = new Vector3(bounds.min.x, bounds.max.y, 0);
        Vector3 topRight    = new Vector3(bounds.max.x, bounds.max.y, 0);
        Vector3 bottomLeft  = new Vector3(bounds.min.x, bounds.min.y, 0);
        Vector3 bottomRight = new Vector3(bounds.max.x, bounds.min.y, 0);

        Gizmos.DrawLine(topLeft,    topRight);
        Gizmos.DrawLine(topRight,   bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft,  topLeft);
    }
}

public enum TeleportDirection
{
    Up,
    Down,
    Left,
    Right
}