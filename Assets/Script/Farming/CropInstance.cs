using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Enhanced crop with multiple growth stages
/// Each crop can have different number of stages (seeds, sprout, etc.)
/// </summary>
public class Crop
{
    public CropData cropData;
    public Vector3Int cellPosition;
    public int currentStage = 0; // 0 = first stage (seed)
    public float stageProgress = 0f; // Days in current stage
    
    private bool isWatered = false;
    private int dayPlanted = 0;
    
    public bool IsWatered => isWatered;
    public bool IsFullyGrown => currentStage >= cropData.GetTotalStages() - 1;
    public bool IsHarvestable => IsFullyGrown;

    public Crop(CropData data, Vector3Int cell, int plantedDay)
    {
        cropData = data;
        cellPosition = cell;
        currentStage = 0; // Start at seed stage
        stageProgress = 0f;
        dayPlanted = plantedDay;
        isWatered = false;
    }

    public void Water()
    {
        isWatered = true;
    }

    // Called at the start of each new day
    public bool GrowForNewDay(Tilemap cropTilemap)
    {
        bool grewThisDay = false;

        // Check if this stage requires water
        if (cropData.growthStages[currentStage].requiresWaterThisStage)
        {
            if (!isWatered)
            {
                Debug.Log($"Crop at {cellPosition} didn't grow - not watered");
                isWatered = false; // Reset water status
                return false;
            }
        }

        // Grow!
        stageProgress += 1f; // +1 day

        // Check if ready for next stage
        float daysNeeded = cropData.growthStages[currentStage].timeToNextStage;
        
        if (stageProgress >= daysNeeded)
        {
            if (currentStage < cropData.GetTotalStages() - 1)
            {
                // Advance to next stage
                currentStage++;
                stageProgress = 0f;
                grewThisDay = true;
                
                Debug.Log($"🌱 Crop advanced to stage {currentStage}: {cropData.GetStageName(currentStage)}");
                
                // Update visual
                UpdateVisual(cropTilemap);
            }
        }

        // Reset water status for next day
        isWatered = false;

        return grewThisDay;
    }

    public void UpdateVisual(Tilemap cropTilemap)
    {
        TileBase stageTile = cropData.GetStageVisual(currentStage);
        
        if (stageTile != null)
        {
            cropTilemap.SetTile(cellPosition, stageTile);
        }
        else
        {
            Debug.LogWarning($"No tile for stage {currentStage} of {cropData.cropName}");
        }
    }

    public string GetStageInfo()
    {
        string stageName = cropData.GetStageName(currentStage);
        float daysNeeded = cropData.growthStages[currentStage].timeToNextStage;
        float daysRemaining = daysNeeded - stageProgress;
        
        if (IsFullyGrown)
        {
            return $"{cropData.cropName} - Ready to Harvest! 🌾";
        }
        else
        {
            return $"{cropData.cropName} - {stageName} ({daysRemaining:F1} days to next stage)";
        }
    }

    public int GetCurrentStage()
    {
        return currentStage;
    }

    public int GetTotalStages()
    {
        return cropData.GetTotalStages();
    }

    public float GetGrowthPercentage()
    {
        if (cropData.GetTotalStages() == 0) return 0f;
        
        // Calculate overall progress
        float completedStages = currentStage;
        float currentStageProgress = 0f;
        
        if (currentStage < cropData.growthStages.Length)
        {
            float daysNeeded = cropData.growthStages[currentStage].timeToNextStage;
            currentStageProgress = stageProgress / daysNeeded;
        }
        
        float totalProgress = (completedStages + currentStageProgress) / cropData.GetTotalStages();
        return Mathf.Clamp01(totalProgress);
    }
}