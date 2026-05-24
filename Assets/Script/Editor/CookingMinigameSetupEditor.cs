#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// Editor tool yang otomatis membuat seluruh hierarki UI untuk
/// Cooking Temperature Minigame (Stardew Valley fishing bar style).
/// 
/// Cara pakai: Unity Menu → Tools → Cooking → Setup Temperature Minigame UI
/// </summary>
public static class CookingMinigameSetupEditor
{
    private const string SPRITE_PATH = "Assets/Art/UI/CookingMinigame";

    [UnityEditor.MenuItem("Tools/Cooking/Setup Temperature Minigame UI")]
    public static void SetupMinigameUI()
    {
        if (!EditorUtility.DisplayDialog(
            "Setup Cooking Minigame UI",
            "Ini akan membuat:\n\n" +
            "• Sprite placeholder (rounded bar, circle)\n" +
            "• Hierarki UI lengkap\n" +
            "• Wiring semua referensi otomatis\n\n" +
            "Lanjutkan?",
            "Ya, Buat!", "Batal"))
            return;

        // ─── 1. Generate Placeholder Sprites ─────────────
        EnsureDirectory(SPRITE_PATH);

        Sprite barBgSprite = CreateRoundedRectSprite("bar_background", 64, 512, 20);
        Sprite catchBarSprite = CreateRoundedRectSprite("catch_bar", 52, 128, 14);
        Sprite circleSprite = CreateCircleSprite("target_circle", 64);
        Sprite progressBgSprite = CreateRoundedRectSprite("progress_bg", 20, 512, 8);
        Sprite progressFillSprite = CreateRoundedRectSprite("progress_fill", 16, 508, 6);

        if (barBgSprite == null || catchBarSprite == null || circleSprite == null)
        {
            Debug.LogError("[CookingMinigameSetup] Gagal membuat sprite!");
            return;
        }

        // ─── 2. Find or Create Canvas ────────────────────
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
        }

        // ─── 3. Build Hierarchy ──────────────────────────
        BuildHierarchy(canvas.transform,
            barBgSprite, catchBarSprite, circleSprite,
            progressBgSprite, progressFillSprite);

