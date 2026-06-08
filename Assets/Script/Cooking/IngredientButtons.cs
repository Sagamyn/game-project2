using UnityEngine;
using UnityEngine.UI;

public class IngredientButton : MonoBehaviour
{
    public KebabIngredientData ingredient;
    public KebabAssembly kebabAssembly; // drag Ngompor di sini

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (kebabAssembly == null)
        {
            Debug.LogError("KebabAssembly belum di-assign!");
            return;
        }

        bool success = kebabAssembly.AddIngredient(ingredient);

        if (!success)
        {
            // nanti bisa tambahin feedback visual/audio di sini
            Debug.Log("Gagal tambah ingredient!");
        }
    }
}