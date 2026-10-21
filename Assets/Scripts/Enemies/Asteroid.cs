using UnityEngine;

/// <summary>
/// Quản lý chuyển động, máu, va chạm và hủy của tiểu hành tinh (Asteroid).
/// Có nhiều biến thể (Nhỏ, Vừa, Lớn), xoay ngẫu nhiên và thưởng điểm khi bị tiêu diệt.
/// </summary>
public class Asteroid : MonoBehaviour
{
    private static float difficultyMultiplier = 1f;

    public enum AsteroidSize { Small, Medium, Large }

    [Header("Variant & Stats")]
    [SerializeField] private AsteroidSize asteroidSize = AsteroidSize.Medium;
    [SerializeField] private int health = 1;
    [SerializeField] private int scoreValue = 25;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject explosionEffectPrefab;

    [Header("Movement")]
    [SerializeField] private float minSpeed = 5f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float verticalDrift = 2.0f;
    [SerializeField] private float rotationSpeed = 120f;

    private float speed;
    private float drift;
    private float rotationDirection;
    private bool destroyed;

    public static void SetDifficultyMultiplier(float multiplier)
    {
        difficultyMultiplier = Mathf.Max(0.5f, multiplier);
    }

    private void Start()
    {
        // Điều chỉnh chỉ số theo kích thước
        switch (asteroidSize)
        {
            case AsteroidSize.Small:
                health = 1;
                scoreValue = 25;
                minSpeed = 7f;
                maxSpeed = 11f;
                break;
            case AsteroidSize.Medium:
                health = 2;
                scoreValue = 50;
                minSpeed = 5f;
                maxSpeed = 8f;
                break;
            case AsteroidSize.Large:
                health = 3;
                scoreValue = 100;
                minSpeed = 3.5f;
                maxSpeed = 6f;
                break;
        }

        speed = Random.Range(minSpeed, maxSpeed);
        drift = Random.Range(-verticalDrift, verticalDrift);
        rotationDirection = Random.value < 0.5f ? -1f : 1f;
    }

    private void Update()
    {
        float speedMult = difficultyMultiplier * GameLoadoutManager.CurrentDifficulty.asteroidSpeedMult;

        // Trôi ổn định sang trái kèm trôi dọc tự nhiên về phía người chơi
        transform.position += new Vector3(
            -speed * speedMult,
            drift * speedMult,
            0f
        ) * Time.deltaTime;

        // Tự xoay 360 độ
        transform.Rotate(
            Vector3.forward,
            rotationDirection * rotationSpeed * speedMult * Time.deltaTime
        );

        // Hủy an toàn khi vượt khỏi cạnh trái màn hình
        Camera cam = Camera.main;
        if (cam != null)
        {
            float leftEdge = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, 0f)).x;
            if (transform.position.x < leftEdge - 6f)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Nhận sát thương từ đạn laser của người chơi
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (destroyed)
            return;

        health -= damage;

        if (health > 0)
        {
            SpawnEffect(hitEffectPrefab);
            return;
        }

        destroyed = true;

        // Cộng điểm một lần duy nhất
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
            ScoreManager.Instance.RecordAsteroidDestroyed();
        }

        // Âm thanh và hiệu ứng nổ
        SpaceGameAudio.PlayAsteroidDestroyed(transform.position, 0.6f);
        SpawnEffect(explosionEffectPrefab);

        float visualScale = Mathf.Clamp(transform.localScale.magnitude / 1.732f, 0.8f, 2.2f);
        SpaceCombatVfx.SpawnExplosion(transform.position, visualScale);

        Destroy(gameObject);
    }

    private void SpawnEffect(GameObject effectPrefab)
    {
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            ParticleSystem[] particles = effect.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem particle in particles)
            {
                particle.Play();
            }
        }
    }
}