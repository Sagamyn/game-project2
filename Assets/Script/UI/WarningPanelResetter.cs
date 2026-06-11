using UnityEngine;

public class WarningPanelResetter : MonoBehaviour
{
    void OnDisable()
    {
        if (QuitGameManager.Instance != null)
        {
            QuitGameManager.Instance.OnNoClicked();
        }
    }
}
