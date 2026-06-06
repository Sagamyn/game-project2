using UnityEngine;
using System.Collections;

public class SummaryPanelAnimator : MonoBehaviour
{
    [Header("Panel")]
    public RectTransform panel;

    [Header("Positions")]
    public Vector2 hiddenPosition;
    public Vector2 shownPosition;

    [Header("Animation")]
    public float slideDuration = 0.5f;

    void Awake()
    {
        if (panel != null)
        {
            panel.anchoredPosition = hiddenPosition;
        }
    }

    public void ShowPanel()
    {
        StopAllCoroutines();
        StartCoroutine(SlidePanel(shownPosition));
    }

    public void HidePanel()
    {
        StopAllCoroutines();
        StartCoroutine(SlidePanel(hiddenPosition));
    }

    IEnumerator SlidePanel(Vector2 target)
    {
        Vector2 startPos = panel.anchoredPosition;

        float time = 0;

        while (time < slideDuration)
        {
            time += Time.deltaTime;

            panel.anchoredPosition =
                Vector2.Lerp(
                    startPos,
                    target,
                    time / slideDuration
                );

            yield return null;
        }

        panel.anchoredPosition = target;
    }
}