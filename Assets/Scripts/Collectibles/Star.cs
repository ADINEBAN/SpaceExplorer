using UnityEngine;

public class Star : MonoBehaviour
{
    [Header("Score")]
    [SerializeField] private int scoreValue = 100;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float rotationSpeed = 60f;

    private void Update()
    {
        // Star trôi nhẹ sang trái
        transform.position +=
            Vector3.left *
            moveSpeed *
            Time.deltaTime;

        // Xoay nhẹ
        transform.Rotate(
            Vector3.forward,
            rotationSpeed * Time.deltaTime
        );

        Camera cam = Camera.main;

        if (cam != null)
        {
            float leftEdge =
                cam.ViewportToWorldPoint(
                    new Vector3(0f, 0.5f, 0f)
                ).x;

            if (transform.position.x < leftEdge - 3f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }

        Destroy(gameObject);
    }
}