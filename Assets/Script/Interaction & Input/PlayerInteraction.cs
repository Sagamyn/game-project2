using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    Interactable currentInteractable;

    public bool HasInteractable => currentInteractable != null;

    void Update()
    {
        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsOpen)
            return;

        if (currentInteractable == null)
            return;

        if (Input.GetKeyDown(KeyCode.E))
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
