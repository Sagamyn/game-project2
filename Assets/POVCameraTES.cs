using UnityEngine;

public class POVCameraTES : MonoBehaviour
{
    public GameObject interactText;

    [Header("Shop")]
    public ShopNPC shopNPC; // ambil shopNPC yang udh ada di scene, nanti di inspector tinggal drag and drop aja
    public GameObject povButtonCanvas; // canvas yang isinya tombol2

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

    // Kalau player keluar dari area, hide interact text dan close shop kalau kebuka
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

    // // Panggil button openShop di canvas
    public void OpenShopFromPOV()
    {
        if (shopNPC != null)
        {
            shopNPC.OpenShop();
        }
    }

    // Ketika tombol back dipencet nanti nutup toko dan switch camera ke main, jangan lupa unlock player movement
    public void ClosePOV()
    {
        if (shopNPC != null)
        {
            shopNPC.CloseShop();
            Debug.Log("Closed shop from POV");
        }

        CameraManager.Instance.SwitchToMain();
        PlayerMovement.Instance?.LockMovement(false);
        Debug.Log("Switched back to main camera from POV");

        if (povButtonCanvas != null)
        {
            povButtonCanvas.SetActive(false);
        }
    }
}