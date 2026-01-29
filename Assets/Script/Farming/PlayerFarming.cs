using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class PlayerFarming : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap soilTilemap;
    public Tilemap cropTilemap;

    [Header("Soil Tiles")]
    public TileBase tilledSoilTile;
    public TileBase wetSoilTile;

    [Header("References")]
    public TilemapIndicator indicator;
    public PlayerInteraction playerInteraction;
    public DialogueManager dialogueManager;
    public PlayerInventory inventory;

    [Header("Selected Item (from Hotbar)")]
    public ItemData selectedItem;

    // Crops planted on the field
    Dictionary<Vector3Int, CropInstance> crops = new();

    // Soil wet state
    Dictionary<Vector3Int, bool> wateredSoil = new();

    void Update()
    {
        float dt = Time.deltaTime;

        foreach (var crop in crops.Values)
            crop.Tick(dt);

        UpdateCropsVisual();
    }

    // =========================
    // FARM ACTION
    // =========================
    public void TryFarm()
    {
        if (dialogueManager != null && dialogueManager.IsOpen) return;
        if (playerInteraction != null && playerInteraction.HasInteractable) return;
        if (indicator == null) return;

        Vector3Int cell =
            soilTilemap.WorldToCell(indicator.transform.position);

        TileBase soilTile = soilTilemap.GetTile(cell);
        TileBase cropTile = cropTilemap.GetTile(cell);

        //  PLANT
        if (soilTile != null && cropTile == null)
        {
            if (selectedItem is not SeedItem seedItem)
                return;

            if (!inventory.HasItem(seedItem))
                return;

            CropData data = seedItem.cropData;
            if (data == null) return;

            crops[cell] = new CropInstance(cell, data);

            cropTilemap.SetTile(cell, data.seedTile);
            inventory.ConsumeItem(seedItem, 1);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.plantSound);
            // New crop starts dry
            wateredSoil[cell] = false;
            return;
        }

        // HARVEST
        if (cropTile != null && crops.ContainsKey(cell))
        {
            CropInstance crop = crops[cell];
            if (crop.CanHarvest)
                HarvestCrop(cell, crop);
        }
    }

    // =========================
    // WATER SYSTEM
    // =========================
    public bool HasCrop(Vector3Int cell)
    {
        return crops.ContainsKey(cell);
    }

    public void WaterCrop(Vector3Int cell)
    {
        if (!crops.ContainsKey(cell))
            return;

        crops[cell].Water();
    }

    // =========================
    // VISUALS
    // =========================
    void UpdateCropsVisual()
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

            if (crop.IsWatered)
            soilTilemap.SetTile(crop.cell, wetSoilTile);
            else
            soilTilemap.SetTile(crop.cell, tilledSoilTile);
        }
    }

    // =========================
    // HARVEST
    // =========================
    void HarvestCrop(Vector3Int cell, CropInstance crop)
    {
        cropTilemap.SetTile(cell, null);
        crops.Remove(cell);
        wateredSoil.Remove(cell);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.harvestSound);
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
