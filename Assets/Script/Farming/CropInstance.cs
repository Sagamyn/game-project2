using UnityEngine;
using UnityEngine.Tilemaps;

public class Crop
{
    public CropData cropData;
    public Vector3Int cellPosition;
    public int currentStage = 0;
    public float stageTimer = 0f;

    private bool isWatered = false;
    private int dayPlanted = 0;

    public bool IsWatered => isWatered;
    public bool IsFullyGrown => currentStage >= cropData.GetTotalStages() - 1;
    public bool IsHarvestable => IsFullyGrown;

    public Crop(CropData data, Vector3Int cell, int plantedDay)
    {
        cropData = data;
        cellPosition = cell;
        currentStage = 0;
        stageTimer = 0f;
        dayPlanted = plantedDay;
        isWatered = false;
    }

    public void Water()
    {
        isWatered = true;
    }

    /// <summary>
    /// Call every frame from PlayerFarming.Update().
    /// Returns true if the crop advanced to a new stage this frame.
    /// </summary>
    public bool Tick(float deltaTime, Tilemap cropTilemap)
    {
        if (IsFullyGrown) return false;

        CropStageData stage = cropData.growthStages[currentStage];

        // Pause growth if water is required but crop is dry
        if (stage.requiresWaterThisStage && !isWatered) return false;

        // Safety: stage with 0 time never auto-advances
        if (stage.timeToNextStage <= 0f) return false;

        stageTimer += deltaTime;

        if (stageTimer >= stage.timeToNextStage)
        {
            stageTimer = 0f;
            currentStage++;

            Debug.Log($"[Crop] {cropData.cropName} at {cellPosition} -> stage {currentStage}: {cropData.GetStageName(currentStage)}");

            UpdateVisual(cropTilemap);
            return true;
        }

        return false;
    }

    public void UpdateVisual(Tilemap cropTilemap)
    {
        TileBase tile = cropData.GetStageVisual(currentStage);
        if (tile != null)
            cropTilemap.SetTile(cellPosition, tile);
        else
            Debug.LogWarning($"[Crop] No tile for stage {currentStage} of {cropData.cropName}");
    }

    public string GetStageInfo()
    {
        if (IsFullyGrown)
            return $"{cropData.cropName} — Ready to harvest!";

        float needed = cropData.growthStages[currentStage].timeToNextStage;
        float remaining = Mathf.Max(0f, needed - stageTimer);
        string stageName = cropData.GetStageName(currentStage);
        return $"{cropData.cropName} — {stageName} ({remaining:F1}s left)";
    }

    public int GetCurrentStage() => currentStage;
    public int GetTotalStages() => cropData.GetTotalStages();

    public float GetGrowthPercentage()
    {
        if (cropData.GetTotalStages() == 0) return 0f;

        float currentStageProgress = 0f;
        if (currentStage < cropData.growthStages.Length)
        {
            float needed = cropData.growthStages[currentStage].timeToNextStage;
            if (needed > 0f)
                currentStageProgress = stageTimer / needed;
        }

        return Mathf.Clamp01((currentStage + currentStageProgress) / cropData.GetTotalStages());
    }
}