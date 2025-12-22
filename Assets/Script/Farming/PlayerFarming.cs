using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class PlayerFarming : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap soilTilemap;
    public Tilemap cropTilemap;

    [Header("Tiles")]
    public TileBase tilledSoilTile;

    [Header("References")]
    public TilemapIndicator indicator;
    public PlayerInteraction playerInteraction;
    public DialogueManager dialogueManager;
    public PlayerInventory inventory;

    [Header("Selected Item (from Hotbar)")]
    public ItemData selectedItem;

    // Active crops on the field
    private Dictionary<Vector3Int, CropInstance> crops =
        new Dictionary<Vector3Int, CropInstance>();

    void Update()
    {
        UpdateCrops();
    }

    public void TryFarm()
    {
        // Block farming during dialogue or interaction
        if (dialogueManager != null && dialogueManager.IsOpen) return;
        if (playerInteraction != null && playerInteraction.HasInteractable) return;
        if (indicator == null || soilTilemap == null || cropTilemap == null) return;

        Vector3Int cell =
            soilTilemap.WorldToCell(indicator.transform.position);

        TileBase soilTile = soilTilemap.GetTile(cell);
        TileBase cropTile = cropTilemap.GetTile(cell);


        if (soilTile == null)
        {
            soilTilemap.SetTile(cell, tilledSoilTile);
            return;
        }

        if (soilTile != null && cropTile == null)
        {
            if (selectedItem == null) return;

            // Selected item MUST be a seed
            if (selectedItem is not SeedItem seedItem) return;

            // Must have seed in inventory
            if (inventory == null || !inventory.HasItem(seedItem)) return;

            CropData data = seedItem.cropData;
            if (data == null) return;

            CropInstance crop = new CropInstance(cell, data);
            crops[cell] = crop;

            cropTilemap.SetTile(cell, data.seedTile);
            inventory.ConsumeItem(seedItem, 1);
            return;
        }

        // 3️⃣ HARVEST
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
        foreach (var crop in crops.Values)
        {
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
        cropTilemap.SetTile(cell, null);
        crops.Remove(cell);

        Vector3 worldPos =
            cropTilemap.CellToWorld(cell) +
            cropTilemap.cellSize / 2f +
            Vector3.up * 0.25f;

        SpawnHarvestBurst(crop.data, worldPos);
    }

    void SpawnHarvestBurst(CropData data, Vector3 position)
    {
        if (data.harvestPrefab == null) return;

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
                rb.gravityScale = 0f;
                rb.drag = 5f;
                rb.angularDrag = 5f;

                Vector2 dir = Random.insideUnitCircle;
                dir.y = Mathf.Abs(dir.y);

                rb.AddForce(
                    dir.normalized * data.burstForce,
                    ForceMode2D.Impulse
                );

                rb.AddTorque(
                    Random.Range(-data.burstTorque, data.burstTorque),
                    ForceMode2D.Impulse
                );
            }

            Destroy(item, data.harvestDespawnTime);
        }
    }
}