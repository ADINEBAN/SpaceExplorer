using UnityEngine;

/// <summary>
/// Quản lý sao thưởng (Collectible Star): Trôi trong không gian, xoay phát sáng.
/// Thu thập bởi Player, đảm bảo chỉ cộng điểm ĐÚNG 1 LẦN cho mỗi ngôi sao.
/// </summary>
public class Star : MonoBehaviour
{
    private static float difficultyMultiplier = 1f;

    [Header("Score")]
    [SerializeField] private int scoreValue = 100;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.8f;
    [SerializeField] private float rotationSpeed = 60f;

    private bool isCollected;

    public static void SetDifficultyMultiplier(float multiplier)
    {
        difficultyMultiplier = Mathf.Max(0.5f, multiplier);
    }

    private void Update()
    {
        float speed = moveSpeed * difficultyMultiplier * GameLoadoutManager.CurrentDifficulty.starSpeed;

        // Trôi nhẹ sang trái
        transform.position += Vector3.left * speed * Time.deltaTime;

        // Xoay nhẹ nhàng tạo hiệu ứng lấp lánh
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        // Hủy khi trôi khỏi màn hình
        Camera cam = Camera.main;
        if (cam != null)
        {
            float leftEdge = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, 0f)).x;
            if (transform.position.x < leftEdge - 4f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Đảm bảo chỉ ăn đúng một lần
        if (isCollected)
            return;

        if (!other.CompareTag("Player"))
            return;

        isCollected = true;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
            ScoreManager.Instance.RecordStarCollected();
        }

        // Âm thanh nhặt sao
        SpaceGameAudio.PlayStarCollected(transform.position, 0.65f);

        // Hiệu ứng hạt phát sáng khi nhặt sao
        SpaceCombatVfx.SpawnHit(transform.position);

        Destroy(gameObject);
    }
}