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

    [Header("Farming Zones")]
    public bool requireZone = true;
    public bool showZoneWarning = true;
    private FarmingZone currentZone;
    
    // Cooldown to prevent dialogue spam
    private float lastWarningTime = 0f;
    private float warningCooldown = 2f; // Only show warning once every 2 seconds

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
    // ZONE MANAGEMENT
    // =========================
    public void EnterFarmingZone(FarmingZone zone)
    {
        currentZone = zone;
        Debug.Log($"✓ ENTERED FARMING ZONE: {zone.zoneName}");
    }

    public void ExitFarmingZone(FarmingZone zone)
    {
        if (currentZone == zone)
        {
            currentZone = null;
            Debug.Log($"✗ EXITED FARMING ZONE: {zone.zoneName}");
        }
    }

    // Check if player can perform action at target cell
    bool CanFarmAtCell(Vector3Int cell, FarmingAction action)
    {
        // If zones are not required, allow anywhere
        if (!requireZone)
        {
            return true;
        }

        // If zones are required but player is not in any zone
        if (currentZone == null)
        {
            ShowWarningMessage("You can't farm here! Find a farming zone.");
            return false;
        }

        // Check if the TARGET CELL is within the zone bounds
        if (!currentZone.IsCellInZone(cell))
        {
            ShowWarningMessage("You can't till the soil here!");
            return false;
        }

        // Check if this specific action is allowed in this zone
        if (!currentZone.IsActionAllowed(action))
        {
            ShowWarningMessage($"{action} is not allowed in this zone!");
            return false;
        }

        return true;
    }

    // Show warning with cooldown to prevent spam
    void ShowWarningMessage(string message)
    {
        if (!showZoneWarning) return;

        // Check cooldown
        if (Time.time - lastWarningTime < warningCooldown)
        {
            Debug.Log($"Warning suppressed (cooldown): {message}");
            return;
        }

        lastWarningTime = Time.time;
        Debug.LogWarning(message);

        if (dialogueManager != null)
        {
            dialogueManager.ShowDialogue(message);
        }
    }

    // =========================
    // FARM ACTION
    // =========================
    public void TryFarm()
    {
        // CRITICAL: Don't allow farming if dialogue is open
        if (dialogueManager != null && dialogueManager.IsOpen)
        {
            Debug.Log("TryFarm blocked - dialogue is open");
            return;
        }

        if (playerInteraction != null && playerInteraction.HasInteractable)
        {
            Debug.Log("TryFarm blocked - has interactable");
            return;
        }

        if (indicator == null)
        {
            Debug.LogWarning("TryFarm blocked - no indicator");
            return;
        }

        Vector3Int cell = soilTilemap.WorldToCell(indicator.transform.position);
        TileBase soilTile = soilTilemap.GetTile(cell);
        TileBase cropTile = cropTilemap.GetTile(cell);

        // PLANT
        if (soilTile != null && cropTile == null)
        {
            // Check if can plant in this zone
            if (!CanFarmAtCell(cell, FarmingAction.Planting))
                return;

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
            wateredSoil[cell] = false;
            return;
        }

        // HARVEST
        if (cropTile != null && crops.ContainsKey(cell))
        {
            // Check if can harvest in this zone
            if (!CanFarmAtCell(cell, FarmingAction.Harvesting))
                return;

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
        // Check if can water in this zone
        if (!CanFarmAtCell(cell, FarmingAction.Watering))
            return;

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

    // =========================
    // PUBLIC HELPERS
    // =========================
    
    public bool IsInFarmingZone()
    {
        return currentZone != null;
    }

    public string GetCurrentZoneName()
    {
        return currentZone != null ? currentZone.zoneName : "None";
    }

    public bool CanPerformAction(Vector3Int cell, FarmingAction action)
    {
        return CanFarmAtCell(cell, action);
    }
}