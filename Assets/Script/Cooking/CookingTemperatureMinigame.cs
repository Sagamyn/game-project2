using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CookingTemperatureMinigame : MonoBehaviour
{
    [Header("UI References")]
    public Slider temperatureSlider;   // Menampilkan suhu saat ini (0-100)
    public Slider progressSlider;      // Menampilkan progress masakan (0-100)
    public TextMeshProUGUI statusText; // Text: "Aman", "Api Besar!", "AWAS GOSONG!"
    public Image temperatureFill;      // Untuk mengubah warna bar suhu
    public UIAnimator uiAnimator;      // Menggunakan UIAnimator yang kamu punya

    [Header("Temperature Settings")]
    public float maxTemperature = 100f;
    public float temperatureDropRate = 15f; // Kecepatan suhu turun saat mouse diam
    public float mouseSensitivity = 50f;    // Seberapa sensitif gerakan mouse menambah suhu

    [Header("Zones (Risk vs Reward)")]
    public float coldZoneMax = 30f;      // < 30: Tidak masak
    public float safeZoneMax = 65f;      // 30-65: Api Kecil (Aman, Lambat)
    public float hotZoneMax = 90f;       // 65-90: Api Besar (Cepat)
    
    [Header("Progress Settings")]
    public float targetProgress = 100f;
    public float safeZoneSpeed = 5f;     // Progress per detik di Api Kecil
    public float hotZoneSpeed = 15f;     // Progress per detik di Api Besar
    public float overHeatTimeLimit = 2f; // Batas waktu sebelum gosong di atas 90 derajat

    [Header("Colors")]
    public Color coldColor = Color.cyan;
    public Color safeColor = Color.green;
    public Color hotColor = new Color(1f, 0.5f, 0f); // Orange
    public Color overheatColor = Color.red;

    private float currentTemp = 0f;
    private float currentProgress = 0f;
    private float overHeatTimer = 0f;
    private bool isPlaying = false;
    
    private CookingStation currentStation;

    // Dipanggil oleh CookingStation saat minigame dimulai
    public void StartMinigame(CookingStation station)
    {
        currentStation = station;
        currentTemp = 0f;
        currentProgress = 0f;
        overHeatTimer = 0f;
        isPlaying = true;

        if (temperatureSlider != null) temperatureSlider.maxValue = maxTemperature;
        if (progressSlider != null) progressSlider.maxValue = targetProgress;

        if (uiAnimator != null)
            uiAnimator.ShowInstant(); // Atau play animasi Show
        else
            gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isPlaying) return;

        HandleTemperatureInput();
        ProcessCooking();
        UpdateUI();
    }

    private void HandleTemperatureInput()
    {
        // Mendapatkan kecepatan gerakan mouse (Shakey-Wakey)
        float mouseMoveX = Mathf.Abs(Input.GetAxisRaw("Mouse X"));
        float mouseMoveY = Mathf.Abs(Input.GetAxisRaw("Mouse Y"));
        float mouseSpeed = mouseMoveX + mouseMoveY;

        // Tambah suhu berdasarkan gerakan mouse, kurangi perlahan jika diam
        if (mouseSpeed > 0.1f)
        {
            currentTemp += mouseSpeed * mouseSensitivity * Time.deltaTime;
        }
        else
        {
            currentTemp -= temperatureDropRate * Time.deltaTime;
        }

        currentTemp = Mathf.Clamp(currentTemp, 0, maxTemperature);
    }

    private void ProcessCooking()
    {
        if (currentTemp < coldZoneMax)
        {
            // Terlalu dingin, tidak ada progress
            overHeatTimer = 0f;
            statusText.text = "Terlalu Dingin!";
            temperatureFill.color = coldColor;
        }
        else if (currentTemp < safeZoneMax)
        {
            // Api Kecil (Aman)
            currentProgress += safeZoneSpeed * Time.deltaTime;
            overHeatTimer = 0f;
            statusText.text = "Api Kecil (Lambat & Aman)";
            temperatureFill.color = safeColor;
        }
        else if (currentTemp < hotZoneMax)
        {
            // Api Besar (Cepat)
            currentProgress += hotZoneSpeed * Time.deltaTime;
            overHeatTimer = 0f;
            statusText.text = "Api Besar! (Sangat Cepat)";
            temperatureFill.color = hotColor;
        }
        else
        {
            // OVERHEAT (Gosong Timer berjalan)
            overHeatTimer += Time.deltaTime;
            statusText.text = $"AWAS GOSONG! {(overHeatTimeLimit - overHeatTimer):F1}s";
            temperatureFill.color = overheatColor;

            if (overHeatTimer >= overHeatTimeLimit)
            {
                EndMinigame(false); // GAGAL (Gosong)
            }
        }

        // Cek jika masakan selesai (berhasil masuk ke fase plating)
        if (currentProgress >= targetProgress)
        {
            EndMinigame(true);
        }
    }

    private void UpdateUI()
    {
        if (temperatureSlider != null) temperatureSlider.value = currentTemp;
        if (progressSlider != null) progressSlider.value = currentProgress;
    }

    private void EndMinigame(bool success)
    {
        isPlaying = false;
        
        if (uiAnimator != null)
            uiAnimator.HideInstant();
        else
            gameObject.SetActive(false);

        if (success)
        {
            Debug.Log("Minigame Suhu Berhasil!");
        }
        else
        {
            Debug.LogWarning("Masakan GOSONG!");
        }

        // Kirim hasil ke CookingStation
        if (currentStation != null)
        {
            currentStation.OnMinigameComplete(success);
        }
    }
}