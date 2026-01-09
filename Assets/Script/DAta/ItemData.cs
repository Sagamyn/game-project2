using UnityEngine;

public enum ItemType
{
    Seed,
    Crop,
    Tool,
    Resource
}

public abstract class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemType type;

    public bool stackable = true;
    public int maxStack = 99;
}
