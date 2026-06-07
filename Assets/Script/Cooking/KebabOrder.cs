using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Kebab Order", menuName = "Restaurant/Kebab Order")]
public class KebabOrder : ScriptableObject
{
    [Header("Order Info")]
    public string orderName = "Classic Kebab";
    public Sprite orderIcon;

    [Header("Required Ingredients")]
    [Tooltip("These specific ItemData objects must all be in the pan")]
    public List<ItemData> requiredIngredients = new List<ItemData>();

    [Header("Required Categories")]
    [Tooltip("At least one ingredient of this category must be present")]
    public bool requireTortilla = true;
    public bool requireProtein = true;
    public bool requireSauce = false;
    public bool requireVeggies = false;

    [Header("Reward")]
    public int basePrice = 20;
    [Tooltip("Extra coins if ALL optional toppings are also present")]
    public int bonusPrice = 5;
}