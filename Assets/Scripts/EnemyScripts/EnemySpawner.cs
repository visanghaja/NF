using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject meleeEnemyPrefab;    // 근접 공격 적 프리팹
    public GameObject rangedEnemyPrefab;   // 원거리 공격 적 프리팹
    public GameObject bossPrefab;          // 보스 프리팹

    [Header("Spawn Settings")]
    public float minSpawnDistance = 12f;   // 최소 스폰 거리 (카메라 밖)
    public float maxSpawnDistance = 15f;   // 최대 스폰 거리
    public float initialSpawnRate = 2f;    // 초기 스폰 간격 (초)
    public float minimumSpawnRate = 0.5f;  // 최소 스폰 간격 (초)
    public float spawnRateDecreaseTime = 30f; // 스폰 간격이 감소하는 시간 (초)
    public float bossSpawnTime = 600f;     // 보스 출현 시간 (10분)

    [Header("Enemy Ratio")]
    [Range(0, 1)]
    public float rangedEnemyRatio = 0.3f;  // 원거리 적 생성 비율 (0.3 = 30%)

    private Camera mainCamera;
    private Transform player;
    private float currentSpawnRate;
    private float gameTime = 0f;
    private bool isBossSpawned = false;

    private void Start()
    {
        mainCamera = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentSpawnRate = initialSpawnRate;
        StartCoroutine(SpawnEnemies());
    }

    private void Update()
    {
        gameTime += Time.deltaTime;

        // 시간에 따라 스폰 간격 감소
        if (gameTime < spawnRateDecreaseTime)
        {
            float normalizedTime = gameTime / spawnRateDecreaseTime;
            currentSpawnRate = Mathf.Lerp(initialSpawnRate, minimumSpawnRate, normalizedTime);
        }

        // 보스 스폰 체크
        if (!isBossSpawned && gameTime >= bossSpawnTime)
        {
            SpawnBoss();
            isBossSpawned = true;
        }
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(currentSpawnRate);
        }
    }

    private void SpawnEnemy()
    {
        // 랜덤 각도 생성 (0-360도)
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        
        // 랜덤 거리 생성 (최소-최대 거리 사이)
        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
        
        // 스폰 위치 계산 (플레이어 기준 원형)
        Vector2 spawnPosition = new Vector2(
            player.position.x + Mathf.Cos(randomAngle) * randomDistance,
            player.position.y + Mathf.Sin(randomAngle) * randomDistance
        );

        // 적 타입 결정 (근접 or 원거리)
        GameObject enemyPrefab = (Random.value < rangedEnemyRatio) ? rangedEnemyPrefab : meleeEnemyPrefab;
        
        // 적 생성
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }

    private void SpawnBoss()
    {
        // 보스는 항상 화면 오른쪽에서 생성
        Vector2 bossSpawnPosition = new Vector2(
            player.position.x + maxSpawnDistance,
            player.position.y
        );

        // 보스 생성
        Instantiate(bossPrefab, bossSpawnPosition, Quaternion.identity);
    }

    // 디버그용 기즈모 (에디터에서만 표시)
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, minSpawnDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, maxSpawnDistance);
    }
} 