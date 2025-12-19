using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private Interactable currentInteractable;

    public bool HasInteractable => currentInteractable != null;

    public void TryInteract()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
            interactable.ShowBubble();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable != null && interactable == currentInteractable)
        {
            interactable.HideBubble();
            currentInteractable = null;
        }
    }
}
