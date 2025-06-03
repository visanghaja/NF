using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;

    [Header("References")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    private Transform player;
    private bool isAttacking = false;
    private float lastAttackTime;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (player == null) return;

        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 공격 범위 내에 있고 쿨다운이 지났다면 공격
        if (distanceToPlayer <= attackRange && !isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
        // 공격 범위 밖이면 플레이어를 추적
        else if (!isAttacking)
        {
            MoveTowardsPlayer();
        }

        // 플레이어를 향해 방향 전환
        UpdateFacingDirection();
    }

    private void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += new Vector3(direction.x, direction.y, 0) * moveSpeed * Time.deltaTime;
    }

    private void UpdateFacingDirection()
    {
        if (player == null) return;

        Vector2 direction = player.position - transform.position;
        spriteRenderer.flipX = direction.x < 0;
    }

    private void Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // 공격 애니메이션 재생
        if (animator != null)
        {
            animator.SetBool("Attack", true);
        }

        // 공격 상태 해제를 위한 코루틴 시작
        StartCoroutine(ResetAttackState());
    }

    private System.Collections.IEnumerator ResetAttackState()
    {
        // 공격 애니메이션 재생 시간만큼 대기 (예: 0.5초)
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;

        // 공격 애니메이션 종료
        if (animator != null)
        {
            animator.SetBool("Attack", false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 공격 범위 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
} 