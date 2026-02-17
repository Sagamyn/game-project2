using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Makes tilemap crops shake when player walks near them
/// More efficient for grid-based farming
/// </summary>
public class CropShakeManager : MonoBehaviour
{
    [Header("References")]
    public Tilemap cropTilemap; // Your crop tilemap
    public Transform playerTransform;
    
    [Header("Detection")]
    public float shakeRadius = 1.5f; // Tiles within this range shake
    
    [Header("Shake Settings")]
    public float shakeDuration = 0.3f;
    public float shakeIntensity = 0.15f;
    public float shakeSpeed = 25f;
    
    private Dictionary<Vector3Int, Coroutine> shakingTiles = new Dictionary<Vector3Int, Coroutine>();
    private Dictionary<Vector3Int, Matrix4x4> originalMatrices = new Dictionary<Vector3Int, Matrix4x4>();
    private Vector3Int lastPlayerCell;

    void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (cropTilemap == null || playerTransform == null) return;

        // Get player's current cell
        Vector3Int playerCell = cropTilemap.WorldToCell(playerTransform.position);

        // Only check if player moved to new cell
        if (playerCell != lastPlayerCell)
        {
            lastPlayerCell = playerCell;
            CheckNearbyTiles(playerCell);
        }
    }

    void CheckNearbyTiles(Vector3Int centerCell)
    {
        // Check tiles in a radius
        int radius = Mathf.CeilToInt(shakeRadius);
        
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int checkCell = centerCell + new Vector3Int(x, y, 0);
                
                // Check if tile exists
                TileBase tile = cropTilemap.GetTile(checkCell);
                if (tile != null)
                {
                    // Check distance
                    Vector3 tileWorldPos = cropTilemap.CellToWorld(checkCell) + cropTilemap.cellSize * 0.5f;
                    float distance = Vector3.Distance(playerTransform.position, tileWorldPos);
                    
                    if (distance <= shakeRadius)
                    {
                        // Shake this tile!
                        if (!shakingTiles.ContainsKey(checkCell))
                        {
                            Coroutine shake = StartCoroutine(ShakeTile(checkCell));
                            shakingTiles[checkCell] = shake;
                        }
                    }
                }
            }
        }
    }

    IEnumerator ShakeTile(Vector3Int cell)
    {
        // Store original transform
        if (!originalMatrices.ContainsKey(cell))
        {
            originalMatrices[cell] = cropTilemap.GetTransformMatrix(cell);
        }
        
        Matrix4x4 originalMatrix = originalMatrices[cell];
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;
            float currentIntensity = shakeIntensity * (1f - progress);

            // Calculate shake rotation
            float angle = Mathf.Sin(Time.time * shakeSpeed) * 15f * (1f - progress);
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            // Calculate shake scale
            float scaleOffset = Mathf.Sin(Time.time * shakeSpeed * 2f) * currentIntensity;
            Vector3 scale = Vector3.one + new Vector3(scaleOffset, -scaleOffset * 0.5f, 0f);

            // Apply transform
            Matrix4x4 shakeMatrix = Matrix4x4.TRS(Vector3.zero, rotation, scale) * originalMatrix;
            cropTilemap.SetTransformMatrix(cell, shakeMatrix);

            yield return null;
        }

        // Reset to original
        cropTilemap.SetTransformMatrix(cell, originalMatrix);
        shakingTiles.Remove(cell);
    }

    void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(playerTransform.position, shakeRadius);
        }
    }
}