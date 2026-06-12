using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerCoinUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI coinText;

    private PlayerMoney playerMoney;

    private int currentDisplayedAmount = 0;
    private int targetAmount = 0;
    private bool isInitialized = false;

    private Coroutine rollingCoroutine;
    private Coroutine bounceCoroutine;

    void Start()
    {
        playerMoney = FindObjectOfType<PlayerMoney>();
        
        if (playerMoney != null)
        {
            playerMoney.OnMoneyChanged += UpdateCoinDisplay;
            UpdateCoinDisplay(playerMoney.CurrentMoney);
        }
        else
        {
            Debug.LogWarning("[PlayerCoinUI] PlayerMoney not found in scene!");
        }
    }

    void OnDestroy()
    {
        if (playerMoney != null)
        {
            playerMoney.OnMoneyChanged -= UpdateCoinDisplay;
        }
    }

    private void UpdateCoinDisplay(int amount)
    {
        if (coinText == null) return;

        targetAmount = amount;

        if (isInitialized)
        {
            int diff = targetAmount - currentDisplayedAmount;
            if (diff != 0) // Muncul popup baik saat nambah maupun ngurang
            {
                SpawnPopupText(diff);
                StartBounceEffect();
            }
        }
        else
        {
            currentDisplayedAmount = amount;
            isInitialized = true;
        }

        if (rollingCoroutine != null) StopCoroutine(rollingCoroutine);
        rollingCoroutine = StartCoroutine(RollNumbers());
    }

    private void StartBounceEffect()
    {
        if (bounceCoroutine != null) StopCoroutine(bounceCoroutine);
        bounceCoroutine = StartCoroutine(BounceRoutine());
    }

    private IEnumerator BounceRoutine()
    {
        Vector3 startScale = Vector3.one;
        Vector3 targetScale = Vector3.one * 1.5f;
        float duration = 0.15f;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            coinText.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            coinText.transform.localScale = Vector3.Lerp(targetScale, startScale, t);
            yield return null;
        }
        
        coinText.transform.localScale = startScale;
    }

    private IEnumerator RollNumbers()
    {
        float duration = 0.5f;
        float t = 0;
        int startVal = currentDisplayedAmount;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            currentDisplayedAmount = Mathf.RoundToInt(Mathf.Lerp(startVal, targetAmount, t));
            coinText.text = currentDisplayedAmount.ToString("N0");
            yield return null;
        }

        currentDisplayedAmount = targetAmount;
        coinText.text = currentDisplayedAmount.ToString("N0");
    }

    private void SpawnPopupText(int diff)
    {
        GameObject popupObj = new GameObject("CoinPopup");
        popupObj.transform.SetParent(transform, false);
        
        RectTransform rt = popupObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.pivot = new Vector2(1, 0.5f);
        
        bool isPositive = diff > 0;
        // Kalau positif muncul dari atas, kalau negatif dari bawah dikit
        rt.anchoredPosition = new Vector2(-15f, isPositive ? 25f : -25f);

        TextMeshProUGUI tmp = popupObj.AddComponent<TextMeshProUGUI>();
        tmp.text = isPositive ? "+$" + diff : "-$" + Mathf.Abs(diff);
        tmp.fontSize = 24;
        tmp.color = isPositive ? Color.green : new Color(1f, 0.3f, 0.3f); // Merah terang
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.fontStyle = FontStyles.Bold;
        
        // Kasih outline tipis biar kebaca di background apapun
        Outline outline = popupObj.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.8f);
        outline.effectDistance = new Vector2(1, -1);
        
        Vector2 targetPos = rt.anchoredPosition + new Vector2(0f, isPositive ? 40f : -40f);
        StartCoroutine(PopupRoutine(tmp, rt, targetPos));
    }

    private IEnumerator PopupRoutine(TextMeshProUGUI tmp, RectTransform rt, Vector2 targetPos)
    {
        float duration = 1.0f;
        float t = 0f;
        Vector2 startPos = rt.anchoredPosition;

        Color startColor = tmp.color;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            
            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            tmp.color = c;
            
            yield return null;
        }

        Destroy(tmp.gameObject);
    }
}
