using System;
using UnityEngine;
using System.Collections;

public enum PlayerState
{
    Idle,
    Running,
    Jumping,
    Attacking,
    Damaged,
    Dash,
    Dead
}

public class Player : MonoBehaviour
{
    [Header("Settings")]
    public float JumpForce;
    public float moveSpeed;

    [Header("Reference")]
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Transform groundCheck;
    public LayerMask groundLayer;

    private PlayerState _currentState = PlayerState.Idle;
    private bool isGrounded = true;
    private bool isAttacking = false;
    private float _horizontalInput;
    private float _verticalInput;
    private Player_Wall.WallDirection _currentWallDirection = Player_Wall.WallDirection.None;
    private Vector2 jumpDirection = Vector2.up;
    private bool isFacingRight = true;

    // Public properties to access private fields
    public PlayerState CurrentState { get { return _currentState; } }
    public float HorizontalInput { get { return _horizontalInput; } }

    public void SetWallDirection(Player_Wall.WallDirection direction)
    {
        // 이전 벽 방향과 새로운 벽 방향이 다를 때만 처리
        if (_currentWallDirection != direction)
        {
            // 벽 방향이 변경될 때 상태 초기화
            if (_currentState != PlayerState.Jumping && _currentState != PlayerState.Attacking && _currentState != PlayerState.Dash)
            {
                // 이전 벽 방향이 좌우 벽이었고, 새로운 방향이 상하 벽이거나 None일 때
                if ((_currentWallDirection == Player_Wall.WallDirection.Left || _currentWallDirection == Player_Wall.WallDirection.Right) &&
                    (direction == Player_Wall.WallDirection.Top || direction == Player_Wall.WallDirection.Bottom || direction == Player_Wall.WallDirection.None))
                {
                    SetState(PlayerState.Idle);
                }
                // 이전 벽 방향이 상하 벽이었고, 새로운 방향이 좌우 벽이거나 None일 때
                else if ((_currentWallDirection == Player_Wall.WallDirection.Top || _currentWallDirection == Player_Wall.WallDirection.Bottom) &&
                         (direction == Player_Wall.WallDirection.Left || direction == Player_Wall.WallDirection.Right || direction == Player_Wall.WallDirection.None))
                {
                    SetState(PlayerState.Idle);
                }
                // 벽에서 완전히 벗어났을 때
                else if (direction == Player_Wall.WallDirection.None && isGrounded)
                {
                    SetState(PlayerState.Idle);
                }
            }

            _currentWallDirection = direction;
            
            // Update jump direction based on wall
            switch (direction)
            {
                case Player_Wall.WallDirection.Left:
                    jumpDirection = Vector2.right;
                    break;
                case Player_Wall.WallDirection.Right:
                    jumpDirection = Vector2.left;
                    break;
                case Player_Wall.WallDirection.Top:
                    jumpDirection = Vector2.down;
                    break;
                case Player_Wall.WallDirection.Bottom:
                    jumpDirection = Vector2.up;
                    break;
                default:
                    jumpDirection = Vector2.up;
                    break;
            }
        }
    }

    void Update()
    {
        PlayerMove();
    }

    void FixedUpdate()
    {
        Move();
        //Debug.Log(_currentState);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with ground layer
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
            if (_currentState == PlayerState.Jumping)
            {
                SetState(PlayerState.Idle);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // Check if the collision exit is with ground layer
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = false;
        }
    }

    void PlayerMove()
    {
        // Get input based on wall direction
        if (_currentWallDirection == Player_Wall.WallDirection.Left || _currentWallDirection == Player_Wall.WallDirection.Right)
        {
            // On side walls, use vertical input for movement
            _horizontalInput = Input.GetAxisRaw("Vertical");
            _verticalInput = Input.GetAxisRaw("Horizontal");
        }
        else
        {
            // Normal movement
            _horizontalInput = Input.GetAxisRaw("Horizontal");
            _verticalInput = Input.GetAxisRaw("Vertical");
        }

        // Update facing direction
        if (_horizontalInput != 0 && _currentState != PlayerState.Damaged)
        {
            if (_currentWallDirection == Player_Wall.WallDirection.Left || _currentWallDirection == Player_Wall.WallDirection.Right)
            {
                isFacingRight = _verticalInput > 0;
            }
            else
            {
                isFacingRight = _horizontalInput > 0;
            }
        }

        // 대시 중에는 다른 상태 변경을 하지 않음
        if (_currentState == PlayerState.Dash)
            return;

        // Handle attack input
        if (Input.GetKeyDown(KeyCode.Q) && !isAttacking)
        {
            Attack();
            return;
        }

        // Handle jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
            return;
        }

        // Update animation state
        if (_currentState != PlayerState.Jumping && _currentState != PlayerState.Attacking)
        {
            bool isMoving = false;
            
            // 벽 방향에 따라 실제 이동 여부 확인
            if (_currentWallDirection == Player_Wall.WallDirection.Left || _currentWallDirection == Player_Wall.WallDirection.Right)
            {
                isMoving = Mathf.Abs(_horizontalInput) > 0.1f; // 수직 이동이 있을 때만 이동으로 간주
            }
            else
            {
                isMoving = Mathf.Abs(_horizontalInput) > 0.1f; // 수평 이동이 있을 때만 이동으로 간주
            }

            if (isMoving)
            {
                SetState(PlayerState.Running);
            }
            else if (isGrounded)
            {
                SetState(PlayerState.Idle);
            }
        }
    }

    void Move()
    {
        Vector2 movement;
        if (_currentWallDirection == Player_Wall.WallDirection.Left || _currentWallDirection == Player_Wall.WallDirection.Right)
        {
            // On side walls, use vertical input for movement
            movement = new Vector2(rb.linearVelocity.x, _horizontalInput * moveSpeed);
        }
        else
        {
            // Normal movement
            movement = new Vector2(_horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
        rb.linearVelocity = movement;
    }

    void Jump()
    {
        if (!isGrounded) return;
        
        rb.AddForce(jumpDirection * JumpForce, ForceMode2D.Impulse);
        SetState(PlayerState.Jumping);
        isGrounded = false;
    }

    void Attack()
    {
        isAttacking = true;
        SetState(PlayerState.Attacking);
        
        // 공격 애니메이션이 끝나면 상태를 되돌리기 위해 코루틴 사용
        StartCoroutine(ResetAttackState());
    }

    System.Collections.IEnumerator ResetAttackState()
    {
        // 공격 애니메이션 재생 시간만큼 대기 (예: 0.5초)
        yield return new WaitForSeconds(0.5f);
        
        isAttacking = false;
        if (_currentState == PlayerState.Attacking)
        {
            // 공격이 끝난 후 상태 복귀
            if (_horizontalInput != 0)
            {
                SetState(PlayerState.Running);
            }
            else
            {
                SetState(PlayerState.Idle);
            }
        }
    }

    public void SetState(PlayerState newState)
    {
        if (_currentState == newState) return;

        _currentState = newState;
        
        // Update animator parameters
        if (animator != null)
        {
            // Set the State parameter to match the current state
            animator.SetInteger("State", (int)_currentState);
        }
    }
}
