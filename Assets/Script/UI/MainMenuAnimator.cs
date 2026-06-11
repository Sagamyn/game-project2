using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuAnimator : MonoBehaviour
{
    [Header("Title Animation")]
    public RectTransform titleTransform;
    public float floatSpeed = 2f;
    public float floatHeight = 15f;
    private float originalY;

    [Header("Background Breathing")]
    public Image backgroundImage;
    public Color color1 = new Color(0.1f, 0.1f, 0.15f);
    public Color color2 = new Color(0.15f, 0.1f, 0.2f);
    public float colorSpeed = 1f;

    void Start()
    {
        if (titleTransform != null)
        {
            originalY = titleTransform.anchoredPosition.y;
        }

        if (backgroundImage != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    void Update()
    {
        if (titleTransform != null)
        {
            float newY = originalY + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            titleTransform.anchoredPosition = new Vector2(titleTransform.anchoredPosition.x, newY);
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = Color.Lerp(color1, color2, (Mathf.Sin(Time.time * colorSpeed) + 1f) / 2f);
        }
    }

    IEnumerator FadeIn()
    {
        float t = 0;
        Color original = color1;
        backgroundImage.color = Color.black;
        
        while (t < 1f)
        {
            t += Time.deltaTime * 0.8f;
            color1 = Color.Lerp(Color.black, original, t);
            yield return null;
        }
        color1 = original;
    }
}
