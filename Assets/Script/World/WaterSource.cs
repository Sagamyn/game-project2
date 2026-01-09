using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSource : Interactable
{
    public PlayerToolController toolController;

    public override void Interact()
    {
        toolController.RefillWater();
    }
}

