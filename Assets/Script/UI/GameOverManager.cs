using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;
    
    [Header("UI References")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI statsText;
    
    public bool IsGameOver { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            
            // Wire up buttons manually at runtime because editor script listeners don't persist
            Transform restartBtn = gameOverPanel.transform.Find("RestartButton");
            if (restartBtn != null && restartBtn.GetComponent<Button>() != null)
                restartBtn.GetComponent<Button>().onClick.AddListener(RestartRun);
                
            Transform quitBtn = gameOverPanel.transform.Find("QuitButton");
            if (quitBtn != null && quitBtn.GetComponent<Button>() != null)
                quitBtn.GetComponent<Button>().onClick.AddListener(QuitToMainMenu);
        }
    }

    void Start()
    {
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnPlayerDied += ShowGameOver;
        }
    }

    void OnDestroy()
    {
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnPlayerDied -= ShowGameOver;
        }
    }

    public void ShowGameOver()
    {
        ShowCustomGameOver("GAME OVER");
    }

    public void ShowCustomGameOver(string customTitle)
    {
        IsGameOver = true;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (titleText == null)
            {
                Transform t = gameOverPanel.transform.Find("GameOverTitle");
                if (t != null) titleText = t.GetComponent<TextMeshProUGUI>();
            }
        }

        if (titleText != null)
            titleText.text = customTitle;

        // Fetch stats if available
        int totalDays = FindObjectOfType<DayNightManager>() != null ? FindObjectOfType<DayNightManager>().currentDay : 0;
        int totalMoney = 0;
        
        FoodStallWaveManager waveMgr = FindObjectOfType<FoodStallWaveManager>();
        if (waveMgr != null)
        {
            // Bisa ambil money dari PlayerMoney juga
            PlayerMoney pm = FindObjectOfType<PlayerMoney>();
            if (pm != null) totalMoney = pm.CurrentMoney;
        }

        if (statsText != null)
        {
            statsText.text = $"Days Survived: {totalDays}\nFinal Money: ${totalMoney}";
        }

        // Pause the game
        Time.timeScale = 0f;
    }

    public void RestartRun()
    {
        Time.timeScale = 1f;
        // Load current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
