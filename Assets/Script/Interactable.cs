using UnityEngine;

public class Interactable : MonoBehaviour
{
    Animator anim;

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    public virtual void Interact()
    {
        Debug.Log("Interacted with " + gameObject.name);
    }

    public void ShowBubble()
    {
        if (anim != null)
            anim.SetBool("ShowBubble", true);
    }

    public void HideBubble()
    {
        if (anim != null)
            anim.SetBool("ShowBubble", false);
    }
}
