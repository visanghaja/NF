using Unity.VisualScripting;
using UnityEngine;

public class Player_Wall : MonoBehaviour
{
    public Vector2 gravityDirection = Vector2.down;
    public float gravityStrength = 9.81f;
    private Rigidbody2D rb;
    private Player playerScript;
    private SpriteRenderer spriteRenderer;
    private bool isOnWall = false;
    private Vector2 originalGravityDirection = Vector2.down;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerScript = GetComponent<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalGravityDirection = gravityDirection;
    }

    void FixedUpdate()
    {
        if (isOnWall && (playerScript.CurrentState == PlayerState.Jumping || playerScript.CurrentState == PlayerState.Dash))
        {
            rb.gravityScale = 0;
            rb.AddForce(gravityDirection.normalized * gravityStrength);
        }
        else
        {
            rb.gravityScale = 1;
            gravityDirection = originalGravityDirection;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (playerScript.CurrentState != PlayerState.Jumping && playerScript.CurrentState != PlayerState.Dash)
            return;

        switch (collision.gameObject.tag)
        {
            case "bottom":
                isOnWall = true;
                SetGravityDirection(Vector2.down);
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case "left":
                isOnWall = true;
                SetGravityDirection(Vector2.left);
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case "right":
                isOnWall = true;
                SetGravityDirection(Vector2.right);
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            case "top":
                isOnWall = true;
                SetGravityDirection(Vector2.up);
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("bottom") || 
            collision.gameObject.CompareTag("left") || 
            collision.gameObject.CompareTag("right") || 
            collision.gameObject.CompareTag("top"))
        {
            isOnWall = false;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            gravityDirection = originalGravityDirection;
            rb.gravityScale = 1;
        }
    }

    void SetGravityDirection(Vector2 direction)
    {
        gravityDirection = direction;
    }
}
