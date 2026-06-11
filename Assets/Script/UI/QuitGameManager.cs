using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class QuitGameManager : MonoBehaviour
{
    public static QuitGameManager Instance;

    [Header("UI References")]
    public GameObject warningPanel;
    public TextMeshProUGUI warningText;
    public Button yesBtn;
    public Button noBtn;

    private int warningStep = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (warningPanel != null) warningPanel.SetActive(false);
    }

    public void OnClickExitButton()
    {
        warningStep = 1;
        warningPanel.SetActive(true);
        warningPanel.transform.localScale = Vector3.one;
        warningText.text = "Yakin mau keluar?\nSemua Progress hari ini akan HANGUS!";
        
        yesBtn.onClick.RemoveAllListeners();
        yesBtn.onClick.AddListener(OnYesClicked);
        
        noBtn.onClick.RemoveAllListeners();
        noBtn.onClick.AddListener(OnNoClicked);
    }

    private void OnYesClicked()
    {
        if (warningStep == 1)
        {
            warningStep = 2;
            warningText.text = "BENERAN NIH?\nGa nyesel? Uang dan Kebabmu bakal lenyap ditelan Iblis loh!";
            warningPanel.transform.localScale = Vector3.one * 1.05f;
        }
        else if (warningStep == 2)
        {
            ExecuteResetAndQuit();
        }
    }

    public void OnNoClicked()
    {
        warningStep = 0;
        warningPanel.SetActive(false);
    }

    private void ExecuteResetAndQuit()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
    }
}
