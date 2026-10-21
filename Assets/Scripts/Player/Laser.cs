using UnityEngine;

/// <summary>
/// Quản lý đường đạn laser: Bay theo hướng ngắm 360 độ độc lập,
/// gây sát thương theo cấu hình vũ khí, tạo hiệu ứng va chạm và tự hủy an toàn.
/// </summary>
public class Laser : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed = 65f;
    [SerializeField] private float lifetime = 2.5f;
    [SerializeField] private int damage = 1;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer laserRenderer;

    private float lifeTimer;
    private bool hasHit;
    private Color projectileColor = Color.white;

    private void Awake()
    {
        if (laserRenderer == null)
        {
            laserRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    /// <summary>
    /// Khởi tạo thông số đạn laser từ Loadout vũ khí đã chọn
    /// </summary>
    public void Initialize(float bulletSpeed, int bulletDamage, Color color, Sprite customSprite = null)
    {
        speed = bulletSpeed;
        damage = bulletDamage;
        projectileColor = color;

        if (laserRenderer != null)
        {
            if (customSprite != null)
            {
                laserRenderer.sprite = customSprite;
                laserRenderer.color = Color.white;
            }
            else
            {
                laserRenderer.color = color;
            }
            laserRenderer.sortingOrder = 15;
        }

        // Tự động gắn TrailRenderer phát sáng neon nếu chưa có
        SetupTrail();
    }

    private void SetupTrail()
    {
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.08f;
            trail.startWidth = 0.35f;
            trail.endWidth = 0.02f;
            trail.material = laserRenderer != null ? laserRenderer.sharedMaterial : null;
            trail.sortingOrder = 14;
            trail.startColor = projectileColor;
            trail.endColor = new Color(projectileColor.r, projectileColor.g, projectileColor.b, 0f);
        }
    }

    private void Start()
    {
        lifeTimer = lifetime;
    }

    private void Update()
    {
        // Bay thẳng theo trục X cục bộ (hướng ngắm tại thời điểm bắn)
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        // Hủy khi bay quá xa ngoài tầm nhìn Camera
        CheckScreenBounds();
    }

    private void CheckScreenBounds()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;

        Vector3 viewportPoint = cam.WorldToViewportPoint(transform.position);
        if (viewportPoint.x < -0.3f || viewportPoint.x > 1.3f ||
            viewportPoint.y < -0.3f || viewportPoint.y > 1.3f)
        {
            Destroy(gameObject);
        }
    }

    private static bool HasTag(Collider2D other, string tagName)
    {
        return other != null && other.gameObject != null && other.gameObject.tag == tagName;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
            return;

        // Bỏ qua nếu chạm vào Player hoặc đạn khác
        if (other.CompareTag("Player") || other.GetComponent<Laser>() != null)
            return;

        // Va chạm với Boss
        Boss boss = other.GetComponent<Boss>();
        if (boss != null || HasTag(other, "Boss"))
        {
            hasHit = true;
            Vector3 impactPosition = other.ClosestPoint(transform.position);
            SpaceCombatVfx.SpawnHit(impactPosition);

            if (boss != null)
            {
                boss.TakeDamage(damage);
            }
            else
            {
                other.GetComponentInParent<Boss>()?.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Asteroid"))
        {
            hasHit = true;
            Vector3 impactPosition = other.ClosestPoint(transform.position);

            // Hiệu ứng tia lửa va chạm
            SpaceCombatVfx.SpawnHit(impactPosition);

            Asteroid asteroid = other.GetComponent<Asteroid>();
            if (asteroid != null)
            {
                asteroid.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}