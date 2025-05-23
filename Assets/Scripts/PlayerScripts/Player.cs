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

    // Public properties to access private fields
    public PlayerState CurrentState { get { return _currentState; } }
    public float HorizontalInput { get { return _horizontalInput; } }

    void Update()
    {
        PlayerMove();
    }

    void FixedUpdate()
    {
        Move();
        Debug.Log(_currentState);
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
        // Get horizontal input (left/right arrow keys)
        _horizontalInput = Input.GetAxisRaw("Horizontal");

        // Update sprite direction immediately when horizontal input changes
        if (_horizontalInput != 0 && _currentState != PlayerState.Damaged)
        {
            spriteRenderer.flipX = _horizontalInput < 0;
        }

        // 대시 중에는 다른 상태 변경을 하지 않음
        if (_currentState == PlayerState.Dash)
            return;

        // Handle attack input
        if (Input.GetKeyDown(KeyCode.Q) && !isAttacking)
        {
            Attack();
            return;  // 공격 중 다른 상태 변경 방지
        }

        // Handle jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
            return;  // 점프 후 다른 상태 변경 방지
        }

        // Update animation state only if not jumping or attacking
        if (_currentState != PlayerState.Jumping && _currentState != PlayerState.Attacking)
        {
            if (_horizontalInput != 0)
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
        // Apply horizontal movement
        Vector2 movement = new Vector2(_horizontalInput * moveSpeed, rb.linearVelocity.y);
        rb.linearVelocity = movement;
    }

    void Jump()
    {
        if (!isGrounded) return;  // 이미 공중에 있다면 점프하지 않음
        
        rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
        SetState(PlayerState.Jumping);  // 상태를 먼저 변경
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
