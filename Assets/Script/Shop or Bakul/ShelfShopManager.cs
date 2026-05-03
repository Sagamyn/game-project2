using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the shelf shop that appears during break phase.
/// Handles:
///   - Fixed items  → always appear on specific slots
///   - Random items → picked from a pool, fill remaining slots
///   - Open/Close   → call OpenShop() when break phase starts
///
/// SETUP IN UNITY:
///   1. Attach this script to a GameObject called "ShelfShopManager"
///   2. Create empty GameObjects as slot positions on each shelf
///      (position them where items should appear on the shelf sprite)
///   3. Assign slot Transforms to shelfSlots list in Inspector
///   4. Create ShopItem ScriptableObjects for fixed and random pool items
///   5. Create a prefab with SpriteRenderer + Collider2D + ShopItemWorld → assign to itemPrefab
/// </summary>
public class ShelfShopManager : MonoBehaviour
{
    // ─── References ──────────────────────────────────────────────
    [Header("Shelf Slots")]
    [Tooltip("Taruh Transform kosong di posisi tiap slot rak (urut dari kiri ke kanan, rak atas dulu)")]
    public List<Transform> shelfSlots = new List<Transform>(); // total 8-10 slot (2 rak × 4-5 item)

    [Header("Item Prefab")]
    [Tooltip("Prefab dengan SpriteRenderer + Collider2D + ShopItemWorld")]
    public GameObject itemPrefab;

    // ─── Fixed Items ──────────────────────────────────────────────
    [Header("Fixed Items")]
    [Tooltip("Item yang selalu muncul. Index = index slot di shelfSlots.")]
    public List<FixedShopSlot> fixedItems = new List<FixedShopSlot>();

    // ─── Random Pool ──────────────────────────────────────────────
    [Header("Random Item Pool")]
    [Tooltip("Pool item yang akan di-random untuk mengisi slot kosong")]
    public List<ShopItem> randomItemPool = new List<ShopItem>();

    [Tooltip("Berapa item random yang muncul tiap break phase")]
    public int randomItemCount = 3;

    [Tooltip("Seed random sama tiap wave (false = beda tiap buka)")]
    public bool useFixedSeed = false;
    public int randomSeed = 42;

    // ─── Shop Root ───────────────────────────────────────────────
    [Header("Shop Root")]
    [Tooltip("Parent GameObject seluruh shop (rak + background). Di-hide waktu shop tutup.")]
    public GameObject shopRoot;

    // ─── Camera ──────────────────────────────────────────────────
    [Header("Camera")]
    [Tooltip("ShopCameraController di Main Camera — handle transisi ke shop view")]
    public ShopCameraController cameraController;

    // ─── Audio ───────────────────────────────────────────────────
    [Header("Audio")]
    public AudioClip openShopSound;
    public AudioClip closeShopSound;

    // ─── Private State ───────────────────────────────────────────
    private List<ShopItemWorld> spawnedItems = new List<ShopItemWorld>();
    private bool isOpen = false;

    // ─────────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────

