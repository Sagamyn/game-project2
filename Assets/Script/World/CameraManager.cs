using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [Header("Cameras")]
    public Camera mainCamera;
    public Camera cookingCamera;
    public Camera cookingPOVCamera;

    [Header("UI")]
    public GameObject hotbarUI; // Assign your hotbar here

    [Header("Transition")]
    public Image fadeImage;
    public float fadeDuration = 0.5f;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SwitchInstant(mainCamera);

        // Make sure hotbar is visible at start
        if (hotbarUI != null)
            hotbarUI.SetActive(true);
    }

    public void SwitchToMain()
    {
        StartCoroutine(SwitchCamera(mainCamera));
    }

    public void SwitchToCooking()
    {
        StartCoroutine(SwitchCamera(cookingCamera));
    }

    public void SwitchToPOV()
    {
        StartCoroutine(SwitchCamera(cookingPOVCamera));
    }

    IEnumerator SwitchCamera(Camera targetCam)
    {
        // Fade Out
        yield return StartCoroutine(Fade(1));

        // Disable all cameras
        mainCamera.gameObject.SetActive(false);
        cookingCamera.gameObject.SetActive(false);
        cookingPOVCamera.gameObject.SetActive(false);

        // Enable target camera
        targetCam.gameObject.SetActive(true);

        // Show hotbar ONLY in main camera
        if (hotbarUI != null)
        {
            hotbarUI.SetActive(targetCam == mainCamera);
        }

        // Fade In
        yield return StartCoroutine(Fade(0));
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float time = 0;

        Color color = fadeImage.color;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            color.a = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            fadeImage.color = color;

            yield return null;
        }

        color.a = targetAlpha;
        fadeImage.color = color;
    }

    void SwitchInstant(Camera cam)
    {
        mainCamera.gameObject.SetActive(false);
        cookingCamera.gameObject.SetActive(false);
        cookingPOVCamera.gameObject.SetActive(false);

        cam.gameObject.SetActive(true);

        // Show hotbar only if using main camera
        if (hotbarUI != null)
        {
            hotbarUI.SetActive(cam == mainCamera);
        }
    }
}