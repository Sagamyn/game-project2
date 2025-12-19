using UnityEngine;
using UnityEngine.Tilemaps;

public enum CropStage
{
    Seed,
    Sprout,
    Grown
}

[CreateAssetMenu(menuName = "Farming/Crop")]
public class CropData : ScriptableObject
{
    public TileBase seedTile;
    public TileBase sproutTile;
    public TileBase grownTile;

    [Header("Growth Time (seconds)")]
    public float seedTime = 5f;
    public float sproutTime = 5f;

    [Header("Harvest")]
    public GameObject harvestPrefab;
    public int minHarvestAmount = 1;
    public int maxHarvestAmount = 3;
    public float burstForce = 3f;
    public float burstTorque = 6f;
    public float harvestDespawnTime = 10f;
}
