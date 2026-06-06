using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ServeButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public Outline outline;

    void Start()
    {
        if (outline == null)
            outline = GetComponent<Outline>();

        if (outline != null)
            outline.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (outline != null)
            outline.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (outline != null)
            outline.enabled = false;
    }
}