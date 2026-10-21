using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý sinh tiểu hành tinh (Asteroids) trong màn chơi:
/// Áp dụng cấu hình Độ Khó (Easy, Normal, Hard, Nightmare),
/// giới hạn số lượng tối đa đồng thời để giữ 60+ FPS và tránh spawn đè lên Player.
/// </summary>
public class WorldSpawner : MonoBehaviour
{
    [Header("Asteroids")]
    [SerializeField] private GameObject[] asteroidPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private float baseSpawnInterval = 1.5f;
    [SerializeField] private float rightPadding = 1.5f;
    [SerializeField] private float minViewportY = 0.12f;
    [SerializeField] private float maxViewportY = 0.88f;

    private Camera mainCamera;
    private float currentInterval;
    private float difficultyMultiplier = 1f;
    private float spawnTimer;
    private PlayerController player;

    public bool IsSpawningActive { get; set; } = true;

    private void Start()
    {
        mainCamera = Camera.main;
        player = FindAnyObjectByType<PlayerController>();

        // Thiết lập nhịp spawn dựa trên độ khó đã chọn
        baseSpawnInterval = GameLoadoutManager.CurrentDifficulty.spawnInterval;
        currentInterval = baseSpawnInterval;
        spawnTimer = 0.8f;
    }

    public void SetDifficultyMultiplier(float multiplier)
    {
        difficultyMultiplier = Mathf.Max(0.5f, multiplier);
        currentInterval = Mathf.Max(
            0.35f,
            GameLoadoutManager.CurrentDifficulty.spawnInterval / difficultyMultiplier
        );
    }

    public void ClearExistingAsteroids()
    {
        GameObject[] currentAsteroids = GameObject.FindGameObjectsWithTag("Asteroid");
        foreach (GameObject ast in currentAsteroids)
        {
            if (ast != null)
            {
                Destroy(ast);
            }
        }
    }

    private void Update()
    {
        if (!IsSpawningActive)
            return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            TrySpawnAsteroid();
            spawnTimer = currentInterval;
        }
    }

    private void TrySpawnAsteroid()
    {
        if (mainCamera == null)
            return;

        // Giới hạn số lượng thiên thạch đồng thời trên màn hình theo độ khó
        int maxAllowed = GameLoadoutManager.CurrentDifficulty.maxSimultaneousAsteroids;
        GameObject[] currentAsteroids = GameObject.FindGameObjectsWithTag("Asteroid");
        if (currentAsteroids.Length >= maxAllowed)
        {
            return; // Đã đạt giới hạn tối đa, không spawn thêm để tránh nghẽn màn hình và tụt FPS
        }

        if (asteroidPrefabs == null || asteroidPrefabs.Length == 0)
            return;

        // Chọn ngẫu nhiên prefab
        GameObject prefab = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];

        // Tính vị trí spawn bên ngoài mép phải camera
        float randomViewportY = Random.Range(minViewportY, maxViewportY);
        Vector3 viewportPos = new Vector3(1.0f + rightPadding / 10f, randomViewportY, 0f);
        Vector3 spawnPosition = mainCamera.ViewportToWorldPoint(viewportPos);
        spawnPosition.z = 0f;

        // Kiểm tra an toàn: không spawn quá gần người chơi
        if (player != null)
        {
            float distToPlayer = Vector2.Distance(spawnPosition, player.transform.position);
            if (distToPlayer < 4.5f)
            {
                // Dịch chuyển vị trí spawn ra xa người chơi
                spawnPosition.y += (spawnPosition.y > player.transform.position.y) ? 3f : -3f;
            }
        }

        GameObject spawned = Instantiate(prefab, spawnPosition, Quaternion.identity);

        // Biến thiên ngẫu nhiên tỉ lệ kích thước (Small, Medium, Large)
        float sizeFactor = Random.Range(0.75f, 1.4f);
        spawned.transform.localScale *= sizeFactor;
    }
}