using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    [Header("Settings")]
    public bool updateEveryFrame = true; // True for moving objects (player)
    public int sortingOrderBase = 5000; // Base value for sorting
    public int offset = 0; // Manual offset if needed
    
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"YPositionSorter on {gameObject.name} has no SpriteRenderer!");
            enabled = false;
            return;
        }
        
        UpdateSortingOrder();
    }

    void LateUpdate()
    {
        if (updateEveryFrame)
        {
            UpdateSortingOrder();
        }
    }

    void UpdateSortingOrder()
    {
        // Higher Y position = lower sorting order (renders behind)
        // Lower Y position = higher sorting order (renders in front)
        spriteRenderer.sortingOrder = (int)(sortingOrderBase - transform.position.y * 100) + offset;
    }
}
