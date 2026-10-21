using System.Collections;
using UnityEngine;

/// <summary>
/// Quản lý sinh mạng và xử lý va chạm của phi thuyền:
/// - Hệ thống 3 mạng (3 trái tim) kèm hiệu ứng bất tử chớp nháy (i-frames).
/// - Nhận sát thương khi va chạm Thiên Thạch (Asteroid), Boss hoặc Đạn Laser của Boss.
/// - Phát âm thanh, vfx va đập, trừ điểm và chuyển sang Game Over khi hết mạng.
/// </summary>
public class PlayerCollision : MonoBehaviour
{
    public static PlayerCollision Instance { get; private set; }

    [Header("Lives & Health")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float invincibilityDuration = 2.0f;
    [SerializeField] private float blinkInterval = 0.12f;

    [Header("Effects")]
    [SerializeField] private GameObject explosionEffectPrefab;

    private int currentLives;
    private bool isInvincible;
    private bool gameOver;
    private SpriteRenderer playerRenderer;
    private Coroutine invincibilityCoroutine;

    public int CurrentLives => currentLives;
    public int MaxLives => maxLives;
    public bool IsInvincible => isInvincible;
    public bool IsGameOver => gameOver;

    public event System.Action<int, int> OnLivesChanged; // (current, max)

    private void Awake()
    {
        Instance = this;
        currentLives = maxLives;
        playerRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        OnLivesChanged?.Invoke(currentLives, maxLives);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Xử lý mất 1 mạng khi trúng đòn
    /// </summary>
    public void TakeDamage(int amount = 1, Vector3 hitPosition = default)
    {
        if (gameOver || isInvincible)
            return;

        if (hitPosition == default)
        {
            hitPosition = transform.position;
        }

        currentLives = Mathf.Max(0, currentLives - amount);
        OnLivesChanged?.Invoke(currentLives, maxLives);

        // Trừ điểm phạt va chạm theo cấu hình độ khó
        int penalty = GameLoadoutManager.CurrentDifficulty.collisionPenalty;
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(-penalty);
        }

        // Hiệu ứng tia lửa va chạm
        SpaceCombatVfx.SpawnHit(hitPosition);

        if (currentLives > 0)
        {
            // Vẫn còn mạng: Bật khung bất tử chớp nháy (i-frames)
            SpaceGameAudio.PlayAsteroidDestroyed(hitPosition, 0.45f);
            if (invincibilityCoroutine != null)
            {
                StopCoroutine(invincibilityCoroutine);
            }
            invincibilityCoroutine = StartCoroutine(InvincibilityRoutine());
            Debug.Log($"[PlayerCollision] Tàu trúng đòn! Mạng còn lại: {currentLives}/{maxLives}");
        }
        else
        {
            // Hết 3 mạng: Game Over!
            Die();
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float elapsed = 0f;

        if (playerRenderer == null)
        {
            playerRenderer = GetComponent<SpriteRenderer>();
        }

        Color originalColor = playerRenderer != null ? playerRenderer.color : Color.white;
        Color blinkColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);

        bool toggle = false;
        while (elapsed < invincibilityDuration)
        {
            if (playerRenderer != null)
            {
                playerRenderer.color = toggle ? blinkColor : originalColor;
            }
            toggle = !toggle;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        if (playerRenderer != null)
        {
            playerRenderer.color = originalColor;
        }

        isInvincible = false;
        invincibilityCoroutine = null;
    }

    private void Die()
    {
        gameOver = true;

        SpaceGameAudio.PlayPlayerDestroyed(transform.position, 0.9f);

        // Tạo hiệu ứng nổ phi thuyền
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                explosionEffectPrefab,
                transform.position,
                Quaternion.identity
            );

            ParticleSystem[] particles = effect.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem particle in particles)
            {
                particle.Play();
            }

            Destroy(effect, 2.5f);
        }
        else
        {
            SpaceCombatVfx.SpawnExplosion(transform.position, 2.6f);
        }

        Debug.Log("[PlayerCollision] GAME OVER - Tàu đã cạn kiệt 3 mạng sống!");

        // Hiển thị Game Over UI
        GameOverUI gameOverUI = FindAnyObjectByType<GameOverUI>();
        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy GameOverUI trong Scene!");
        }

        // Ẩn Player
        gameObject.SetActive(false);
    }

    private static bool HasTag(Collider2D other, string tagName)
    {
        return other != null && other.gameObject != null && other.gameObject.tag == tagName;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gameOver || isInvincible)
            return;

        // 1. Va chạm Thiên thạch (Asteroid)
        if (other.CompareTag("Asteroid"))
        {
            Vector3 impactPos = other.ClosestPoint(transform.position);
            TakeDamage(1, impactPos);

            // Phá hủy hoặc làm nổ thiên thạch đã tông trúng
            Asteroid asteroid = other.GetComponent<Asteroid>();
            if (asteroid != null)
            {
                asteroid.TakeDamage(999);
            }
            return;
        }

        // 2. Va chạm Laser của Boss
        if (HasTag(other, "BossLaser") || other.GetComponent<BossLaser>() != null)
        {
            Vector3 impactPos = other.ClosestPoint(transform.position);
            TakeDamage(1, impactPos);
            Destroy(other.gameObject);
            return;
        }

        // 3. Va chạm thân thể Boss
        if (HasTag(other, "Boss") || other.GetComponent<Boss>() != null)
        {
            Vector3 impactPos = other.ClosestPoint(transform.position);
            TakeDamage(1, impactPos);
        }
    }
}