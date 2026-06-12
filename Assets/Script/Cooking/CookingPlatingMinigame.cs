using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CookingPlatingMinigame : MonoBehaviour
{
    [Header("UI References")]
    public Image ringBackground;
    public Image successZoneArc;
    public RectTransform needleTransform;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI promptText;
    public UIAnimator uiAnimator;

    [Header("QTE Settings")]
    [Range(60f, 720f)]
    public float needleSpeed = 270f;

    [Header("Success Zone (Degrees)")]
    [Range(0f, 360f)]
    public float zoneCenterAngle = 90f;

    [Range(10f, 120f)]
    public float zoneWidthAngle = 40f;

    [Header("Timing")]
    public float resultDisplayDuration = 1.5f;

    [Header("Colors")]
    public Color zoneColor = new Color(0.2f, 0.85f, 0.3f, 0.6f);
    public Color successColor = new Color(0.2f, 0.9f, 0.3f);
    public Color failColor = new Color(0.95f, 0.2f, 0.2f);

    private bool isPlaying = false;
    private float currentAngle = 0f;
    private float zoneLowAngle;
    private float zoneHighAngle;
    private CookingStation currentStation;
    private System.Action<bool> onCompleteCallback;

    /// <summary>
    /// Overload untuk dipanggil dari CookingPOVMinigameFlow (tanpa CookingStation).
    /// </summary>
    public void StartMinigame(System.Action<bool> onComplete)
    {
        onCompleteCallback = onComplete;
        currentStation = null;
        StartMinigameInternal();
    }

    public void StartMinigame(CookingStation station)
    {
        onCompleteCallback = null;
        currentStation = station;
        StartMinigameInternal();
    }

    private void StartMinigameInternal()
    {
        currentAngle = 0f;
        isPlaying = true;

        zoneCenterAngle = Random.Range(0f, 360f);

        RefreshZoneBounds();
        ApplySuccessZoneVisual();

        if (needleTransform != null)
            needleTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        if (resultText != null)
        {
            resultText.text = "";
            resultText.gameObject.SetActive(false);
        }

        if (promptText != null)
        {
            promptText.text = "Space";
            promptText.gameObject.SetActive(true);
        }

        if (uiAnimator != null)
            uiAnimator.ShowInstant();
        else
            gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isPlaying) return;

        currentAngle += needleSpeed * Time.deltaTime;
        if (currentAngle >= 360f)
            currentAngle -= 360f;

        if (needleTransform != null)
            needleTransform.localRotation = Quaternion.Euler(0f, 0f, -currentAngle);

        if (Input.GetKeyDown(KeyCode.Space))
            EvaluatePress();
    }

    void OnValidate()
    {
        RefreshZoneBounds();
        ApplySuccessZoneVisual();
    }

    private void RefreshZoneBounds()
    {
        float halfWidth = zoneWidthAngle * 0.5f;
        zoneLowAngle = zoneCenterAngle - halfWidth;
        zoneHighAngle = zoneCenterAngle + halfWidth;

        if (zoneLowAngle < 0f) zoneLowAngle += 360f;
        if (zoneHighAngle >= 360f) zoneHighAngle -= 360f;
    }

    private void ApplySuccessZoneVisual()
    {
        if (successZoneArc == null) return;

        successZoneArc.type = Image.Type.Filled;
        successZoneArc.fillMethod = Image.FillMethod.Radial360;
        successZoneArc.fillOrigin = (int)Image.Origin360.Top;
        successZoneArc.fillClockwise = true;
        successZoneArc.fillAmount = zoneWidthAngle / 360f;
        successZoneArc.color = zoneColor;

        float rotZ = -(zoneCenterAngle - zoneWidthAngle * 0.5f);
        successZoneArc.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotZ);
    }

    private bool IsAngleInZone(float angle)
    {
        if (zoneLowAngle <= zoneHighAngle)
            return angle >= zoneLowAngle && angle <= zoneHighAngle;
        else
            return angle >= zoneLowAngle || angle <= zoneHighAngle;
    }

    private void EvaluatePress()
    {
        isPlaying = false;

        bool success = IsAngleInZone(currentAngle);

        if (promptText != null)
            promptText.gameObject.SetActive(false);

        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);

            if (success)
            {
                resultText.text = "Well Done!";
                resultText.color = successColor;
            }
            else
            {
                resultText.text = "Failure!";
                resultText.color = failColor;
            }
        }

        StartCoroutine(DelayedEnd(success));
    }

    private IEnumerator DelayedEnd(bool success)
    {
        yield return new WaitForSeconds(resultDisplayDuration);

        if (uiAnimator != null)
            uiAnimator.HideInstant();
        else
            gameObject.SetActive(false);

        // Callback mode (dari CookingPOVMinigameFlow)
        if (onCompleteCallback != null)
        {
            var cb = onCompleteCallback;
            onCompleteCallback = null;
            currentStation = null;
            cb.Invoke(success);
            yield break;
        }

        // Station mode (dari CookingStation)
        if (currentStation != null)
            currentStation.OnPlatingComplete(success);
    }
}
