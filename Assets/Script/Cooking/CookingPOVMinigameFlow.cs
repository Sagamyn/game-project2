using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Orchestrator yang menghubungkan flow minigame di CookingPOV:
/// Ngompor diklik → Temperature Minigame → Plating QTE → Hasil ke Inventory
/// 
/// Attach ke GameObject Ngompor (atau sibling).
/// </summary>
public class CookingPOVMinigameFlow : MonoBehaviour
{
    [Header("References")]
    public KebabAssembly kebabAssembly;
    public CookingTemperatureMinigame temperatureMinigame;
    public CookingPlatingMinigame platingMinigame;
    public PlayerInventory playerInventory;

    [Header("Optional")]
    [Tooltip("Difficulty override. Jika null, pakai defaultDifficulty dari temperatureMinigame.")]
    public MinigameDifficulty difficultyOverride;

    // ── Raycast Blocker (dibuat otomatis saat runtime) ──
    private GameObject raycastBlocker;

    /// <summary>
    /// Dipanggil oleh tombol Ngompor (menggantikan ServeKebab).
    /// Memulai flow: validate → temperature minigame → plating → inventory.
    /// </summary>
    public void StartCookingFlow()
    {
        Debug.Log("[CookingPOVMinigameFlow] StartCookingFlow dipanggil!");

        // ── Validasi KebabAssembly ──
        if (kebabAssembly == null)
        {
            Debug.LogError("[CookingPOVMinigameFlow] KebabAssembly belum di-assign!");
            return;
        }

        if (!kebabAssembly.HasTortilla)
        {
            Debug.Log("[CookingPOVMinigameFlow] Tortilla belum ada! Taruh tortilla dulu.");
            return;
        }

        if (kebabAssembly.GetIngredients().Count < 2)
        {
            Debug.Log("[CookingPOVMinigameFlow] Minimal 2 ingredient!");
            return;
        }

        // ── Block input ke background ──
        ShowBlocker();

        // ── Mulai Temperature Minigame ──
        if (temperatureMinigame != null)
        {
            Debug.Log("[CookingPOVMinigameFlow] Memulai Temperature Minigame...");
            temperatureMinigame.StartMinigame(OnTemperatureComplete, difficultyOverride);
        }
        else
        {
            // Tidak ada temperature minigame → langsung ke plating
            Debug.LogWarning("[CookingPOVMinigameFlow] TemperatureMinigame null, skip ke Plating...");
            OnTemperatureComplete(true);
        }
    }

    /// <summary>
    /// Callback setelah Temperature Minigame selesai.
    /// </summary>
    private void OnTemperatureComplete(bool success)
    {
        Debug.Log($"[CookingPOVMinigameFlow] Temperature Minigame selesai: {(success ? "BERHASIL" : "GAGAL")}");

        if (!success)
        {
            // Gagal — bahan terbuang, clear kebab
            Debug.LogWarning("[CookingPOVMinigameFlow] Masakan GOSONG! Bahan terbuang!");
            HideBlocker();
            kebabAssembly.ClearKebab();
            return;
        }

        // Berhasil → lanjut ke Plating QTE
        if (platingMinigame != null)
        {
            Debug.Log("[CookingPOVMinigameFlow] Memulai Plating QTE...");
            platingMinigame.StartMinigame(OnPlatingComplete);
        }
        else
        {
            // Tidak ada plating minigame → langsung berhasil
            Debug.LogWarning("[CookingPOVMinigameFlow] PlatingMinigame null, skip...");
            OnPlatingComplete(true);
        }
    }

