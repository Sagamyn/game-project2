using UnityEngine;
using System.Collections;

/// <summary>
/// Handles camera transition between gameplay view and shop (food truck) view.
///
/// HOW IT WORKS:
///   - Saves the current camera position/settings when shop opens
///   - Smoothly moves camera to a fixed "shop view" position
///   - Disables CameraFollow while in shop view (so camera doesn't chase player)
///   - Restores everything when shop closes
///
/// SETUP IN UNITY:
///   1. Attach this script to the Main Camera GameObject
///   2. Create an empty GameObject → name it "ShopCameraPoint"
///      → position it in front of the food truck interior view you want
///   3. Assign shopCameraPoint in Inspector
///   4. Assign this component to ShelfShopManager.cameraController in Inspector
/// </summary>
public class ShopCameraController : MonoBehaviour
{
    // ─── References ──────────────────────────────────────────────
    [Header("References")]
    [Tooltip("Empty GameObject yang posisinya = posisi kamera waktu lihat food truck")]
    public Transform shopCameraPoint;

    [Tooltip("CameraFollow component di Main Camera — di-disable waktu shop view")]
    public CameraFollow cameraFollow;

    // ─── Transition Settings ─────────────────────────────────────
    [Header("Transition")]
    [Tooltip("Durasi transisi kamera pindah ke shop view (detik)")]
    public float transitionDuration = 0.6f;

    [Tooltip("Curve untuk easing transisi — EaseInOut paling enak")]
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // ─── Fade Settings ───────────────────────────────────────────
    [Header("Fade (Opsional)")]
    [Tooltip("Pakai fade hitam saat transisi kamera?")]
    public bool useFade = true;
    public TeleportFadeManager fadeManager; // pakai yang udah ada di project

    // ─── Orthographic Zoom (Opsional) ────────────────────────────
    [Header("Orthographic Size")]
    [Tooltip("Ubah zoom kamera waktu di shop view? (0 = pakai size yang sama)")]
    public float shopOrthoSize = 4f;
    [Tooltip("Size gameplay normal — auto-saved waktu OpenShopView dipanggil")]
    private float originalOrthoSize;

    // ─── Private State ───────────────────────────────────────────
    private Vector3 originalPosition;
    private bool isInShopView = false;
    private Coroutine transitionCoroutine;
    private Camera mainCam;

    // ─────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────

    void Awake()
    {
        mainCam = GetComponent<Camera>();

        // Auto-find CameraFollow kalau belum di-assign
        if (cameraFollow == null)
            cameraFollow = GetComponent<CameraFollow>();

        // Auto-find FadeManager kalau belum di-assign
        if (fadeManager == null)
            fadeManager = FindObjectOfType<TeleportFadeManager>();
    }

    // ─────────────────────────────────────────────────────────────
    // PUBLIC API — dipanggil oleh ShelfShopManager
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Pindahkan kamera ke shop view (food truck interior).
    /// Dipanggil dari ShelfShopManager.OpenShop()
    /// </summary>
    public void OpenShopView()
    {
        if (isInShopView) return;
        if (shopCameraPoint == null)
        {
            Debug.LogError("[ShopCameraController] shopCameraPoint belum di-assign!");
            return;
        }

        isInShopView = true;

        // Simpan state kamera saat ini
        originalPosition = transform.position;
        originalOrthoSize = mainCam != null ? mainCam.orthographicSize : 5f;

        // Stop transisi yang lagi jalan (kalau ada)
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(TransitionToShop());
    }

    /// <summary>
    /// Kembalikan kamera ke posisi gameplay.
    /// Dipanggil dari ShelfShopManager.CloseShop()
    /// </summary>
    public void CloseShopView()
    {
        if (!isInShopView) return;
        isInShopView = false;

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(TransitionToGameplay());
    }

    // ─────────────────────────────────────────────────────────────
    // TRANSITIONS
    // ─────────────────────────────────────────────────────────────

    IEnumerator TransitionToShop()
    {
        // ── Fade out ──
        if (useFade && fadeManager != null)
            yield return StartCoroutine(fadeManager.FadeOut(3f, Color.black));

        // ── Disable CameraFollow biar kamera nggak chase player ──
        if (cameraFollow != null)
            cameraFollow.enabled = false;

        // ── Langsung snap ke posisi shop (karena udah ketutup fade) ──
        Vector3 targetPos = new Vector3(
            shopCameraPoint.position.x,
            shopCameraPoint.position.y,
            transform.position.z  // pertahankan Z (depth)
        );
        transform.position = targetPos;

        // ── Ubah orthographic size kalau shopOrthoSize > 0 ──
        if (mainCam != null && shopOrthoSize > 0f)
            mainCam.orthographicSize = shopOrthoSize;

        // ── Fade in ke shop view ──
        if (useFade && fadeManager != null)
            yield return StartCoroutine(fadeManager.FadeIn(3f));
        else
        {
            // Tanpa fade: smooth lerp
            yield return StartCoroutine(SmoothMoveCamera(
                transform.position, targetPos,
                originalOrthoSize, shopOrthoSize
            ));
        }

        Debug.Log("[ShopCameraController] Now in shop view.");
    }

    IEnumerator TransitionToGameplay()
    {
        // ── Fade out ──
        if (useFade && fadeManager != null)
            yield return StartCoroutine(fadeManager.FadeOut(3f, Color.black));

        // ── Snap balik ke posisi gameplay ──
        transform.position = originalPosition;

        // ── Restore orthographic size ──
        if (mainCam != null)
            mainCam.orthographicSize = originalOrthoSize;

        // ── Re-enable CameraFollow ──
        if (cameraFollow != null)
            cameraFollow.enabled = true;

        // ── Fade in ke gameplay ──
        if (useFade && fadeManager != null)
            yield return StartCoroutine(fadeManager.FadeIn(3f));

        Debug.Log("[ShopCameraController] Back to gameplay view.");
    }

    /// <summary>
    /// Smooth lerp kamera (dipakai kalau useFade = false).
    /// </summary>
    IEnumerator SmoothMoveCamera(Vector3 fromPos, Vector3 toPos, float fromSize, float toSize)
    {
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = transitionCurve.Evaluate(Mathf.Clamp01(elapsed / transitionDuration));

            transform.position = Vector3.Lerp(fromPos, toPos, t);

            if (mainCam != null && shopOrthoSize > 0f)
                mainCam.orthographicSize = Mathf.Lerp(fromSize, toSize, t);

            yield return null;
        }

        transform.position = toPos;
        if (mainCam != null && shopOrthoSize > 0f)
            mainCam.orthographicSize = toSize;
    }

    // ─────────────────────────────────────────────────────────────
    // DEBUG
    // ─────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        if (shopCameraPoint == null) return;

        // Gambar posisi kamera shop di scene view
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(shopCameraPoint.position, 0.3f);
        Gizmos.DrawLine(transform.position, shopCameraPoint.position);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            shopCameraPoint.position + Vector3.up * 0.5f,
            "Shop Camera Point"
        );
#endif
    }
}