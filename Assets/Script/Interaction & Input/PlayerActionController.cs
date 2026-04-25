using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerActionController : MonoBehaviour
{
    [Header("References")]
    public PlayerFarming farming;
    public DialogueManager dialogue;
    public PlayerToolController toolController;
    public Hotbar hotbar;
    public Camera mainCamera;

    [Header("Harvest Click Settings")]
    [Tooltip("Layer that contains your crop tilemap colliders, or leave empty to use world position")]
    public Tilemap cropTilemap;   // assign your crop tilemap here

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (dialogue != null && dialogue.IsOpen)
            return;

        if (hotbar == null) return;

        ItemData selected = hotbar.SelectedItem;

        // ── LEFT CLICK: harvest with Hoe ──────────────────────────────
        if (selected is ToolItem tool && tool.toolType == ToolType.Hoe)
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryHarvestAtIndicator();
            }
        }

        // ── E KEY: existing tool use / planting ───────────────────────
        if (!Input.GetKeyDown(KeyCode.E))
            return;

        if (selected is ToolItem toolE)
        {
            if (toolController == null) return;
            toolController.UseTool(toolE);
            return;
        }

        // Seeds / harvesting via E (kept as fallback)
        if (farming != null)
            farming.TryFarm();
    }

    // ─────────────────────────────────────────────────────────────────
    // Harvest at the indicator tile (same target as tilling / planting)
    // ─────────────────────────────────────────────────────────────────
    void TryHarvestAtIndicator()
    {
        if (farming == null) return;

        // Use the indicator position — exactly the same cell the E-key actions use
        if (farming.indicator == null)
        {
            Debug.LogWarning("[PlayerActionController] No indicator assigned on PlayerFarming!");
            return;
        }

        Tilemap referenceTilemap = farming.soilTilemap != null ? farming.soilTilemap : cropTilemap;
        if (referenceTilemap == null) return;

        // indicator.transform.position is already snapped to tile center
        Vector3Int cell = referenceTilemap.WorldToCell(farming.indicator.transform.position);

        farming.TryHarvestAtCell(cell);
    }
}