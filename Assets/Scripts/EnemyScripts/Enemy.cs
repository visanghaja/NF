using UnityEngine;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float minDistanceToPlayer = 1f;  // 플레이어와의 최소 거리
    public float separationRadius = 1.5f;   // 다른 적과의 최소 거리
    public float separationWeight = 1f;     // 분리 힘의 가중치

    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rb;
    protected Transform player;
    protected static List<Enemy> allEnemies = new List<Enemy>();

    protected virtual void Start()
    {
        // 필요한 컴포넌트들 가져오거나 생성
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        
        // Rigidbody2D가 없다면 추가
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;         // 중력 영향 제거
            rb.freezeRotation = true;     // 회전 방지
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;  // 충돌 감지 모드 설정
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;      // 회전 제한
        }

        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Sorting Layer 설정
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = SortingLayerManager.ENEMY_LAYER;
            spriteRenderer.sortingOrder = SortingLayerManager.ENEMY_ORDER;
        }

        // 적 리스트에 자신 추가
        allEnemies.Add(this);
    }

    protected virtual void OnDestroy()
    {
        // 적 리스트에서 자신 제거
        allEnemies.Remove(this);
    }

    protected virtual void FixedUpdate()
    {
        if (player == null) return;

        // 플레이어 방향으로의 이동 벡터 계산
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 다른 적들로부터의 분리 벡터 계산
        Vector2 separationForce = CalculateSeparation();

        // 최종 이동 방향 계산
        Vector2 finalDirection = Vector2.zero;

        // 플레이어와의 거리가 최소 거리보다 크면 플레이어 방향으로 이동
        if (distanceToPlayer > minDistanceToPlayer)
        {
            finalDirection += directionToPlayer;
        }

        // 분리 힘 적용
        finalDirection += separationForce * separationWeight;

        // 방향 정규화
        if (finalDirection != Vector2.zero)
        {
            finalDirection.Normalize();
            
            // 이동 적용
            rb.linearVelocity = finalDirection * moveSpeed;

            // 스프라이트 방향 설정
            if (finalDirection.x != 0)
            {
                spriteRenderer.flipX = finalDirection.x < 0;
            }
        }
    }

    protected virtual Vector2 CalculateSeparation()
    {
        Vector2 separationForce = Vector2.zero;
        int neighborCount = 0;

        // 모든 다른 적들과의 거리 확인
        foreach (Enemy other in allEnemies)
        {
            if (other != this)
            {
                float distance = Vector2.Distance(transform.position, other.transform.position);

                // 분리 반경 내에 있는 경우
                if (distance < separationRadius)
                {
                    // 다른 적으로부터 멀어지는 벡터 계산
                    Vector2 awayFromOther = (Vector2)(transform.position - other.transform.position);
                    
                    // 거리가 가까울수록 더 강한 분리력 적용
                    awayFromOther = awayFromOther.normalized / Mathf.Max(0.1f, distance);
                    
                    separationForce += awayFromOther;
                    neighborCount++;
                }
            }
        }

        // 이웃이 있는 경우 평균 분리 벡터 계산
        if (neighborCount > 0)
        {
            separationForce /= neighborCount;
        }

        return separationForce;
    }

    // 디버그용 기즈모
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistanceToPlayer);
    }
} 