    /// <summary>
    /// Callback setelah Plating QTE selesai.
    /// </summary>
    private void OnPlatingComplete(bool success)
    {
        Debug.Log($"[CookingPOVMinigameFlow] Plating QTE selesai: {(success ? "BERHASIL" : "GAGAL")}");

        // ── Selalu unblock input setelah flow selesai ──
        HideBlocker();

        if (!success)
        {
            // Gagal — bahan terbuang, clear kebab
            Debug.LogWarning("[CookingPOVMinigameFlow] Plating BERANTAKAN! Bahan terbuang!");
            kebabAssembly.ClearKebab();
            return;
        }

        // ── Berhasil! Simpan result ke inventory ──
        // Simpan KebabResult untuk pencocokan dengan customer order nanti
        KebabResult result = new KebabResult();
        foreach (var ing in kebabAssembly.GetIngredients())
            result.AddIngredient(ing);

        KebabAssembly.LastResult = result;

        // Masukkan ke inventory
        if (playerInventory == null)
        {
            // Fallback: coba ambil dari kebabAssembly
            playerInventory = kebabAssembly.playerInventory;
        }

        if (playerInventory != null && kebabAssembly.kebabResult != null)
        {
            bool added = playerInventory.AddItem(kebabAssembly.kebabResult, 1);

            if (added)
            {
                Debug.Log("[CookingPOVMinigameFlow] ✓ Kebab masuk inventory!");

                // Log inventory contents
                foreach (var slot in playerInventory.items)
                {
                    if (slot.item != null)
                        Debug.Log($"  Inventory: {slot.item.itemName} x{slot.amount}");
                }
            }
            else
            {
                Debug.LogWarning("[CookingPOVMinigameFlow] Inventory penuh! Kebab hilang!");
            }
        }
        else
        {
            Debug.LogError("[CookingPOVMinigameFlow] PlayerInventory atau kebabResult null!");
        }

        // Clear kebab assembly untuk order selanjutnya
        kebabAssembly.ClearKebab();

        Debug.Log("[CookingPOVMinigameFlow] ✓ Flow selesai! Kebab siap diberikan ke customer.");
    }

    // ═══════════════════════════════════════════════════
    //  RAYCAST BLOCKER — mencegah klik ke background
    // ═══════════════════════════════════════════════════

    /// <summary>
    /// Membuat panel transparan fullscreen yang memblokir semua klik
    /// ke elemen di belakang minigame (bahan-bahan, kompor, dll).
    /// Panel ini ditempatkan tepat di BELAKANG minigame panels.
    /// </summary>
    private void ShowBlocker()
    {
        if (raycastBlocker != null)
        {
            raycastBlocker.SetActive(true);
            return;
        }

        // Cari Canvas parent (CookingPOV > Canvas)
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogWarning("[CookingPOVMinigameFlow] Canvas parent tidak ditemukan, blocker tidak dibuat.");
            return;
        }

        // Buat blocker GameObject
        raycastBlocker = new GameObject("MinigameRaycastBlocker");
        raycastBlocker.transform.SetParent(parentCanvas.transform, false);

        // RectTransform — fullscreen stretch
        RectTransform rt = raycastBlocker.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Image transparan tapi raycastTarget = true → memblokir klik
        Image blockerImage = raycastBlocker.AddComponent<Image>();
        blockerImage.color = new Color(0f, 0f, 0f, 0f); // 100% transparan
        blockerImage.raycastTarget = true;

        // Posisikan di BELAKANG minigame panels tapi DI DEPAN elemen cooking
        // Caranya: letakkan sebagai child terakhir-1 (sebelum minigame panels)
        // Atau cukup SetAsLastSibling lalu pindah ke belakang minigame panels
        
        // Strategi: cari index CookingMinigamePanel, taruh blocker tepat sebelumnya
        int blockerIndex = parentCanvas.transform.childCount - 1; // default: paling atas

        if (temperatureMinigame != null)
        {
            int tempIndex = temperatureMinigame.transform.GetSiblingIndex();
            blockerIndex = Mathf.Min(blockerIndex, tempIndex);
        }
        if (platingMinigame != null)
        {
            int platIndex = platingMinigame.transform.GetSiblingIndex();
            blockerIndex = Mathf.Min(blockerIndex, platIndex);
        }

        raycastBlocker.transform.SetSiblingIndex(blockerIndex);

        Debug.Log("[CookingPOVMinigameFlow] ✓ Raycast blocker aktif — background tidak bisa diklik.");
    }

    private void HideBlocker()
    {
        if (raycastBlocker != null)
        {
            raycastBlocker.SetActive(false);
            Debug.Log("[CookingPOVMinigameFlow] Raycast blocker dinonaktifkan.");
        }
    }

    private void OnDestroy()
    {
        if (raycastBlocker != null)
            Destroy(raycastBlocker);
    }
}
