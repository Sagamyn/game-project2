using UnityEngine;

public class FoodStallPOV : MonoBehaviour
{
    [Header("Interaction")]
    public GameObject interactText;

    [Header("Wave Manager")]
    public FoodStallWaveManager waveManager;

    [Header("Buttons Canvas")]
    public GameObject povButtonCanvas; // Canvas with Back button etc

    private bool playerNear = false;

    void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            EnterPOV();
        }
    }

void EnterPOV()
{
    PlayerMovement.Instance?.LockMovement(true);
    CameraManager.Instance.SwitchToCooking();

    if (povButtonCanvas != null)
        povButtonCanvas.SetActive(true);

    if (interactText != null)
        interactText.SetActive(false);


    Debug.Log("✓ Entered Food Stall POV");
}

    // Called by the Back button in the POV canvas
    public void ExitPOV()
    {
        // Stop waves
        if (waveManager != null)
            waveManager.StopWaves();

        // Switch back to main camera
        CameraManager.Instance.SwitchToMain();

        // Unlock player
        PlayerMovement.Instance?.LockMovement(false);

        // Hide buttons
        if (povButtonCanvas != null)
            povButtonCanvas.SetActive(false);

        Debug.Log("Exited Food Stall POV");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            if (interactText != null)
                interactText.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            if (interactText != null)
                interactText.SetActive(false);
        }
    }
}