using UnityEngine;

public class CropInstance
{
    public Vector3Int cell;
    public CropData data;

    CropStage stage = CropStage.Seed;
    float growthTimer = 0f;
    bool watered = false;

    public CropInstance(Vector3Int cell, CropData data)
    {
        this.cell = cell;
        this.data = data;
    }

    // Called every frame
    public void Tick(float dt)
    {
        if (!watered)
            return;

        growthTimer += dt;

        float stageTime = GetStageDuration();

        if (growthTimer >= stageTime)
        {
            AdvanceStage();
        }
    }

    float GetStageDuration()
    {
        return stage switch
        {
            CropStage.Seed => data.seedTime,
            CropStage.Sprout => data.sproutTime,
            _ => 0f
        };
    }

    void AdvanceStage()
    {
        growthTimer = 0f;
        watered = false; // 🔥 WATER IS CONSUMED

        if (stage == CropStage.Seed)
            stage = CropStage.Sprout;
        else if (stage == CropStage.Sprout)
            stage = CropStage.Grown;
    }

    public void Water()
    {
        if (stage == CropStage.Grown)
            return;

        watered = true;
    }

    public CropStage GetStage() => stage;

    public bool CanHarvest => stage == CropStage.Grown;

    public bool IsWatered => watered;
}
