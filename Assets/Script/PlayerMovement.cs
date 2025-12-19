using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

    public float moveSpeed = 4f;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 movement;

    private bool canMove = true;

    // ✅ SAFE getters (no more Gizmo crashes)
    public float GetLastMoveX()
    {
        if (anim == null) return 0f;
        return anim.GetFloat("LastMoveX");
    }

    public float GetLastMoveY()
    {
        if (anim == null) return -1f; // default facing DOWN
        return anim.GetFloat("LastMoveY");
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // ✅ Initialize default facing direction
        anim.SetFloat("LastMoveX", 0);
        anim.SetFloat("LastMoveY", -1);
    }

    void Update()
    {
        if (!canMove)
        {
            anim.SetFloat("Speed", 0);
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Normalize for diagonal movement
        movement = movement.normalized;

        if (movement != Vector2.zero)
        {
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                anim.SetFloat("MoveX", Mathf.Sign(movement.x));
                anim.SetFloat("MoveY", 0);

                anim.SetFloat("LastMoveX", Mathf.Sign(movement.x));
                anim.SetFloat("LastMoveY", 0);
            }
            else
            {
                anim.SetFloat("MoveX", 0);
                anim.SetFloat("MoveY", Mathf.Sign(movement.y));

                anim.SetFloat("LastMoveX", 0);
                anim.SetFloat("LastMoveY", Mathf.Sign(movement.y));
            }
        }
        else
        {
            anim.SetFloat("MoveX", 0);
            anim.SetFloat("MoveY", 0);
        }

        anim.SetFloat("Speed", movement.sqrMagnitude);
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        rb.MovePosition(
            rb.position + movement * moveSpeed * Time.fixedDeltaTime
        );
    }

    public void LockMovement(bool lockMove)
    {
        canMove = !lockMove;

        movement = Vector2.zero;
        rb.velocity = Vector2.zero;

        anim.SetFloat("Speed", 0f);
        anim.SetFloat("MoveX", 0f);
        anim.SetFloat("MoveY", 0f);
    }
}
