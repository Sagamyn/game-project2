using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingText : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public float moveSpeed = 50f;
    public float duration = 1.5f;

    public void Setup(string text, Color color)
    {
        if (textMesh == null) textMesh = GetComponentInChildren<TextMeshProUGUI>();
        
        textMesh.text = text;
        textMesh.color = color;

        // Perkecil ukurannya karena canvas kita World Space
        transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);

        StartCoroutine(AnimateAndDestroy());
    }

    IEnumerator AnimateAndDestroy()
    {
        float timer = 0f;
        Color startColor = textMesh.color;
        float actualSpeed = 2f; // Kecepatan di World Space harus kecil

        while (timer < duration)
        {
            transform.position += Vector3.up * actualSpeed * Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, timer / duration);
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
