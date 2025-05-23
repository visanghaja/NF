using Unity.VisualScripting;
using UnityEngine;

public class Player_Wall : MonoBehaviour
{
    public float gravityStrength = 9.81f;
    private Rigidbody2D rb;
    private Player playerScript;
    private SpriteRenderer spriteRenderer;
    private bool isOnWall = false;
    private Vector2 originalGravity;
    private WallDirection currentWallDirection = WallDirection.None;
    private Quaternion originalRotation;
    private float lastRotationTime = 0f;
    private float rotationCooldown = 0.3f; // 회전 쿨다운 시간을 0.3초로 줄임

    public enum WallDirection
    {
        None,
        Left,
        Right,
        Top,
        Bottom
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerScript = GetComponent<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalGravity = Physics2D.gravity;
        originalRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        if (!isOnWall)
        {
            Physics2D.gravity = originalGravity;
            currentWallDirection = WallDirection.None;
            transform.rotation = originalRotation;
        }
        playerScript.SetWallDirection(currentWallDirection);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 회전 쿨다운 체크
        if (Time.time - lastRotationTime < rotationCooldown)
            return;

        WallDirection newWallDirection = WallDirection.None;
        switch (collision.gameObject.tag)
        {
            case "bottom":
                newWallDirection = WallDirection.Bottom;
                break;
            case "left":
                newWallDirection = WallDirection.Left;
                break;
            case "right":
                newWallDirection = WallDirection.Right;
                break;
            case "top":
                newWallDirection = WallDirection.Top;
                break;
            default:
                return;
        }

        // 이미 같은 벽에 닿아있으면 무시
        if (newWallDirection == currentWallDirection)
            return;

        isOnWall = true;
        currentWallDirection = newWallDirection;
        lastRotationTime = Time.time;

        switch (newWallDirection)
        {
            case WallDirection.Bottom:
                Debug.Log("Hit bottom wall");
                Physics2D.gravity = Vector2.down * gravityStrength;
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case WallDirection.Left:
                Debug.Log("Hit left wall");
                Physics2D.gravity = Vector2.left * gravityStrength;
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            case WallDirection.Right:
                Debug.Log("Hit right wall");
                Physics2D.gravity = Vector2.right * gravityStrength;
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case WallDirection.Top:
                Debug.Log("Hit top wall");
                Physics2D.gravity = Vector2.up * gravityStrength;
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
            // 회전 쿨다운 체크
            if (Time.time - lastRotationTime < rotationCooldown)
                return;

            Debug.Log("Exiting wall collision");
            isOnWall = false;
            currentWallDirection = WallDirection.None;
            transform.rotation = originalRotation;
            lastRotationTime = Time.time;
            Physics2D.gravity = originalGravity;
        }
    }
}
