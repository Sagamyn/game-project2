using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Designates areas where player can farm
/// IMPORTANT: Player needs Rigidbody2D for trigger detection!
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class FarmingZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public string zoneName = "Farm Plot";
    public bool allowTilling = true;
    public bool allowPlanting = true;
    public bool allowWatering = true;
    public bool allowHarvesting = true;

    [Header("Visual")]
    public Color zoneColor = new Color(0.5f, 1f, 0.5f, 0.3f);
    public bool showInGame = false;
    public SpriteRenderer zoneVisual;

    [Header("Boundary (Auto-set from collider)")]
    public Vector2Int minCell;
    public Vector2Int maxCell;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private BoxCollider2D zoneCollider;
    private Tilemap referenceTilemap;
    private bool playerInZone = false;

    void Awake()
    {
        zoneCollider = GetComponent<BoxCollider2D>();
        zoneCollider.isTrigger = true;

        Debug.Log($"[FarmingZone] '{zoneName}' Awake - Collider is trigger: {zoneCollider.isTrigger}");

        // Find the soil tilemap
        PlayerFarming farming = FindObjectOfType<PlayerFarming>();
        if (farming != null)
        {
            referenceTilemap = farming.soilTilemap;
            CalculateCellBounds();
        }
        else
        {
            Debug.LogError("[FarmingZone] Could not find PlayerFarming in scene!");
        }

        // Setup visual if present
        if (zoneVisual != null)
        {
            zoneVisual.color = zoneColor;
            zoneVisual.enabled = showInGame;
        }
    }

    void Start()
    {
        // Double-check player has rigidbody
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError("[FarmingZone] CRITICAL: Player does not have Rigidbody2D! Triggers won't work!");
                Debug.LogError("[FarmingZone] → Add Rigidbody2D to Player with Body Type = Kinematic");
            }
            else
            {
                Debug.Log($"[FarmingZone] Player Rigidbody2D found: BodyType={rb.bodyType}");
            }
        }
        else
        {
            Debug.LogError("[FarmingZone] Could not find Player with tag 'Player'!");
        }
    }

    void CalculateCellBounds()
    {
        if (referenceTilemap == null)
        {
            Debug.LogError($"[FarmingZone] '{zoneName}' - No tilemap reference!");
            return;
        }

        Bounds bounds = zoneCollider.bounds;
        
        Vector3 bottomLeft = bounds.min;
        Vector3 topRight = bounds.max;
        
        Vector3Int minCellPos = referenceTilemap.WorldToCell(bottomLeft);
        Vector3Int maxCellPos = referenceTilemap.WorldToCell(topRight);

        minCell = new Vector2Int(minCellPos.x, minCellPos.y);
        maxCell = new Vector2Int(maxCellPos.x, maxCellPos.y);

        if (showDebugLogs)
        {
            Debug.Log($"[FarmingZone] '{zoneName}' BOUNDS CALCULATED:");
            Debug.Log($"  World Bounds: {bounds.min} to {bounds.max}");
            Debug.Log($"  Cell Bounds: {minCell} to {maxCell}");
            Debug.Log($"  Coverage: {maxCell.x - minCell.x + 1} x {maxCell.y - minCell.y + 1} cells");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[FarmingZone] OnTriggerEnter2D called! other={other.name}, tag={other.tag}");

        if (other.CompareTag("Player"))
        {
            playerInZone = true;

            PlayerFarming farming = other.GetComponent<PlayerFarming>();
            if (farming != null)
            {
                farming.EnterFarmingZone(this);
                Debug.Log($"✓✓✓ PLAYER ENTERED ZONE: {zoneName} ✓✓✓");
            }
            else
            {
                Debug.LogError($"[FarmingZone] Player has no PlayerFarming component!");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"[FarmingZone] OnTriggerExit2D called! other={other.name}");

        if (other.CompareTag("Player"))
        {
            playerInZone = false;

            PlayerFarming farming = other.GetComponent<PlayerFarming>();
            if (farming != null)
            {
                farming.ExitFarmingZone(this);
                Debug.Log($"✗✗✗ PLAYER EXITED ZONE: {zoneName} ✗✗✗");
            }
        }
    }

    // CRITICAL: Also check OnTriggerStay for continuous detection
    void OnTriggerStay2D(Collider2D other)
    {
        if (!playerInZone && other.CompareTag("Player"))
        {
            Debug.LogWarning($"[FarmingZone] Player in OnTriggerStay but not marked as in zone! Fixing...");
            OnTriggerEnter2D(other);
        }
    }

    public bool IsCellInZone(Vector3Int cell)
    {
        bool inZone = cell.x >= minCell.x && cell.x <= maxCell.x &&
                      cell.y >= minCell.y && cell.y <= maxCell.y;
        
        if (showDebugLogs)
        {
            if (!inZone)
            {
                Debug.Log($"[FarmingZone] Cell {cell} is OUTSIDE bounds ({minCell} to {maxCell})");
            }
            else
            {
                Debug.Log($"[FarmingZone] Cell {cell} is INSIDE bounds ({minCell} to {maxCell})");
            }
        }
        
        return inZone;
    }

    public bool IsActionAllowed(FarmingAction action)
    {
        return action switch
        {
            FarmingAction.Tilling => allowTilling,
            FarmingAction.Planting => allowPlanting,
            FarmingAction.Watering => allowWatering,
            FarmingAction.Harvesting => allowHarvesting,
            _ => false
        };
    }

    [ContextMenu("Recalculate Cell Bounds")]
    void EditorRecalculateBounds()
    {
        if (referenceTilemap == null)
        {
            PlayerFarming farming = FindObjectOfType<PlayerFarming>();
            if (farming != null)
                referenceTilemap = farming.soilTilemap;
        }

        CalculateCellBounds();
    }

    [ContextMenu("Check Player Rigidbody")]
    void CheckPlayerRigidbody()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("No GameObject with tag 'Player' found!");
            return;
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Player does not have Rigidbody2D! ADD IT with Body Type = Kinematic");
        }
        else
        {
            Debug.Log($"✓ Player has Rigidbody2D: BodyType={rb.bodyType}, Simulated={rb.simulated}");
        }

        Collider2D col = player.GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("Player does not have any Collider2D!");
        }
        else
        {
            Debug.Log($"✓ Player has Collider2D: Type={col.GetType().Name}, IsTrigger={col.isTrigger}");
        }
    }

    void OnDrawGizmos()
    {
        if (zoneCollider == null)
            zoneCollider = GetComponent<BoxCollider2D>();

        // Draw zone area
        Gizmos.color = zoneColor;
        Gizmos.DrawCube(zoneCollider.bounds.center, zoneCollider.bounds.size);

        // Draw border
        Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 1f);
        DrawBorder(zoneCollider.bounds);

        // Draw cell grid in play mode
        if (referenceTilemap != null && Application.isPlaying)
        {
            DrawCellGrid();
        }
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

    void DrawCellGrid()
    {
        Gizmos.color = new Color(1f, 1f, 1f, 0.3f);

        for (int x = minCell.x; x <= maxCell.x + 1; x++)
        {
            Vector3 start = referenceTilemap.CellToWorld(new Vector3Int(x, minCell.y, 0));
            Vector3 end = referenceTilemap.CellToWorld(new Vector3Int(x, maxCell.y + 1, 0));
            Gizmos.DrawLine(start, end);
        }

        for (int y = minCell.y; y <= maxCell.y + 1; y++)
        {
            Vector3 start = referenceTilemap.CellToWorld(new Vector3Int(minCell.x, y, 0));
            Vector3 end = referenceTilemap.CellToWorld(new Vector3Int(maxCell.x + 1, y, 0));
            Gizmos.DrawLine(start, end);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && referenceTilemap != null)
        {
            Vector3 centerWorld = zoneCollider.bounds.center;
            
#if UNITY_EDITOR
            string label = $"{zoneName}\n";
            label += $"Cells: ({minCell.x}, {minCell.y}) to ({maxCell.x}, {maxCell.y})\n";
            label += $"Size: {maxCell.x - minCell.x + 1}x{maxCell.y - minCell.y + 1}\n";
            label += $"Player In Zone: {playerInZone}";

            UnityEditor.Handles.Label(
                centerWorld + Vector3.up * (zoneCollider.bounds.size.y / 2 + 0.5f),
                label
            );
#endif
        }
    }
}

public enum FarmingAction
{
    Tilling,
    Planting,
    Watering,
    Harvesting
}