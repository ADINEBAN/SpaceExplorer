using UnityEngine;

/// <summary>
/// Quản lý sinh sao thưởng (Collectible Stars) theo chu kỳ,
/// điều chỉnh theo độ khó và giới hạn số lượng trên màn hình.
/// </summary>
public class StarSpawner : MonoBehaviour
{
    [Header("Star")]
    [SerializeField] private GameObject starPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float baseSpawnInterval = 3f;
    [SerializeField] private float rightPadding = 1.5f;
    [SerializeField] private float minViewportY = 0.15f;
    [SerializeField] private float maxViewportY = 0.85f;

    private Camera mainCamera;
    private float currentInterval;
    private float difficultyMultiplier = 1f;
    private float spawnTimer;

    private void Start()
    {
        mainCamera = Camera.main;
        baseSpawnInterval = GameLoadoutManager.CurrentDifficulty.starInterval;
        currentInterval = baseSpawnInterval;
        spawnTimer = 1.5f;
    }

    public void SetDifficultyMultiplier(float multiplier)
    {
        difficultyMultiplier = Mathf.Max(0.5f, multiplier);
        currentInterval = Mathf.Max(
            1.2f,
            GameLoadoutManager.CurrentDifficulty.starInterval / difficultyMultiplier
        );
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            TrySpawnStar();
            spawnTimer = currentInterval;
        }
    }

    private void TrySpawnStar()
    {
        if (mainCamera == null || starPrefab == null)
            return;

        // Giới hạn số sao xuất hiện cùng lúc trên màn hình (tối đa 6 ngôi sao)
        GameObject[] currentStars = GameObject.FindGameObjectsWithTag("Collectible");
        if (currentStars.Length >= 6)
            return;

        float randomY = Random.Range(minViewportY, maxViewportY);
        float cameraHeight = mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        float rightEdge = mainCamera.transform.position.x + cameraWidth;

        Vector3 spawnPosition = new Vector3(
            rightEdge + rightPadding,
            mainCamera.ViewportToWorldPoint(new Vector3(0f, randomY, 0f)).y,
            0f
        );

        Instantiate(starPrefab, spawnPosition, Quaternion.identity);
    }
}