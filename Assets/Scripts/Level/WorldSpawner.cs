using UnityEngine;

public class WorldSpawner : MonoBehaviour
{
    [Header("Asteroids")]
    [SerializeField] private GameObject[] asteroidPrefabs;

    [Header("Spawn")]
    [SerializeField] private float spawnInterval = 1.5f;

    [Header("Screen Spawn")]
    [SerializeField] private float rightPadding = 1.5f;
    [SerializeField] private float minViewportY = 0.15f;
    [SerializeField] private float maxViewportY = 0.85f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError(
                "Không tìm thấy Main Camera. " +
                "Hãy kiểm tra Tag của Camera phải là MainCamera."
            );

            return;
        }

        InvokeRepeating(
            nameof(SpawnAsteroid),
            1f,
            spawnInterval
        );
    }

    private void SpawnAsteroid()
    {
        if (asteroidPrefabs == null ||
            asteroidPrefabs.Length == 0)
            return;

        GameObject prefab =
            asteroidPrefabs[
                Random.Range(
                    0,
                    asteroidPrefabs.Length
                )
            ];

        float randomViewportY = Random.Range(
            minViewportY,
            maxViewportY
        );

        Vector3 viewportPosition = new Vector3(
            1.0f + rightPadding / 10f,
            randomViewportY,
            0f
        );

        Vector3 spawnPosition =
            mainCamera.ViewportToWorldPoint(
                viewportPosition
            );

        spawnPosition.z = 0f;

        Instantiate(
            prefab,
            spawnPosition,
            Quaternion.identity
        );
    }
}