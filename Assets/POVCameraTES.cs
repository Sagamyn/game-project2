using UnityEngine;

public class POVCameraTES : MonoBehaviour
{
    public GameObject interactText;

    private bool playerNear;

    void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            CameraManager.Instance.SwitchToCooking();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;

            if (interactText != null)
                interactText.SetActive(true);

            Debug.Log("Player entered cooking station");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;

            if (interactText != null)
                interactText.SetActive(false);

            Debug.Log("Player exited cooking station");
        }
    }
}