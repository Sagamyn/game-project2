using UnityEngine;

public class ToolPointController : MonoBehaviour
{
    public Transform toolPoint;
    public float distance = 0.5f;

    void Update()
    {
        if (PlayerMovement.Instance == null)
            return;

        float x = PlayerMovement.Instance.GetLastMoveX();
        float y = PlayerMovement.Instance.GetLastMoveY();

        Vector2 dir = new Vector2(x, y);

        // Safety fallback
        if (dir == Vector2.zero)
            dir = Vector2.down;

        toolPoint.localPosition = dir.normalized * distance;
    }
}