        Debug.Log("✓ [CookingMinigameSetup] UI berhasil dibuat!");
        EditorUtility.DisplayDialog(
            "Setup Berhasil! ✓",
            "UI Cooking Temperature Minigame sudah dibuat!\n\n" +
            "Langkah selanjutnya:\n" +
            "1. Assign CookingMinigamePanel ke CookingStation.temperatureMinigame\n" +
            "2. (Opsional) Ganti sprite placeholder dengan sprite custom\n" +
            "3. Play dan test!",
            "OK");
    }

    // ═══════════════════════════════════════════════════
    //  BUILD HIERARCHY
    // ═══════════════════════════════════════════════════

    static void BuildHierarchy(Transform canvasTransform,
        Sprite barBgSprite, Sprite catchBarSprite, Sprite circleSprite,
        Sprite progressBgSprite, Sprite progressFillSprite)
    {
        // ─── ROOT: CookingMinigamePanel ──────────────────
        GameObject panel = CreateUIObject("CookingMinigamePanel", canvasTransform);
        StretchAll(panel.GetComponent<RectTransform>());

        // Image transparan (untuk raycast blocking)
        var panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0);
        panelImage.raycastTarget = false;

        // CanvasGroup untuk UIAnimator
        panel.AddComponent<CanvasGroup>();

        // UIAnimator
        var uiAnimator = panel.AddComponent<UIAnimator>();
        uiAnimator.animationType = UIAnimator.AnimationType.Fade;
        uiAnimator.animationDuration = 0.25f;
        uiAnimator.disableOnHide = false;

        // CookingTemperatureMinigame
        var minigame = panel.AddComponent<CookingTemperatureMinigame>();

        // Set default difficulty
        var difficulty = new MinigameDifficulty();
        difficulty.catchBarSize = 0.25f;
        difficulty.targetSpeed = 150f;
        difficulty.progressGainRate = 15f;
        difficulty.progressLossRate = 10f;
        difficulty.gravity = 400f;
        difficulty.pushForce = 600f;
        difficulty.minIdleTime = 0.5f;
        difficulty.maxIdleTime = 2f;
        minigame.defaultDifficulty = difficulty;

        // CookingMinigameUI
        var visualUI = panel.AddComponent<CookingMinigameUI>();

        // ─── BACKGROUND DIM ─────────────────────────────
        GameObject bgDim = CreateUIObject("BackgroundDim", panel.transform);
        StretchAll(bgDim.GetComponent<RectTransform>());
        var bgDimImage = bgDim.AddComponent<Image>();
        bgDimImage.color = new Color(0f, 0f, 0f, 0.45f);
        bgDimImage.raycastTarget = true;

        // ─── MINIGAME CONTAINER ─────────────────────────
        // Ini container utama, diposisikan di sisi kanan layar
        GameObject container = CreateUIObject("MinigameContainer", panel.transform);
        var containerRT = container.GetComponent<RectTransform>();
        containerRT.anchorMin = new Vector2(1f, 0.5f);
        containerRT.anchorMax = new Vector2(1f, 0.5f);
        containerRT.pivot = new Vector2(1f, 0.5f);
        containerRT.anchoredPosition = new Vector2(-100f, 0f);
        containerRT.sizeDelta = new Vector2(150f, 440f);

        // ─── VERTICAL BAR (Background bar utama) ────────
        GameObject vertBar = CreateUIObject("VerticalBar", container.transform);
        var vertBarRT = vertBar.GetComponent<RectTransform>();
        vertBarRT.anchorMin = new Vector2(0.5f, 0.5f);
        vertBarRT.anchorMax = new Vector2(0.5f, 0.5f);
        vertBarRT.pivot = new Vector2(0.5f, 0.5f);
        vertBarRT.sizeDelta = new Vector2(60f, 400f);
        vertBarRT.anchoredPosition = new Vector2(15f, 0f); // sedikit ke kanan, sisakan ruang progress bar

        var vertBarImage = vertBar.AddComponent<Image>();
        vertBarImage.sprite = barBgSprite;
        vertBarImage.type = Image.Type.Sliced;
        vertBarImage.color = new Color(0.10f, 0.10f, 0.18f, 0.92f); // dark navy

        // Border / outline effect (inner glow)
        GameObject barBorder = CreateUIObject("BarBorder", vertBar.transform);
        var barBorderRT = barBorder.GetComponent<RectTransform>();
        StretchAll(barBorderRT);
        barBorderRT.offsetMin = new Vector2(-2f, -2f);
        barBorderRT.offsetMax = new Vector2(2f, 2f);
        barBorder.transform.SetAsFirstSibling(); // behind everything
        var barBorderImage = barBorder.AddComponent<Image>();
        barBorderImage.sprite = barBgSprite;
        barBorderImage.type = Image.Type.Sliced;
        barBorderImage.color = new Color(0.25f, 0.25f, 0.4f, 0.6f); // subtle border

        // ─── CATCH BAR (player-controlled green bar) ─────
        GameObject catchBarObj = CreateUIObject("CatchBar", vertBar.transform);
        var catchBarRT = catchBarObj.GetComponent<RectTransform>();
        catchBarRT.anchorMin = new Vector2(0.5f, 0f); // anchored to bottom
        catchBarRT.anchorMax = new Vector2(0.5f, 0f);
        catchBarRT.pivot = new Vector2(0.5f, 0.5f);   // center pivot
        catchBarRT.sizeDelta = new Vector2(52f, 80f);  // height akan di-override oleh script
        catchBarRT.anchoredPosition = new Vector2(0f, 200f); // mulai di tengah

        var catchBarImage = catchBarObj.AddComponent<Image>();
        catchBarImage.sprite = catchBarSprite;
        catchBarImage.type = Image.Type.Sliced;
        catchBarImage.color = new Color(0.29f, 0.87f, 0.50f, 0.75f); // green

        // ─── TARGET (bouncing icon) ─────────────────────
        GameObject targetObj = CreateUIObject("Target", vertBar.transform);
        var targetRT = targetObj.GetComponent<RectTransform>();
        targetRT.anchorMin = new Vector2(0.5f, 0f);
        targetRT.anchorMax = new Vector2(0.5f, 0f);
        targetRT.pivot = new Vector2(0.5f, 0.5f);
        targetRT.sizeDelta = new Vector2(38f, 38f);
        targetRT.anchoredPosition = new Vector2(0f, 200f);

        var targetImage = targetObj.AddComponent<Image>();
        targetImage.sprite = circleSprite;
        targetImage.color = new Color(1f, 0.85f, 0.4f, 0.95f); // warm yellow/orange

        // Target inner icon (emoji-style)
        GameObject targetInner = CreateUIObject("TargetIcon", targetObj.transform);
        var targetInnerRT = targetInner.GetComponent<RectTransform>();
        StretchAll(targetInnerRT);
        targetInnerRT.offsetMin = new Vector2(6f, 6f);
        targetInnerRT.offsetMax = new Vector2(-6f, -6f);
        var targetInnerImage = targetInner.AddComponent<Image>();
        targetInnerImage.sprite = circleSprite;
        targetInnerImage.color = new Color(1f, 0.55f, 0.1f, 0.9f); // darker center

        // ─── PROGRESS SLIDER ─────────────────────────────
        // Positioned to the left of the vertical bar
        GameObject sliderObj = CreateUIObject("ProgressSlider", container.transform);
        var sliderRT = sliderObj.GetComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRT.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRT.pivot = new Vector2(0.5f, 0.5f);
        sliderRT.sizeDelta = new Vector2(16f, 400f);
        sliderRT.anchoredPosition = new Vector2(-30f, 0f); // left of vertical bar

        // Slider Background
        GameObject sliderBg = CreateUIObject("Background", sliderObj.transform);
        var sliderBgRT = sliderBg.GetComponent<RectTransform>();
        StretchAll(sliderBgRT);
        var sliderBgImage = sliderBg.AddComponent<Image>();
        sliderBgImage.sprite = progressBgSprite;
        sliderBgImage.type = Image.Type.Sliced;
        sliderBgImage.color = new Color(0.12f, 0.12f, 0.18f, 0.85f);

        // Fill Area
        GameObject fillArea = CreateUIObject("Fill Area", sliderObj.transform);
        var fillAreaRT = fillArea.GetComponent<RectTransform>();
        StretchAll(fillAreaRT);
        fillAreaRT.offsetMin = new Vector2(2f, 2f);
        fillAreaRT.offsetMax = new Vector2(-2f, -2f);

        // Fill
        GameObject fill = CreateUIObject("Fill", fillArea.transform);
        var fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        var fillImage = fill.AddComponent<Image>();
        fillImage.sprite = progressFillSprite;
        fillImage.type = Image.Type.Sliced;
        fillImage.color = new Color(0.13f, 0.77f, 0.37f); // green

        // Slider Component
        var slider = sliderObj.AddComponent<Slider>();
        slider.direction = Slider.Direction.BottomToTop;
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.value = 50f;
        slider.fillRect = fillRT;
        slider.interactable = false; // controlled by script only
        slider.transition = Selectable.Transition.None;

        // Remove navigation to prevent input issues
        var nav = slider.navigation;
        nav.mode = Navigation.Mode.None;
        slider.navigation = nav;

        // ─── INSTRUCTION TEXT ────────────────────────────
        GameObject instrObj = CreateUIObject("InstructionText", panel.transform);
        var instrRT = instrObj.GetComponent<RectTransform>();
        instrRT.anchorMin = new Vector2(0.5f, 0.15f);
        instrRT.anchorMax = new Vector2(0.5f, 0.15f);
        instrRT.pivot = new Vector2(0.5f, 0.5f);
        instrRT.sizeDelta = new Vector2(500f, 60f);
        instrRT.anchoredPosition = Vector2.zero;

        var instrText = instrObj.AddComponent<TextMeshProUGUI>();
        instrText.text = "Klik Tahan untuk Menaikkan!";
        instrText.fontSize = 28f;
        instrText.alignment = TextAlignmentOptions.Center;
        instrText.color = new Color(1f, 1f, 1f, 0.9f);
        instrText.fontStyle = FontStyles.Bold;
        instrText.enableWordWrapping = false;

        // Text shadow via outline
        var instrOutline = instrObj.AddComponent<Outline>();
        instrOutline.effectColor = new Color(0f, 0f, 0f, 0.7f);
        instrOutline.effectDistance = new Vector2(2f, -2f);

        // ─── RESULT TEXT ─────────────────────────────────
        GameObject resultObj = CreateUIObject("ResultText", panel.transform);
        var resultRT = resultObj.GetComponent<RectTransform>();
        resultRT.anchorMin = new Vector2(0.5f, 0.5f);
        resultRT.anchorMax = new Vector2(0.5f, 0.5f);
        resultRT.pivot = new Vector2(0.5f, 0.5f);
        resultRT.sizeDelta = new Vector2(500f, 80f);
        resultRT.anchoredPosition = new Vector2(0f, 50f);

        var resultText = resultObj.AddComponent<TextMeshProUGUI>();
        resultText.text = "BERHASIL!";
        resultText.fontSize = 48f;
        resultText.alignment = TextAlignmentOptions.Center;
        resultText.color = new Color(0.2f, 0.95f, 0.35f);
        resultText.fontStyle = FontStyles.Bold;
        resultText.enableWordWrapping = false;
        resultObj.SetActive(false);

        var resultOutline = resultObj.AddComponent<Outline>();
        resultOutline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        resultOutline.effectDistance = new Vector2(3f, -3f);

        // ─── WIRE REFERENCES ─────────────────────────────
        // CookingTemperatureMinigame
        minigame.verticalBar = vertBarRT;
        minigame.catchBar = catchBarRT;
        minigame.target = targetRT;
        minigame.progressBar = slider;
        minigame.uiAnimator = uiAnimator;
        minigame.visualUI = visualUI;

        // CookingMinigameUI
        visualUI.catchBar = catchBarRT;
        visualUI.target = targetRT;
        visualUI.progressBar = slider;
        visualUI.catchBarImage = catchBarImage;
        visualUI.targetImage = targetImage;
        visualUI.progressFillImage = fillImage;
        visualUI.backgroundDim = bgDimImage;
        visualUI.minigameContainer = containerRT;
        visualUI.instructionText = instrText;
        visualUI.resultText = resultText;

        // ─── FINALIZE ────────────────────────────────────
        // Start hidden (set langsung karena UIAnimator.Awake belum jalan di edit mode)
        var cg = panel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }

        // Register undo
        Undo.RegisterCreatedObjectUndo(panel, "Create Cooking Minigame UI");

        // Select the created object
        Selection.activeGameObject = panel;

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
    }

    // ═══════════════════════════════════════════════════
    //  SPRITE GENERATION
    // ═══════════════════════════════════════════════════

    static Sprite CreateRoundedRectSprite(string name, int width, int height, int radius)
    {
        string path = $"{SPRITE_PATH}/{name}.png";

        // Check if already exists
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool inside = IsInsideRoundedRect(x, y, width, height, radius);
                pixels[y * width + x] = inside
                    ? new Color32(255, 255, 255, 255)
                    : new Color32(0, 0, 0, 0);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();

        return SaveTextureAsSprite(tex, path, radius);
    }

    static Sprite CreateCircleSprite(string name, int size)
    {
        string path = $"{SPRITE_PATH}/{name}.png";

        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[size * size];
        float center = (size - 1) / 2f;
        float radiusSq = center * center;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float distSq = dx * dx + dy * dy;

                if (distSq <= radiusSq)
                {
                    // Anti-aliasing at edges
                    float dist = Mathf.Sqrt(distSq);
                    float edge = center - dist;
                    byte alpha = (byte)(edge >= 1f ? 255 : (byte)(edge * 255));
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
                else
                {
                    pixels[y * size + x] = new Color32(0, 0, 0, 0);
                }
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();

        return SaveTextureAsSprite(tex, path, 0);
    }

    static bool IsInsideRoundedRect(int x, int y, int w, int h, int r)
    {
        // Inside the main rectangular regions
        if (x >= r && x < w - r) return true;  // horizontal middle
        if (y >= r && y < h - r) return true;  // vertical middle

        // Check the four corner circles
        int cx, cy;

        if (x < r && y < r)
        {
            cx = r; cy = r;   // bottom-left corner
        }
        else if (x >= w - r && y < r)
        {
            cx = w - r - 1; cy = r;   // bottom-right
        }
        else if (x < r && y >= h - r)
        {
            cx = r; cy = h - r - 1;   // top-left
        }
        else if (x >= w - r && y >= h - r)
        {
            cx = w - r - 1; cy = h - r - 1; // top-right
        }
        else
        {
            return true; // shouldn't reach here
        }

        float dx = x - cx;
        float dy = y - cy;
        return (dx * dx + dy * dy) <= (r * r);
    }

    static Sprite SaveTextureAsSprite(Texture2D tex, string path, int border)
    {
        byte[] pngData = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);

        File.WriteAllBytes(path, pngData);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        // Configure as sprite with 9-slice border
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            if (border > 0)
            {
                importer.spriteBorder = new Vector4(border, border, border, border);
            }

            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ═══════════════════════════════════════════════════
    //  UTILITY
    // ═══════════════════════════════════════════════════

    static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] parts = path.Split('/');
            string current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }

    static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    static void StretchAll(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
#endif
