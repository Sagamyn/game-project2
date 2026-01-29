using UnityEngine;

public enum ToolType
{
    Hoe,
    WateringCan,
    Axe,
    Pickaxe,
    Shovel

}

[CreateAssetMenu(menuName = "Items/Tool")]
public class ToolItem : ItemData
{
    public ToolType toolType;
    public int maxDurability = 100;

    [Header("Watering Can")]
    public int maxWater = 10;
}

