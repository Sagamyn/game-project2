using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerToolController : MonoBehaviour
{
    [Header("References")]
    public PlayerFarming farming;
    public Tilemap soilTilemap;
    public TileBase tilledSoilTile;
    public TilemapIndicator indicator;

    [Header("World Interaction")]
    public Transform toolPoint;
    public float toolRange = 0.8f;
    public LayerMask resourceMask;

    [Header("Watering Can")]
    public int maxWater = 10;
    int currentWater;

    void Start()
    {
        currentWater = maxWater;
    }

    public void UseTool(ItemData item)
    {
        if (item is not ToolItem tool)
            return;

        Vector3Int cell = soilTilemap.WorldToCell(indicator.transform.position);

        switch (tool.toolType)
        {
            // ===== SOIL =====
            case ToolType.Hoe:
                UseHoe(cell);
                break;

            case ToolType.Shovel:
                UseShovel(cell);
                break;

            case ToolType.WateringCan:
                UseWateringCan(cell);
                break;

            // ===== WORLD =====
            case ToolType.Axe:
                UseAxe();
                break;

            case ToolType.Pickaxe:
                UsePickaxe();
                break;
        }
    }

    // ===================== SOIL TOOLS =====================

    void UseHoe(Vector3Int cell)
    {
        Debug.Log($"UseHoe called at cell: {cell}");

        // Check if farming zones are required
        if (farming != null && farming.requireZone)
        {
            // Use the new public method that checks the CELL
            if (!farming.CanPerformAction(cell, FarmingAction.Tilling))
            {
                Debug.LogWarning($"CanPerformAction returned FALSE for tilling at {cell}");
                return; // The farming script already showed the warning
            }
        }

        // Proceed with tilling
        if (soilTilemap.GetTile(cell) == null)
        {
            soilTilemap.SetTile(cell, tilledSoilTile);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.hoeSound);
            Debug.Log($"✓ Tilled soil at {cell}");
        }
        else
        {
            Debug.Log($"Cell {cell} already has a tile");
        }
    }

    void UseShovel(Vector3Int cell)
    {
        if (soilTilemap.GetTile(cell) == tilledSoilTile)
        {
            soilTilemap.SetTile(cell, null);
            AudioManager.Instance.PlaySFX(AudioManager.Instance.shovelSound);
        }
    }

    void UseWateringCan(Vector3Int cell)
    {
        if (currentWater <= 0)
        {
            Debug.Log("Watering can empty");
            return;
        }

        if (farming == null || !farming.HasCrop(cell))
            return;

        farming.WaterCrop(cell);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.waterSound);
        currentWater--;

        Debug.Log($"Water left: {currentWater}");
    }

    // ===================== WORLD TOOLS =====================

    void UseAxe()
    {
        TryHitResource<Tree>();
    }

    void UsePickaxe()
    {
        TryHitResource<Rock>();
    }

    void TryHitResource<T>() where T : Component
    {
        if (toolPoint == null)
            return;

        Collider2D hit = Physics2D.OverlapCircle(
            toolPoint.position,
            toolRange,
            resourceMask
        );

        if (hit == null)
        {
            Debug.Log("No resource hit");
            return;
        }

        T target = hit.GetComponent<T>();
        if (target == null)
        {
            Debug.Log("Hit something, but wrong type");
            return;
        }

        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable == null)
        {
            Debug.Log("Target is not damageable");
            return;
        }

        if (typeof(T) == typeof(Tree))
            AudioManager.Instance.PlaySFX(AudioManager.Instance.axeHitSound);
        else if (typeof(T) == typeof(Rock))
            AudioManager.Instance.PlaySFX(AudioManager.Instance.pickaxeHitSound);

        damageable.TakeDamage(1);
        Debug.Log($"Hit {typeof(T).Name}");
    }

    // ===================== REFILL =====================

    public void RefillWater()
    {
        currentWater = maxWater;
        Debug.Log("Water refilled!");
    }

    // ===================== DEBUG =====================

    void OnDrawGizmosSelected()
    {
        if (toolPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(toolPoint.position, toolRange);
    }
}