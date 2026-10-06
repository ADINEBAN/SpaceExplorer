using UnityEngine;

public class StarSpawner : MonoBehaviour
{
    [Header("Star")]
    [SerializeField] private GameObject starPrefab;

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 3f;

    [Header("Screen Spawn")]
    [SerializeField] private float rightPadding = 1.5f;
    [SerializeField] private float minViewportY = 0.2f;
    [SerializeField] private float maxViewportY = 0.8f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError(
                "StarSpawner: Không tìm thấy MainCamera."
            );

            return;
        }

        InvokeRepeating(
            nameof(SpawnStar),
            2f,
            spawnInterval
        );
    }

    private void SpawnStar()
    {
        if (starPrefab == null)
            return;

        float randomY = Random.Range(
            minViewportY,
            maxViewportY
        );

        float cameraHeight =
            mainCamera.orthographicSize;

        float cameraWidth =
            cameraHeight * mainCamera.aspect;

        float rightEdge =
            mainCamera.transform.position.x +
            cameraWidth;

        Vector3 spawnPosition = new Vector3(
            rightEdge + rightPadding,
            mainCamera.ViewportToWorldPoint(
                new Vector3(
                    0f,
                    randomY,
                    0f
                )
            ).y,
            0f
        );

        Instantiate(
            starPrefab,
            spawnPosition,
            Quaternion.identity
        );
    }
}