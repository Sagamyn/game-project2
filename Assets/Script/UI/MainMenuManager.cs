using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        // Load Test Scene (Asumsikan nama scene utama adalah "Test Scene")
        SceneManager.LoadScene("Test Scene");
    }
}
