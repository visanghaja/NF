using UnityEngine;
using System.Collections;

public class Player_Dash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashCooldown = 1f;
    public LayerMask wallLayer;

    [Header("AfterImage Effect")]
    public Color afterImageColor = new Color(1f, 1f, 1f, 0.5f);
    public float afterImageLifetime = 0.3f;
    public float afterImageSpawnRate = 0.05f;

    private Player playerScript;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool canDash = true;
    private bool isDashing = false;
    private Vector2 dashDirection;
    private float originalGravityScale;
    private Coroutine afterImageCoroutine;

    private void Start()
    {
        playerScript = GetComponent<Player>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        // 대시 중일 때 Shift를 다시 누르면 대시 취소
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (isDashing)
            {
                StopDash();
                return;
            }
            else if (canDash)
            {
                StartDash();
            }
        }

        // 대시 중 벽 체크
        if (isDashing)
        {
            CheckWallCollision();
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            // 대시 중일 때는 설정된 방향으로 계속 이동
            rb.linearVelocity = dashDirection * dashSpeed;
        }
    }

    private void StartDash()
    {
        isDashing = true;
        canDash = false;

        // 대시 방향 결정
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // 입력이 없으면 현재 바라보는 방향으로 대시
        if (horizontal == 0 && vertical == 0)
        {
            dashDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }
        else
        {
            // 입력 방향으로 대시
            dashDirection = new Vector2(horizontal, vertical).normalized;
        }

        // 중력 비활성화
        rb.gravityScale = 0f;

        // 잔상 효과 시작
        if (afterImageCoroutine != null)
        {
            StopCoroutine(afterImageCoroutine);
        }
        afterImageCoroutine = StartCoroutine(SpawnAfterImages());

        // 대시 쿨다운 시작
        StartCoroutine(DashCooldown());
    }

    private void StopDash()
    {
        isDashing = false;
        
        // 중력 복원
        rb.gravityScale = originalGravityScale;
        
        // 속도를 0으로 초기화하지 않고, 수평 속도만 감소
        float currentXVelocity = rb.linearVelocity.x * 0.5f; // 수평 속도를 절반으로 줄임
        rb.linearVelocity = new Vector2(currentXVelocity, rb.linearVelocity.y);

        // 잔상 효과 중지
        if (afterImageCoroutine != null)
        {
            StopCoroutine(afterImageCoroutine);
            afterImageCoroutine = null;
        }

        // 쿨다운 시작
        StartCoroutine(DashCooldown());
    }

    private IEnumerator SpawnAfterImages()
    {
        while (isDashing)
        {
            // 잔상 생성
            CreateAfterImage();
            yield return new WaitForSeconds(afterImageSpawnRate);
        }
    }

    private void CreateAfterImage()
    {
        // 잔상용 게임오브젝트 생성
        GameObject afterImage = new GameObject("DashAfterImage");
        afterImage.transform.position = transform.position;
        afterImage.transform.rotation = transform.rotation;
        afterImage.transform.localScale = transform.localScale;

        // 스프라이트 렌더러 추가 및 설정
        SpriteRenderer afterImageSprite = afterImage.AddComponent<SpriteRenderer>();
        afterImageSprite.sprite = spriteRenderer.sprite;
        afterImageSprite.flipX = spriteRenderer.flipX;
        afterImageSprite.color = afterImageColor;
        afterImageSprite.sortingLayerName = spriteRenderer.sortingLayerName;
        afterImageSprite.sortingOrder = spriteRenderer.sortingOrder - 1;

        // 페이드아웃 효과 시작
        StartCoroutine(FadeOutAfterImage(afterImageSprite));
    }

    private IEnumerator FadeOutAfterImage(SpriteRenderer afterImageSprite)
    {
        float elapsed = 0f;
        Color startColor = afterImageSprite.color;

        while (elapsed < afterImageLifetime)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / afterImageLifetime;
            
            // 알파값을 서서히 감소
            afterImageSprite.color = new Color(
                startColor.r,
                startColor.g,
                startColor.b,
                Mathf.Lerp(startColor.a, 0f, normalizedTime)
            );

            yield return null;
        }

        // 잔상 오브젝트 제거
        Destroy(afterImageSprite.gameObject);
    }

    private void CheckWallCollision()
    {
        // 현재 위치에서 대시 방향으로 레이캐스트
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dashDirection, 0.5f, wallLayer);
        
        if (hit.collider != null)
        {
            // 벽에 부딪혔을 때 약간의 반동 효과
            Vector2 wallNormal = hit.normal;
            rb.linearVelocity = wallNormal * dashSpeed * 0.2f; // 벽의 수직 방향으로 약한 반동
            StopDash();
        }
    }

    private IEnumerator DashCooldown()
    {
        canDash = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // 대시 중인지 확인하는 public 프로퍼티
    public bool IsDashing
    {
        get { return isDashing; }
    }

    // 수동으로 대시 중지 (다른 스크립트에서 호출 가능)
    public void ForceDashStop()
    {
        if (isDashing)
        {
            StopDash();
        }
    }
} 