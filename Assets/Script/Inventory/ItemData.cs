using UnityEngine;

public enum ItemType
{
    Seed,
    Crop,
    Tool
}

public abstract class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemType type;

    public bool stackable = true;
    public int maxStack = 99;
}
