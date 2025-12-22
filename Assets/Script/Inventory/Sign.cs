using System;
using System.Collections.Generic;
using UnityEngine;

public class Sign : Interactable
{
    [TextArea(3, 5)]
    public List<string> dialoguePages;

    public override void Interact()
    {
        DialogueManager.Instance.ShowDialogue(dialoguePages);
    }
}
