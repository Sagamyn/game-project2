using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{

    public GameObject menuCanvas;
    // Start is called before the first frame update
    void Start()
    {
        menuCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I))
        {
            if(menuCanvas.activeSelf)
            {
                menuCanvas.SetActive(false);
                Time.timeScale = 1f; // Resume the game
            }
            else
            {
                menuCanvas.SetActive(true);
                Time.timeScale = 0f; // Pause the game
            }
        }
    }
}
