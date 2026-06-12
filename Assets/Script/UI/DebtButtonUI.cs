using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class DebtButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Merchant merchant;
    public bool isPayButton;

    private Vector3 originalScale;
    private Image btnImage;
    private Color originalColor;
    private Button btn;
    private RectTransform rt;

    private bool isHovered = false;

    void Awake()
    {
        originalScale = transform.localScale;
        btnImage = GetComponent<Image>();
        btn = GetComponent<Button>();
        rt = GetComponent<RectTransform>();
        if (btnImage != null)
            originalColor = btnImage.color;
    }

    void Update()
    {
        if (btn != null && !btn.interactable) return;

        // BRUTE FORCE MOUSE DETECTION (Bypass EventSystem)
        Vector2 mousePos = Input.mousePosition;
        bool mathematicallyHovered = RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos, null);

        if (mathematicallyHovered && !isHovered)
        {
            isHovered = true;
            StopAllCoroutines();
            StartCoroutine(ScaleTo(originalScale * 1.05f, 0.1f));
            if (btnImage != null) btnImage.color = originalColor * 1.2f;
        }
        else if (!mathematicallyHovered && isHovered)
        {
            isHovered = false;
            StopAllCoroutines();
            StartCoroutine(ScaleTo(originalScale, 0.1f));
            if (btnImage != null) btnImage.color = originalColor;
        }

        if (mathematicallyHovered && Input.GetMouseButtonDown(0))
        {
            ExecuteClick();
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {}
    public void OnPointerExit(PointerEventData eventData) {}
    
    public void OnPointerClick(PointerEventData eventData)
    {
        ExecuteClick();
    }

    private void ExecuteClick()
    {
        if (btn != null && !btn.interactable) return;

        StopAllCoroutines();
        transform.localScale = originalScale * 0.95f; // Click effect
        StartCoroutine(ScaleTo(originalScale, 0.1f));

        if (merchant != null)
        {
            if (isPayButton)
                merchant.OnPayClicked();
            else
                merchant.OnRefuseClicked();
        }
    }

    IEnumerator ScaleTo(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
            yield return null;
        }
        transform.localScale = targetScale;
    }
}