    void Awake()
    {
        // Pastiin shop tutup saat game mulai
        if (shopRoot != null)
            shopRoot.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────
    // OPEN / CLOSE
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Panggil ini waktu break phase mulai.
    /// Bisa dipanggil dari WaveManager: ShelfShopManager.Instance.OpenShop()
    /// </summary>
    public void OpenShop()
    {
        if (isOpen) return;
        isOpen = true;

        // Pindah kamera ke shop view (food truck interior)
        cameraController?.OpenShopView();

        // Tampilkan shop
        if (shopRoot != null)
            shopRoot.SetActive(true);

        // Lock player movement
        PlayerMovement.Instance?.LockMovement(true);

        // Spawn items ke rak
        SpawnItems();

        if (openShopSound != null)
            AudioSource.PlayClipAtPoint(openShopSound, Camera.main.transform.position);

        Debug.Log("[ShelfShopManager] Shop opened!");
    }

    /// <summary>
    /// Panggil ini waktu break phase selesai / player klik tombol lanjut.
    /// </summary>
    public void CloseShop()
    {
        if (!isOpen) return;
        isOpen = false;

        // Kembalikan kamera ke gameplay
        cameraController?.CloseShopView();

        // Sembunyikan shop
        if (shopRoot != null)
            shopRoot.SetActive(false);

        // Bersihkan item yang di-spawn
        ClearItems();

        // Unlock player movement
        PlayerMovement.Instance?.LockMovement(false);

        if (closeShopSound != null)
            AudioSource.PlayClipAtPoint(closeShopSound, Camera.main.transform.position);

        Debug.Log("[ShelfShopManager] Shop closed!");
    }

    // ─────────────────────────────────────────────────────────────
    // ITEM SPAWNING
    // ─────────────────────────────────────────────────────────────

    void SpawnItems()
    {
        ClearItems();

        if (itemPrefab == null)
        {
            Debug.LogError("[ShelfShopManager] itemPrefab is not assigned!");
            return;
        }

        // Track slot mana yang udah diisi fixed item
        HashSet<int> occupiedSlots = new HashSet<int>();

        // ── Spawn Fixed Items ──────────────────────────────────
        foreach (FixedShopSlot fixedSlot in fixedItems)
        {
            if (fixedSlot.shopItem == null) continue;
            if (fixedSlot.slotIndex < 0 || fixedSlot.slotIndex >= shelfSlots.Count) continue;

            SpawnItemAtSlot(fixedSlot.shopItem, fixedSlot.slotIndex);
            occupiedSlots.Add(fixedSlot.slotIndex);
        }

        // ── Spawn Random Items ────────────────────────────────
        if (randomItemPool.Count == 0 || randomItemCount <= 0) return;

        // Set random seed kalau pakai fixed seed
        if (useFixedSeed)
            Random.InitState(randomSeed);

        // Kumpulkan slot yang masih kosong
        List<int> emptySlots = new List<int>();
        for (int i = 0; i < shelfSlots.Count; i++)
        {
            if (!occupiedSlots.Contains(i))
                emptySlots.Add(i);
        }

        // Shuffle empty slots
        for (int i = emptySlots.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = emptySlots[i];
            emptySlots[i] = emptySlots[j];
            emptySlots[j] = temp;
        }

        // Buat copy pool biar nggak repeat item yang sama
        List<ShopItem> poolCopy = new List<ShopItem>(randomItemPool);

        int spawnCount = Mathf.Min(randomItemCount, emptySlots.Count, poolCopy.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            // Pick random dari pool
            int randomIndex = Random.Range(0, poolCopy.Count);
            ShopItem randomItem = poolCopy[randomIndex];
            poolCopy.RemoveAt(randomIndex); // hapus dari pool biar nggak dobel

            SpawnItemAtSlot(randomItem, emptySlots[i]);
        }

        Debug.Log($"[ShelfShopManager] Spawned {spawnedItems.Count} items on shelves.");
    }

    void SpawnItemAtSlot(ShopItem shopItem, int slotIndex)
    {
        Transform slot = shelfSlots[slotIndex];

        GameObject itemObj = Instantiate(itemPrefab, slot.position, Quaternion.identity, slot);
        itemObj.name = $"ShelfItem_{shopItem.item.itemName}";

        ShopItemWorld itemWorld = itemObj.GetComponent<ShopItemWorld>();
        if (itemWorld != null)
        {
            itemWorld.Setup(shopItem);
            spawnedItems.Add(itemWorld);
        }
        else
        {
            Debug.LogError($"[ShelfShopManager] itemPrefab doesn't have ShopItemWorld component!");
        }
    }

    void ClearItems()
    {
        foreach (ShopItemWorld item in spawnedItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        spawnedItems.Clear();
    }

    // ─────────────────────────────────────────────────────────────
    // CONTEXT MENU (untuk test di Editor)
    // ─────────────────────────────────────────────────────────────

    [ContextMenu("Test: Open Shop")]
    void TestOpenShop() => OpenShop();

    [ContextMenu("Test: Close Shop")]
    void TestCloseShop() => CloseShop();

    [ContextMenu("Test: Respawn Items")]
    void TestRespawnItems() => SpawnItems();

    public bool IsOpen => isOpen;
}

// ─────────────────────────────────────────────────────────────────
// DATA CLASS
// ─────────────────────────────────────────────────────────────────

/// <summary>
/// Pasangan slot index + ShopItem untuk fixed item.
/// </summary>
[System.Serializable]
public class FixedShopSlot
{
    [Tooltip("Index slot di shelfSlots list (mulai dari 0)")]
    public int slotIndex;

    public ShopItem shopItem;
}