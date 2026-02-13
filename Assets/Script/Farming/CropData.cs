using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Enhanced crop data with multiple customizable growth stages
/// Each crop can have different number of stages
/// </summary>
[CreateAssetMenu(fileName = "New Crop", menuName = "Farming/Crop Data")]
public class CropData : ScriptableObject
{
    [Header("Crop Info")]
    public string cropName = "Carrot";
    public Sprite cropIcon;
    
    [Header("Growth Stages")]
    [Tooltip("All growth stages for this crop")]
    public CropStageData[] growthStages;
    
    [Header("Harvest")]
    public GameObject harvestPrefab;
    public int minHarvestAmount = 1;
    public int maxHarvestAmount = 3;
    public float burstForce = 3f;
    public float burstTorque = 50f;
    public float harvestDespawnTime = 30f;
    
    [Header("Requirements")]
    [Tooltip("Must be watered to grow?")]
    public bool requiresWater = true;
    
    [Tooltip("Can only grow in certain seasons (future)")]
    public bool seasonDependent = false;
    
    // Helper methods
    public int GetTotalStages()
    {
        return growthStages != null ? growthStages.Length : 0;
    }
    
    public float GetTotalGrowthTime()
    {
        float total = 0f;
        if (growthStages != null)
        {
            foreach (var stage in growthStages)
            {
                total += stage.timeToNextStage;
            }
        }
        return total;
    }
    
    public TileBase GetStageVisual(int stageIndex)
    {
        if (growthStages == null || stageIndex < 0 || stageIndex >= growthStages.Length)
            return null;
        
        return growthStages[stageIndex].stageTile;
    }
    
    public string GetStageName(int stageIndex)
    {
        if (growthStages == null || stageIndex < 0 || stageIndex >= growthStages.Length)
            return "Unknown";
        
        return growthStages[stageIndex].stageName;
    }
}

[System.Serializable]
public class CropStageData
{
    [Header("Stage Info")]
    public string stageName = "Seed";
    
    [Header("Visual")]
    [Tooltip("Tile to display for this stage")]
    public TileBase stageTile;
    
    [Header("Growth Time")]
    [Tooltip("Days to reach next stage")]
    public float timeToNextStage = 1f; // Days
    
    [Header("Optional: Stage Requirements")]
    [Tooltip("Must be watered during this stage?")]
    public bool requiresWaterThisStage = true;
}