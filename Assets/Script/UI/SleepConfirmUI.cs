using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// The Yes/No panel that appears when player
/// tries to sleep at the tent.
/// Assign your pixel art buttons in Inspector.
/// </summary>
public class SleepConfirmUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;
    public UIAnimator uiAnimator;   // optional, for slide in animation

    [Header("Text")]
    public TextMeshProUGUI titleText;        // "Go to sleep?"
    public TextMeshProUGUI subtitleText;     // "Tomorrow is a new day..."

    [Header("Buttons")]
    public Button yesButton;
    public Button noButton;

    // Callbacks
    private System.Action onYes;
    private System.Action onNo;

    void Awake()
    {
        // Hide at start
        if (uiAnimator != null)
            uiAnimator.HideInstant();
        else if (panel != null)
            panel.SetActive(false);

        if (yesButton != null)
            yesButton.onClick.AddListener(OnYesClicked);

        if (noButton != null)
            noButton.onClick.AddListener(OnNoClicked);
    }

    public void Show(System.Action yesCallback, System.Action noCallback)
    {
        onYes = yesCallback;
        onNo  = noCallback;

        if (uiAnimator != null)
        {
            panel.SetActive(true);
            uiAnimator.Show();
        }
        else if (panel != null)
        {
            panel.SetActive(true);
        }

        // Lock player while panel is open
        PlayerMovement.Instance?.LockMovement(true);
    }

    public void Hide()
    {
        if (!gameObject.activeInHierarchy || (panel != null && !panel.activeInHierarchy)) 
            return;

        if (uiAnimator != null)
            uiAnimator.Hide();
        else if (panel != null)
            panel.SetActive(false);

        PlayerMovement.Instance?.LockMovement(false);
    }

    void OnYesClicked()
    {
        Hide();
        onYes?.Invoke();
    }

    void OnNoClicked()
    {
        Hide();
        onNo?.Invoke();
    }
}