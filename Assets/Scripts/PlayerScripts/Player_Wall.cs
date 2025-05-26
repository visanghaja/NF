using UnityEngine;

public class Player_Wall : MonoBehaviour
{
    public enum WallDirection
    {
        None,
        Left,
        Right,
        Top,
        Bottom
    }

    [Header("Wall Detection")]
    public float wallCheckDistance = 0.6f;
    public LayerMask wallLayer;
    public float gravityStrength = 3f;

    private Player playerScript;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private WallDirection currentWallDirection = WallDirection.None;
    private Vector2 gravityDirection = Vector2.down;

    private void Start()
    {
        playerScript = GetComponent<Player>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0; // 기본 중력을 비활성화하고 직접 제어
    }

    private void FixedUpdate()
    {
        CheckWallCollision();
        ApplyGravity();
    }

    private void CheckWallCollision()
    {
        // 각 방향으로의 레이캐스트 체크
        RaycastHit2D leftCheck = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer);
        RaycastHit2D rightCheck = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);
        RaycastHit2D topCheck = Physics2D.Raycast(transform.position, Vector2.up, wallCheckDistance, wallLayer);
        RaycastHit2D bottomCheck = Physics2D.Raycast(transform.position, Vector2.down, wallCheckDistance, wallLayer);

        WallDirection newWallDirection = WallDirection.None;

        // 벽 방향 감지
        if (leftCheck.collider != null)
        {
            newWallDirection = WallDirection.Left;
        }
        else if (rightCheck.collider != null)
        {
            newWallDirection = WallDirection.Right;
        }
        else if (topCheck.collider != null)
        {
            newWallDirection = WallDirection.Top;
        }
        else if (bottomCheck.collider != null)
        {
            newWallDirection = WallDirection.Bottom;
        }

        // 벽 방향이 바뀌었을 때만 처리
        if (newWallDirection != currentWallDirection && newWallDirection != WallDirection.None)
        {
            currentWallDirection = newWallDirection;
            UpdateGravityDirection();
            RotatePlayer();
            playerScript.SetWallDirection(currentWallDirection);
        }
    }

    private void UpdateGravityDirection()
    {
        switch (currentWallDirection)
        {
            case WallDirection.Left:
                gravityDirection = Vector2.left;
                break;
            case WallDirection.Right:
                gravityDirection = Vector2.right;
                break;
            case WallDirection.Top:
                gravityDirection = Vector2.up;
                break;
            case WallDirection.Bottom:
                gravityDirection = Vector2.down;
                break;
            default:
                gravityDirection = Vector2.down;
                break;
        }
    }

    private void ApplyGravity()
    {
        // 현재 속도에 중력 방향으로 힘을 추가
        rb.linearVelocity += gravityDirection * gravityStrength * Time.fixedDeltaTime;
    }

    private void RotatePlayer()
    {
        // 벽 방향에 따라 플레이어 회전
        switch (currentWallDirection)
        {
            case WallDirection.Left:
                transform.rotation = Quaternion.Euler(0, 0, -90); // 왼쪽 벽에서는 시계방향으로 90도 회전
                break;
            case WallDirection.Right:
                transform.rotation = Quaternion.Euler(0, 0, 90); // 오른쪽 벽에서는 반시계방향으로 90도 회전
                break;
            case WallDirection.Top:
                transform.rotation = Quaternion.Euler(0, 0, 180); // 180도 회전
                break;
            case WallDirection.Bottom:
                transform.rotation = Quaternion.Euler(0, 0, 0); // 기본 회전
                break;
            default:
                transform.rotation = Quaternion.Euler(0, 0, 0); // 기본 회전
                break;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        // 왼쪽 레이
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * wallCheckDistance);
        // 오른쪽 레이
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * wallCheckDistance);
        // 위쪽 레이
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * wallCheckDistance);
        // 아래쪽 레이
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * wallCheckDistance);
    }
}
