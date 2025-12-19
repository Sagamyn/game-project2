using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerTileInteractor : MonoBehaviour
{
    [Header("References")]
    public Tilemap referenceTilemap;   // Assign GroundTilemap
    public PlayerMovement playerMovement;

    [Header("Settings")]
    public Vector2 feetOffset = new Vector2(0, -0.25f);

    public Vector3Int GetTargetCell()
    {
        Vector2 facing = new Vector2(
            Mathf.Round(playerMovement.GetLastMoveX()),
            Mathf.Round(playerMovement.GetLastMoveY())
        );

        Vector3 origin = transform.position + (Vector3)feetOffset;
        Vector3 targetWorld = origin + (Vector3)facing;

        return referenceTilemap.WorldToCell(targetWorld);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Vector3Int cell = GetTargetCell();
            Debug.Log("Target tile: " + cell);
        }
    }

    void OnDrawGizmos()
    {
        if (!referenceTilemap) return;

        Vector3Int cell = GetTargetCell();
        Vector3 center =
            referenceTilemap.CellToWorld(cell) +
            referenceTilemap.cellSize / 2f;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, referenceTilemap.cellSize);
    }
}
