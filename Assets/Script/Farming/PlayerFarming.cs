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
    public DayNightManager dayNightManager; // NEW: Reference to day/night system

    [Header("Selected Item (from Hotbar)")]
    public ItemData selectedItem;

    [Header("Farming Zones")]
    public bool requireZone = true;
    public bool showZoneWarning = true;
    private FarmingZone currentZone;
    
    // Cooldown to prevent dialogue spam
    private float lastWarningTime = 0f;
    private float warningCooldown = 2f;

    // NEW: Changed from CropInstance to Crop
    Dictionary<Vector3Int, Crop> crops = new Dictionary<Vector3Int, Crop>();

    // Soil wet state
    Dictionary<Vector3Int, bool> wateredSoil = new Dictionary<Vector3Int, bool>();

    void Start()
    {
        // Subscribe to new day event
        if (dayNightManager != null)
        {
            dayNightManager.OnNewDay += OnNewDay;
        }
        else
        {
            Debug.LogWarning("⚠️ DayNightManager not assigned to PlayerFarming! Crops won't grow!");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe
        if (dayNightManager != null)
        {
            dayNightManager.OnNewDay -= OnNewDay;
        }
    }

    // NEW: Called when a new day starts
    void OnNewDay(int newDay)
    {
        Debug.Log($"🌅 NEW DAY {newDay} - Growing crops...");
        
        int cropsGrown = 0;
        
        // Grow all crops
        foreach (var crop in crops.Values)
        {
            bool grew = crop.GrowForNewDay(cropTilemap);
            if (grew)
            {
                cropsGrown++;
            }
            
            // Update soil visual based on water status
            UpdateSoilVisual(crop.cellPosition, crop.IsWatered);
        }
        
        Debug.Log($"✓ {cropsGrown} crops advanced to next stage");
        
        // Update all visuals
        UpdateAllCropVisuals();
    }

    void UpdateSoilVisual(Vector3Int cell, bool isWatered)
    {
        if (isWatered)
            soilTilemap.SetTile(cell, wetSoilTile);
        else
            soilTilemap.SetTile(cell, tilledSoilTile);
    }

    void UpdateAllCropVisuals()
    {
        foreach (var crop in crops.Values)
        {
            crop.UpdateVisual(cropTilemap);
            UpdateSoilVisual(crop.cellPosition, crop.IsWatered);
        }
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
        if (!requireZone)
        {
            return true;
        }

        if (currentZone == null)
        {
            ShowWarningMessage("You can't farm here! Find a farming zone.");
            return false;
        }

        if (!currentZone.IsCellInZone(cell))
        {
            ShowWarningMessage("You can't till the soil here!");
            return false;
        }

        if (!currentZone.IsActionAllowed(action))
        {
            ShowWarningMessage($"{action} is not allowed in this zone!");
            return false;
        }

        return true;
    }

    void ShowWarningMessage(string message)
    {
        if (!showZoneWarning) return;

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
            if (!CanFarmAtCell(cell, FarmingAction.Planting))
                return;

            if (selectedItem is not SeedItem seedItem)
                return;

            if (!inventory.HasItem(seedItem))
                return;

            CropData data = seedItem.cropData;
            if (data == null)
            {
                Debug.LogError("Seed has no CropData assigned!");
                return;
            }

            // Get current day from DayNightManager
            int currentDay = dayNightManager != null ? dayNightManager.currentDay : 1;

            // Create NEW Crop (not CropInstance)
            Crop newCrop = new Crop(data, cell, currentDay);
            crops[cell] = newCrop;

            // Set initial visual
            newCrop.UpdateVisual(cropTilemap);

            inventory.ConsumeItem(seedItem, 1);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.plantSound);
            
            wateredSoil[cell] = false;
            
            Debug.Log($"🌱 Planted {data.cropName} at {cell}");
            return;
        }

        // HARVEST
        if (cropTile != null && crops.ContainsKey(cell))
        {
            if (!CanFarmAtCell(cell, FarmingAction.Harvesting))
                return;

            Crop crop = crops[cell];
            if (crop.IsHarvestable)
            {
                HarvestCrop(cell, crop);
            }
            else
            {
                Debug.Log($"Crop not ready! {crop.GetStageInfo()}");
            }
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
        if (!CanFarmAtCell(cell, FarmingAction.Watering))
            return;

        if (!crops.ContainsKey(cell))
            return;

        crops[cell].Water();
        
        // Update visual immediately
        soilTilemap.SetTile(cell, wetSoilTile);
        
        Debug.Log($"💧 Watered crop at {cell}");
    }

    // Called by WeatherManager during rain
    public int WaterAllCropsInRain()
    {
        int cropsWatered = 0;
        
        foreach (var crop in crops.Values)
        {
            if (!crop.IsWatered)
            {
                crop.Water();
                soilTilemap.SetTile(crop.cellPosition, wetSoilTile);
                cropsWatered++;
            }
        }
        
        Debug.Log($"🌧️ Rain watered {cropsWatered} crops");
        return cropsWatered;
    }

    // =========================
    // HARVEST
    // =========================
    void HarvestCrop(Vector3Int cell, Crop crop)
    {
        cropTilemap.SetTile(cell, null);
        crops.Remove(cell);
        wateredSoil.Remove(cell);
        
        AudioManager.Instance.PlaySFX(AudioManager.Instance.harvestSound);
        
        Vector3 worldPos =
            cropTilemap.CellToWorld(cell) +
            cropTilemap.cellSize / 2f +
            Vector3.up * 0.25f;

        SpawnHarvestBurst(crop.cropData, worldPos);
        
        Debug.Log($"🌾 Harvested {crop.cropData.cropName} at {cell}");
    }

    void SpawnHarvestBurst(CropData data, Vector3 position)
    {
        if (data.harvestPrefab == null)
        {
            Debug.LogWarning($"No harvest prefab for {data.cropName}!");
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

    // NEW: Get crop info at a cell (for UI/debugging)
    public string GetCropInfoAtCell(Vector3Int cell)
    {
        if (crops.ContainsKey(cell))
        {
            return crops[cell].GetStageInfo();
        }
        return "No crop planted";
    }

    // NEW: Debug - show all crops
    [ContextMenu("Debug: List All Crops")]
    public void DebugListCrops()
    {
        Debug.Log($"=== CROPS ({crops.Count}) ===");
        foreach (var kvp in crops)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value.GetStageInfo()}");
        }
    }
}