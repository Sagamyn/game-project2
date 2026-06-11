using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;

public class TutorialDialogueManager : MonoBehaviour
{
    public static TutorialDialogueManager Instance;

    [Header("UI References")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI dialogueText;
    public AudioSource audioSource;
    public AudioClip[] talkSounds;
    public float typingSpeed = 0.04f;

    [Header("State Tracking")]
    public int currentStep = 0;
    private Coroutine typeCoroutine;
    private bool isTyping = false;

    // References
    private FoodStallWaveManager waveMgr;
    private KebabAssembly kebabAssembly;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
            
            // Matikan blokade klik (Raycast Target) agar tidak mengganggu klik spatula/ticket
            Image panelImg = tutorialPanel.GetComponent<Image>();
            if (panelImg != null) panelImg.raycastTarget = false;
            
            // Matikan juga buat textnya jika perlu
            if (dialogueText != null) dialogueText.raycastTarget = false;
        }
    }

    void Start()
    {
        waveMgr = FindObjectOfType<FoodStallWaveManager>();
        kebabAssembly = FindObjectOfType<KebabAssembly>();

        // Check if we are on day 0
        DayNightManager dnm = FindObjectOfType<DayNightManager>();
        if (dnm != null && dnm.currentDay == 0)
        {
            StartTutorial();
        }
        else if (dnm != null && dnm.currentDay > 0)
        {
            // Skip tutorial
            Destroy(gameObject);
        }
        else if (dnm == null)
        {
            // Fallback for testing without DayNightManager
            StartTutorial();
        }
    }

    public void StartTutorial()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        currentStep = 1;
        ShowDialogue("Hai chief, kamu pasti tidak penasaran kenapa kamu dipanggil kesini.. Cobalah Untuk mendekati mobil van disebelahmu itu..");
    }

    void Update()
    {
        if (currentStep == 0) return; // Tutorial tidak aktif

        switch (currentStep)
        {
            case 1: // Wait for entering CookingStationPOV
                if (CameraManager.Instance != null && CameraManager.Instance.cookingCamera != null && CameraManager.Instance.cookingCamera.gameObject.activeInHierarchy)
                {
                    AdvanceStep(2, "Bagus! Hari ini adalah hari pertamamu. Mari kita buka warung dengan mengklik Spatula di sebelah kiri!");
                }
                break;

            case 2: // Wait for Spatula click (IsRunning)
                if (waveMgr != null && waveMgr.IsRunning)
                {
                    // Pemain sudah klik spatula, sekarang tunggu sampai customernya benar-benar spawn
                    currentStep = 3; 
                }
                break;
                
            case 3: // Wait for customer to spawn
                if (waveMgr != null && waveMgr.activeCustomer != null)
                {
                    AdvanceStep(4, "Wah ada pelanggan datang! Dia memesan kebab. Cepat klik area Kompor untuk mulai meracik!");
                }
                break;
            
            case 4: // Wait for Cooking POV
                if (kebabAssembly != null && kebabAssembly.gameObject.activeInHierarchy)
                {
                    AdvanceStep(5, "Pertama-tama, siapkan kulitnya. Klik ikon Tortilla di daftar bahan!");
                }
                break;

            case 5: // Wait for Tortilla
                if (kebabAssembly != null && kebabAssembly.HasTortilla)
                {
                    AdvanceStep(6, "Bagus! Sekarang klik daging dan sayur untuk memasukkannya sesuai pesanan.");
                }
                break;

            case 6: // Wait for Ingredients
                if (kebabAssembly != null && kebabAssembly.GetIngredients().Count >= 2)
                {
                    AdvanceStep(7, "Lengkap! Sekarang klik Wajan (Ngompor) untuk memanggang dagingnya dan ikuti Minigame-nya!");
                }
                break;

            case 7: // Wait for Cooking Result (Kebab in inventory)
                PlayerInventory inv = FindObjectOfType<PlayerInventory>();
                if (inv != null && inv.HasItem(waveMgr.kebabResult))
                {
                    AdvanceStep(8, "Kebabnya berhasil dibuat! Klik ikon 'Kembali' untuk menemui pelanggan di depan.");
                }
                break;

            case 8: // Wait for Station POV
                if (kebabAssembly != null && !kebabAssembly.gameObject.activeInHierarchy)
                {
                    AdvanceStep(9, "Terakhir, berikan Kebab ini kepada pelanggan dengan menekan Ticket Order (Buku Pesanan) di sebelah kanan kompor!");
                }
                break;

            case 9: // Wait for Customer to leave
                if (waveMgr != null && waveMgr.activeCustomer == null)
                {
                    PlayerInventory inv7 = FindObjectOfType<PlayerInventory>();
                    if (inv7 == null || !inv7.HasItem(waveMgr.kebabResult))
                    {
                        currentStep = 10;
                        StartCoroutine(PlayEndingMonologue());
                    }
                }
                break;
            
            case 10:
                // Sedang baca monolog jahat. Nunggu coroutine jalan ke Step 11
                break;

            case 11: // Wait for day end (CanSleep becomes true)
                if (FoodStallWaveManager.CanSleep)
                {
                    if (tutorialPanel != null) tutorialPanel.SetActive(true); // Tampilkan lagi
                    AdvanceStep(12, "Keluarlah dari Van.");
                }
                break;

            case 12: // Wait for player to exit Van (CookingCamera becomes inactive)
                if (CameraManager.Instance != null && CameraManager.Instance.cookingCamera != null && !CameraManager.Instance.cookingCamera.gameObject.activeInHierarchy)
                {
                    AdvanceStep(13, "Pergi menuju Tempat peristirahatan dibawah, Tidurlah....");
                }
                break;

            case 13: // Wait for player to actually sleep (CanSleep becomes false again)
                // Selesai. Panel disembunyikan saat tidur.
                if (!FoodStallWaveManager.CanSleep)
                {
                    currentStep = 0;
                    if (tutorialPanel != null) tutorialPanel.SetActive(false);
                }
                break;
        }
    }

    private IEnumerator PlayEndingMonologue()
    {
        ShowDialogue("Kerja bagus! Latihan selesai.");
        yield return new WaitForSeconds(4f); // Tunggu sampai selesai ngetik + baca
        
        if (currentStep != 10) yield break; // Safety check
        ShowDialogue("Tapi ingat... Kau harus menyetorkan uang kepadaku jika kau ingin hidup!");
        yield return new WaitForSeconds(5f);

        if (currentStep != 10) yield break;
        ShowDialogue("Carilah uang sebanyak-banyaknya dan berusahalah untuk bertahan hidup ahhahahaha!");
        yield return new WaitForSeconds(6f);

        if (currentStep == 10)
        {
            currentStep = 11; // Pindah ke fase nunggu tidur
            dialogueText.text = ""; // Kosongkan layar sambil nunggu
            if (tutorialPanel != null) tutorialPanel.SetActive(false); // Sembunyikan panelnya biar bersih layarnya
        }
    }

    private void AdvanceStep(int nextStep, string text)
    {
        currentStep = nextStep;
        ShowDialogue(text);
    }

    private void ShowDialogue(string text)
    {
        if (typeCoroutine != null) StopCoroutine(typeCoroutine);
        typeCoroutine = StartCoroutine(TypeText(text));
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        
        int charCount = 0;
        foreach (char c in text)
        {
            dialogueText.text += c;
            charCount++;

            // Play sound sync with typing
            if (audioSource != null && talkSounds != null && talkSounds.Length > 0)
            {
                // Mainkan suara tiap 2 huruf sekali biar gak tumpang tindih berisik
                if (charCount % 2 == 0 && c != ' ') 
                {
                    audioSource.pitch = Random.Range(0.95f, 1.05f);
                    audioSource.PlayOneShot(talkSounds[Random.Range(0, talkSounds.Length)]);
                }
            }

            // Dramatic Delay System
            if (c == '.' || c == '!' || c == '?')
            {
                yield return new WaitForSeconds(typingSpeed * 8f); // Jeda panjang pas tanda seru/titik
            }
            else if (c == ',')
            {
                yield return new WaitForSeconds(typingSpeed * 4f); // Jeda medium pas koma
            }
            else
            {
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        isTyping = false;
    }
}
