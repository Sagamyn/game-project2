using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    private SpriteRenderer sr;

    // Adjust this to match your sprite feet position
    [Tooltip("Negative value moves sorting point downward")]
    public float yOffset = -0.16f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        float sortY = transform.position.y + yOffset;
        sr.sortingOrder = Mathf.RoundToInt(-sortY * 100);
    }
}
