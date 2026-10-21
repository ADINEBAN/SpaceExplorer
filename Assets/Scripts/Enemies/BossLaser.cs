using UnityEngine;

/// <summary>
/// Đạn Laser sóng năng lượng của Boss (dùng asset energy_waves_04.png):
/// Bay theo quỹ đạo về phía người chơi, có hiệu ứng phát sáng neon và gây sát thương khi trúng đích.
/// </summary>
public class BossLaser : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifetime = 3.5f;
    [SerializeField] private int damage = 1;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer laserRenderer;

    private float lifeTimer;
    private Vector2 moveDirection = Vector2.left;
    private bool hasHit;
    private bool isSpinning;
    private float spinSpeed = 260f;

    private void Awake()
    {
        if (laserRenderer == null)
        {
            laserRenderer = GetComponent<SpriteRenderer>();
            if (laserRenderer == null)
            {
                laserRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        // Tự động gán Sprite energy_waves_05 nếu chưa có
        if (laserRenderer.sprite == null)
        {
            Sprite waveSprite = Resources.Load<Sprite>("Lasers/energy_waves_05");
            if (waveSprite == null)
            {
                waveSprite = Resources.Load<Sprite>("Lasers/energy_waves_04");
            }
            laserRenderer.sprite = waveSprite;
        }

        laserRenderer.sortingOrder = 18;
        laserRenderer.color = Color.white;

        // Tự động cấu hình Collider
        BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
        if (boxCol == null)
        {
            boxCol = gameObject.AddComponent<BoxCollider2D>();
        }
        boxCol.isTrigger = true;
        boxCol.size = new Vector2(1.6f, 1.3f);

        // Tự động cấu hình Rigidbody2D Kinematic
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        try
        {
            gameObject.tag = "BossLaser";
        }
        catch (System.Exception)
        {
            // Bỏ qua nếu Unity chưa đăng ký tag, hệ thống va chạm tự động nhận diện qua GetComponent<BossLaser>()
        }
    }

    private void Start()
    {
        lifeTimer = lifetime;
    }

    /// <summary>
    /// Khởi tạo hướng bay, vận tốc, sprite và hiệu ứng đặc thù của đạn Boss
    /// </summary>
    public void Initialize(
        Vector2 direction,
        float bulletSpeed = 25f,
        int bulletDamage = 1,
        Sprite customSprite = null,
        Color? tintColor = null,
        bool spin = false,
        float visualScale = 1.6f,
        Color? trailColor = null
    )
    {
        moveDirection = direction.normalized;
        speed = bulletSpeed;
        damage = bulletDamage;
        isSpinning = spin;

        if (customSprite != null && laserRenderer != null)
        {
            laserRenderer.sprite = customSprite;
        }

        transform.localScale = new Vector3(visualScale, visualScale, 1f);

        if (!isSpinning)
        {
            // Đạn sóng năng lượng lưỡi liềm: Xoay mặt sóng theo hướng bay
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        else
        {
            // Đạn hố đen / vật chất tối: xoay ngẫu nhiên lúc đầu
            transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        }

        if (tintColor.HasValue && laserRenderer != null)
        {
            laserRenderer.color = tintColor.Value;
        }
        else if (laserRenderer != null)
        {
            laserRenderer.color = Color.white;
        }

        SetupTrail(trailColor ?? (spin ? new Color(0.7f, 0.15f, 1f, 0.9f) : new Color(0.3f, 0.75f, 1f, 0.9f)));
    }

    private void SetupTrail(Color glowColor)
    {
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.14f;
            trail.startWidth = 0.55f;
            trail.endWidth = 0.05f;
            trail.material = laserRenderer != null ? laserRenderer.sharedMaterial : null;
            trail.sortingOrder = 17;
            trail.startColor = glowColor;
            trail.endColor = new Color(glowColor.r, glowColor.g, glowColor.b, 0f);
        }
        else
        {
            trail.startColor = glowColor;
            trail.endColor = new Color(glowColor.r, glowColor.g, glowColor.b, 0f);
        }
    }

    private void Update()
    {
        // Bay thẳng theo hướng đã định
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

        if (isSpinning)
        {
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        }

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        CheckBounds();
    }

    private void CheckBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 vp = cam.WorldToViewportPoint(transform.position);
        if (vp.x < -0.4f || vp.x > 1.4f || vp.y < -0.4f || vp.y > 1.4f)
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
        if (hasHit) return;

        // Tránh va chạm với Boss hoặc đạn Boss khác
        if (HasTag(other, "Boss") || other.GetComponent<Boss>() != null || other.GetComponent<BossLaser>() != null)
            return;

        if (other.CompareTag("Player") || other.GetComponent<PlayerCollision>() != null)
        {
            hasHit = true;
            PlayerCollision pc = other.GetComponent<PlayerCollision>();
            if (pc != null)
            {
                pc.TakeDamage(damage, transform.position);
            }
            SpaceCombatVfx.SpawnHit(transform.position);
            Destroy(gameObject);
        }
    }
}
