#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ClearSaveDataEditor
{
    [UnityEditor.MenuItem("Tools/Save/Clear All Save Data (Reset Tutorial)")]
    public static void ClearSaveData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("✔️ All PlayerPrefs and Save Data have been cleared! Next play will start from WAVE - 0.");
    }
}
#endif
