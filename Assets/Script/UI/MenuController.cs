using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{

    public GameObject menuCanvas;

    [Header("References for ESC Guard")]
    public MerchantShop merchantShop;

    // Start is called before the first frame update
    void Start()
    {
        menuCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        bool tabOrI = Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I);
        bool esc = Input.GetKeyDown(KeyCode.Escape);

        if (tabOrI || esc)
        {
            // Kalau ESC ditekan tapi ada UI lain yang sedang menangani ESC sendiri,
            // jangan toggle menu
            if (esc || tabOrI && IsOtherEscHandlerActive())
                return;

            if (menuCanvas.activeSelf)
            {
                menuCanvas.SetActive(false);
                Time.timeScale = 1f;
            }
            else
            {
                // Jangan buka menu pakai ESC kalau lagi di tengah UI lain
                if (esc) return;

                menuCanvas.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }

    bool IsOtherEscHandlerActive()
    {
        // Merchant shop sedang ada card yang dipilih
        if (merchantShop != null && merchantShop.IsCardSelected())
            return true;

        return false;
    }
}
