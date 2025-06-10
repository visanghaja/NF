using UnityEngine;
using System.Collections;

public class Player_Dash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeedMultiplier = 2f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 1f;
    
    [Header("AfterImage Settings")]
    public Color afterImageColor = new Color(1f, 1f, 1f, 0.5f);
    public float afterImageLifetime = 0.5f;
    public float afterImageSpawnRate = 0.05f;

    private Player playerScript;
    private SpriteRenderer spriteRenderer;
    private bool canDash = true;
    private bool isDashing = false;
    private float originalMoveSpeed;

    private void Start()
    {
        playerScript = GetComponent<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMoveSpeed = playerScript.moveSpeed;
    }

    private void Update()
    {
        // Dash input check
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !isDashing)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    private IEnumerator DashCoroutine()
    {
        // Start dash
        isDashing = true;
        canDash = false;
        playerScript.SetState(PlayerState.Dash);
        playerScript.moveSpeed *= dashSpeedMultiplier;

        // Start spawning afterimages
        StartCoroutine(SpawnAfterImages());

        // Wait for dash duration
        yield return new WaitForSeconds(dashDuration);

        // End dash
        isDashing = false;
        playerScript.moveSpeed = originalMoveSpeed;
        
        if (playerScript.CurrentState == PlayerState.Dash)
        {
            if (playerScript.HorizontalInput != 0)
                playerScript.SetState(PlayerState.Running);
            else
                playerScript.SetState(PlayerState.Idle);
        }

        // Cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private IEnumerator SpawnAfterImages()
    {
        while (isDashing)
        {
            CreateAfterImage();
            yield return new WaitForSeconds(afterImageSpawnRate);
        }
    }

    private void CreateAfterImage()
    {
        // Create a new GameObject for the afterimage
        GameObject afterImage = new GameObject("AfterImage");
        afterImage.transform.position = transform.position;
        afterImage.transform.rotation = transform.rotation;
        afterImage.transform.localScale = transform.localScale;

        // Add SpriteRenderer and copy properties
        SpriteRenderer afterImageSprite = afterImage.AddComponent<SpriteRenderer>();
        afterImageSprite.sprite = spriteRenderer.sprite;
        afterImageSprite.flipX = spriteRenderer.flipX;
        afterImageSprite.color = afterImageColor;
        afterImageSprite.sortingOrder = spriteRenderer.sortingOrder - 1;

        // Destroy after lifetime
        Destroy(afterImage, afterImageLifetime);
    }
} 