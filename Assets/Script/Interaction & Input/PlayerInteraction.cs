using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    Interactable currentInteractable;
    bool canInteract = true;

    public bool HasInteractable => currentInteractable != null;

    public void TryInteract()
    {
        if (!canInteract) return;

        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsOpen)
            return;

        if (currentInteractable == null) return;

        canInteract = false; // require key release
        currentInteractable.Interact();
    }

    void Update()
    {
        // Wait for key release before allowing interaction again
        if (!canInteract && Input.GetKeyUp(KeyCode.E))
            canInteract = true;
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

    // Called by DialogueManager when closing dialogue
    public void ResetInteraction()
    {
        canInteract = false;
    }
}
