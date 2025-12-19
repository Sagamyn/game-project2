using UnityEngine;

public class CropInstance
{
    public Vector3Int cell;
    public CropData data;
    float plantedTime;

    public CropInstance(Vector3Int cell, CropData data)
    {
        this.cell = cell;
        this.data = data;
        plantedTime = Time.time;
    }

    public CropStage GetStage()
    {
        float elapsed = Time.time - plantedTime;

        if (elapsed < data.seedTime)
            return CropStage.Seed;

        if (elapsed < data.seedTime + data.sproutTime)
            return CropStage.Sprout;

        return CropStage.Grown;
    }

    public bool CanHarvest =>
        GetStage() == CropStage.Grown;
}
