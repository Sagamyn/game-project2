using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(SpriteRenderer))]
public class TilemapIndicator : MonoBehaviour
{
    [Header("References")]
    public Tilemap referenceTilemap;   // Assign GroundTilemap
    public Transform player;
    public Animator playerAnimator;

    [Header("Settings")]
    public Vector2 feetOffset = new Vector2(0, -0.25f);
    public bool showOnlyWhenPressE = true;

    private SpriteRenderer sr;
    private Vector2Int facingDirection = Vector2Int.down;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;
    }

    void Update()
    {
        UpdateFacingDirection();
        UpdateIndicatorPosition();
        UpdateVisibility();
    }

    void UpdateFacingDirection()
    {
        float lastX = playerAnimator.GetFloat("LastMoveX");
        float lastY = playerAnimator.GetFloat("LastMoveY");

        if (Mathf.Abs(lastX) > Mathf.Abs(lastY))
            facingDirection = new Vector2Int((int)Mathf.Sign(lastX), 0);
        else
            facingDirection = new Vector2Int(0, (int)Mathf.Sign(lastY));
    }

    void UpdateIndicatorPosition()
    {
        Vector3 origin = player.position + (Vector3)feetOffset;
        Vector3 targetWorld = origin + new Vector3(
            facingDirection.x,
            facingDirection.y,
            0
        );

        Vector3Int cell = referenceTilemap.WorldToCell(targetWorld);

        transform.position =
            referenceTilemap.CellToWorld(cell) +
            referenceTilemap.cellSize / 2f;
    }

    void UpdateVisibility()
    {
        sr.enabled = showOnlyWhenPressE
            ? Input.GetKey(KeyCode.E)
            : true;
    }
}
