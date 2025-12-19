using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class PlayerFarming : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap soilTilemap;
    public Tilemap cropTilemap;

    [Header("Crop")]
    public CropData cropData;

    [Header("Tiles")]
    public TileBase tilledSoilTile;

    [Header("References")]
    public TilemapIndicator indicator;
    public PlayerInteraction playerInteraction;
    public DialogueManager dialogueManager;

    Dictionary<Vector3Int, CropInstance> crops =
        new Dictionary<Vector3Int, CropInstance>();

    void Update()
    {
        UpdateCrops();
    }

    public void TryFarm()
    {
        if (dialogueManager != null && dialogueManager.IsOpen) return;
        if (playerInteraction != null && playerInteraction.HasInteractable) return;

        Vector3Int cell =
            soilTilemap.WorldToCell(indicator.transform.position);

        TileBase soil = soilTilemap.GetTile(cell);
        TileBase cropTile = cropTilemap.GetTile(cell);

        if (soil == null)
        {
            soilTilemap.SetTile(cell, tilledSoilTile);
            return;
        }

        if (soil != null && cropTile == null)
        {
            CropInstance instance = new CropInstance(cell, cropData);
            crops.Add(cell, instance);

            cropTilemap.SetTile(cell, cropData.seedTile);
            return;
        }

        if (cropTile != null && crops.ContainsKey(cell))
        {
            CropInstance crop = crops[cell];
            if (crop.CanHarvest)
            {
                HarvestCrop(cell, crop);
            }
        }
    }


    void UpdateCrops()
    {
        foreach (var kvp in crops)
        {
            CropInstance crop = kvp.Value;

            switch (crop.GetStage())
            {
                case CropStage.Seed:
                    cropTilemap.SetTile(crop.cell, crop.data.seedTile);
                    break;

                case CropStage.Sprout:
                    cropTilemap.SetTile(crop.cell, crop.data.sproutTile);
                    break;

                case CropStage.Grown:
                    cropTilemap.SetTile(crop.cell, crop.data.grownTile);
                    break;
            }
        }
    }

    void HarvestCrop(Vector3Int cell, CropInstance crop)
    {
        // Remove crop tile
        cropTilemap.SetTile(cell, null);
        crops.Remove(cell);

        Vector3 worldPos =
            cropTilemap.CellToWorld(cell) +
            cropTilemap.cellSize / 2f;

        SpawnHarvestBurst(crop.data, worldPos);
    }

    void SpawnHarvestBurst(CropData data, Vector3 position)
    {
        if (data.harvestPrefab == null)
        {
            Debug.LogWarning("Harvest prefab missing!");
            return;
        }

        int count = Random.Range(
            data.minHarvestAmount,
            data.maxHarvestAmount + 1
        );

        for (int i = 0; i < count; i++)
        {
            GameObject item = Instantiate(
                data.harvestPrefab,
                position,
                Quaternion.identity
            );

            Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Random burst direction (upward bias)
                Vector2 dir = Random.insideUnitCircle;
                dir.y = Mathf.Abs(dir.y);

                rb.AddForce(
                    dir.normalized * data.burstForce,
                    ForceMode2D.Impulse
                );

                // Spin
                rb.AddTorque(
                    Random.Range(-data.burstTorque, data.burstTorque),
                    ForceMode2D.Impulse
                );
            }

            // Auto despawn
            Destroy(item, data.harvestDespawnTime);
        }
    }
}
