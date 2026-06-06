using UnityEngine;

public enum BuffType
{
    HealPlayer,
    MaxHealthUp,
    InvincibleNextWave,
    DoublePayNextWave,
    BonusPerHappyCustomer,
    RefundOnAngry,
    MorePatience,
    NoPatienceNextWave,
    FewerCustomers,
    WrongFoodNoLeave,
    SkipOneAngryCustomer,
    FreeRerollNextMerchant,
    RandomBuffEachWave
}

public enum BuffRarity
{
    Common,
    Rare,
    Epic
}

[CreateAssetMenu(fileName = "NewBuff", menuName = "FoodStall/Buff")]
public class BuffData : ScriptableObject
{
    [Header("Info")]
    public string buffName;
    public BuffType buffType;
    public BuffRarity rarity;

    [Header("Card Sprite")]
    public Sprite cardSprite;   // drag your sliced card sprite here
    public Sprite cardSpriteOutline;

    [Header("Description")]
    [TextArea(2, 4)]
    public string description;

    [Header("Price")]
    public int price;

    [Header("Value")]
    public float value;

    [Header("Merchant Reaction")]
    [TextArea(1, 3)]
    public string merchantReaction;
}