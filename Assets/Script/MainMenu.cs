using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public AudioClip buttonClickSound;

    public void PlayGame()
    {
        AudioSource.PlayClipAtPoint(buttonClickSound, Camera.main.transform.position);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {;
        AudioSource.PlayClipAtPoint(buttonClickSound, Camera.main.transform.position);
        Application.Quit();
    }
}
