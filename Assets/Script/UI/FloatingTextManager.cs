using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance;

    public GameObject floatingTextPrefab;
    public Transform textContainer; // Pindahkan ke parent Canvas agar rapi

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowText(string text, Vector3 spawnPosition, Color color)
    {
        if (floatingTextPrefab == null) return;

        // Bikin objectnya di Canvas posisi World
        GameObject txtObj = Instantiate(floatingTextPrefab, spawnPosition, Quaternion.identity, textContainer ?? transform);
        txtObj.SetActive(true); // Pastikan aktif karena prefab-nya disabled
        FloatingText ft = txtObj.GetComponent<FloatingText>();
        if (ft != null)
        {
            ft.Setup(text, color);
        }
    }
}
