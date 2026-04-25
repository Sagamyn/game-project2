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
    public DayNightManager dayNightManager;

    [Header("Selected Item (from Hotbar)")]
    public ItemData selectedItem;

    [Header("Farming Zones")]
    public bool requireZone = true;
    public bool showZoneWarning = true;
    private FarmingZone currentZone;

    private float lastWarningTime = 0f;
    private float warningCooldown = 2f;

    Dictionary<Vector3Int, Crop> crops = new Dictionary<Vector3Int, Crop>();
    Dictionary<Vector3Int, bool> wateredSoil = new Dictionary<Vector3Int, bool>();

    // ─────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────

    void Start()
    {
        if (dayNightManager == null)
            Debug.LogWarning("[PlayerFarming] DayNightManager not assigned.");
    }

    void Update()
    {
        foreach (var kvp in crops)
            kvp.Value.Tick(Time.deltaTime, cropTilemap);
    }

    // ─────────────────────────────────────────
    // SOIL VISUALS
    // ─────────────────────────────────────────

    void UpdateSoilVisual(Vector3Int cell, bool isWatered)
    {
        soilTilemap.SetTile(cell, isWatered ? wetSoilTile : tilledSoilTile);
    }

    // ─────────────────────────────────────────
    // ZONE MANAGEMENT
    // ─────────────────────────────────────────

    public void EnterFarmingZone(FarmingZone zone)
    {
        currentZone = zone;
        Debug.Log($"[FarmingZone] Entered: {zone.zoneName}");
    }

    public void ExitFarmingZone(FarmingZone zone)
    {
        if (currentZone == zone)
        {
            currentZone = null;
            Debug.Log($"[FarmingZone] Exited: {zone.zoneName}");
        }
    }

    bool CanFarmAtCell(Vector3Int cell, FarmingAction action)
    {
        if (!requireZone) return true;

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
        if (Time.time - lastWarningTime < warningCooldown) return;

        lastWarningTime = Time.time;
        Debug.LogWarning(message);

        if (dialogueManager != null)
            dialogueManager.ShowDialogue(message);
    }

    // ─────────────────────────────────────────
    // FARM ACTION via E key (planting + fallback harvest)
    // ─────────────────────────────────────────

    public void TryFarm()
    {
        if (dialogueManager != null && dialogueManager.IsOpen) return;
        if (playerInteraction != null && playerInteraction.HasInteractable) return;
        if (indicator == null) return;

        Vector3Int cell = soilTilemap.WorldToCell(indicator.transform.position);
        TileBase soilTile = soilTilemap.GetTile(cell);
        TileBase cropTile = cropTilemap.GetTile(cell);

        // PLANT only — harvesting is done via left-click with Hoe
        if (soilTile != null && cropTile == null)
        {
            if (!CanFarmAtCell(cell, FarmingAction.Planting)) return;
            if (selectedItem is not SeedItem seedItem) return;
            if (!inventory.HasItem(seedItem)) return;

            CropData data = seedItem.cropData;
            if (data == null)
            {
                Debug.LogError("[PlayerFarming] Seed has no CropData assigned!");
                return;
            }

            int currentDay = dayNightManager != null ? dayNightManager.currentDay : 1;
            Crop newCrop = new Crop(data, cell, currentDay);
            crops[cell] = newCrop;
            newCrop.UpdateVisual(cropTilemap);

            inventory.ConsumeItem(seedItem, 1);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.plantSound);
            wateredSoil[cell] = false;

            Debug.Log($"[PlayerFarming] Planted {data.cropName} at {cell}");
        }
    }

    // ─────────────────────────────────────────
    // HARVEST via LEFT CLICK (from PlayerActionController)
    // ─────────────────────────────────────────

    /// <summary>
    /// Called when the player left-clicks while the Hoe is equipped.
    /// The cell is derived from the mouse world position in PlayerActionController.
    /// </summary>
    public void TryHarvestAtCell(Vector3Int cell)
    {
        if (dialogueManager != null && dialogueManager.IsOpen) return;

        TileBase cropTile = cropTilemap.GetTile(cell);
        if (cropTile == null)
        {
            Debug.Log($"[PlayerFarming] No crop at clicked cell {cell}");
            return;
        }

        if (!crops.ContainsKey(cell))
        {
            Debug.Log($"[PlayerFarming] No tracked crop at {cell}");
            return;
        }

        if (!CanFarmAtCell(cell, FarmingAction.Harvesting)) return;

        Crop crop = crops[cell];

        if (crop.IsHarvestable)
        {
            HarvestCrop(cell, crop);
        }
        else
        {
            string info = crop.GetStageInfo();
            Debug.Log($"[PlayerFarming] Crop not ready — {info}");

            if (dialogueManager != null)
                dialogueManager.ShowDialogue(info);
        }
    }

    // ─────────────────────────────────────────
    // WATER SYSTEM
    // ─────────────────────────────────────────

    public bool HasCrop(Vector3Int cell) => crops.ContainsKey(cell);

    public void WaterCrop(Vector3Int cell)
    {
        if (!CanFarmAtCell(cell, FarmingAction.Watering)) return;
        if (!crops.ContainsKey(cell)) return;

        crops[cell].Water();
        soilTilemap.SetTile(cell, wetSoilTile);
        Debug.Log($"[PlayerFarming] Watered crop at {cell}");
    }

    public int WaterAllCropsInRain()
    {
        int count = 0;
        foreach (var crop in crops.Values)
        {
            if (!crop.IsWatered)
            {
                crop.Water();
                soilTilemap.SetTile(crop.cellPosition, wetSoilTile);
                count++;
            }
        }
        Debug.Log($"[Weather] Rain watered {count} crops");
        return count;
    }

    // ─────────────────────────────────────────
    // HARVEST
    // ─────────────────────────────────────────

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
        Debug.Log($"[PlayerFarming] Harvested {crop.cropData.cropName} at {cell}");
    }

    void SpawnHarvestBurst(CropData data, Vector3 position)
    {
        if (data.harvestPrefab == null) return;

        int count = Random.Range(data.minHarvestAmount, data.maxHarvestAmount + 1);

        for (int i = 0; i < count; i++)
        {
            GameObject item = Instantiate(
                data.harvestPrefab, position, Quaternion.identity);

            Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.drag = 5f;
                rb.angularDrag = 5f;

                Vector2 dir = Random.insideUnitCircle;
                dir.y = Mathf.Abs(dir.y);
                rb.AddForce(dir.normalized * data.burstForce, ForceMode2D.Impulse);
                rb.AddTorque(
                    Random.Range(-data.burstTorque, data.burstTorque),
                    ForceMode2D.Impulse);
            }
            Destroy(item, data.harvestDespawnTime);
        }
    }

    // ─────────────────────────────────────────
    // PUBLIC HELPERS
    // ─────────────────────────────────────────

    public bool IsInFarmingZone() => currentZone != null;
    public string GetCurrentZoneName() =>
        currentZone != null ? currentZone.zoneName : "None";
    public bool CanPerformAction(Vector3Int cell, FarmingAction action) =>
        CanFarmAtCell(cell, action);
    public string GetCropInfoAtCell(Vector3Int cell) =>
        crops.ContainsKey(cell) ? crops[cell].GetStageInfo() : "No crop planted";

    [ContextMenu("Debug: List All Crops")]
    public void DebugListCrops()
    {
        Debug.Log($"=== CROPS ({crops.Count}) ===");
        foreach (var kvp in crops)
            Debug.Log($"{kvp.Key}: {kvp.Value.GetStageInfo()}");
    }
}