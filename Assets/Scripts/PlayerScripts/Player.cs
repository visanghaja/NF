using System;
using UnityEngine;

public enum PlayerState
{
    Idle,
    Running,
    Jumping,
    Attacking,
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

    private PlayerState currentState = PlayerState.Idle;
    private bool isGrounded = true;
    private bool isAttacking = false;
    private float horizontalInput;

    void Update()
    {
        PlayerMove();
    }

    void FixedUpdate()
    {
        Move();
        Debug.Log(isGrounded);
        Debug.Log(currentState);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with ground layer
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
            if (currentState == PlayerState.Jumping)
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
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Handle jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
            return;  // 점프 후 다른 상태 변경 방지
        }

        // Update animation state only if not jumping
        if (currentState != PlayerState.Jumping)
        {
            if (horizontalInput != 0)
            {
                SetState(PlayerState.Running);
                spriteRenderer.flipX = horizontalInput < 0;
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
        Vector2 movement = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        rb.linearVelocity = movement;
    }

    void Jump()
    {
        if (!isGrounded) return;  // 이미 공중에 있다면 점프하지 않음
        
        rb.AddForce(Vector2.up * JumpForce, ForceMode2D.Impulse);
        SetState(PlayerState.Jumping);  // 상태를 먼저 변경
        isGrounded = false;
    }

    public void SetState(PlayerState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        
        // Update animator parameters
        if (animator != null)
        {
            // Set the State parameter to match the current state
            animator.SetInteger("State", (int)currentState);
            
        }
    }
}
