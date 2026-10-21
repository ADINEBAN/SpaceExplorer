using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Quản lý bắn laser: Bắn bằng Chuột Trái (Left Click) hoặc Phím Space.
/// Hỗ trợ ngắm 360 độ, bắn liên tục với fire rate có thể chỉnh,
/// tích hợp 4 loại vũ khí laser từ Loadout.
/// </summary>
public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Fire Rate")]
    [SerializeField] private float shootCooldown = 0.16f;

    [Header("Sound")]
    [SerializeField, Range(0f, 1f)] private float shootingVolume = 0.35f;

    private float baseShootCooldown;
    private float shootTimer;
    private AudioSource shootingAudioSource;
    private Camera mainCamera;
    private Sprite levelLaserSprite;
    private GameLoadoutManager.LaserData currentLaserConfig;

    private void Awake()
    {
        baseShootCooldown = shootCooldown;
        shootingAudioSource = GetComponent<AudioSource>();

        if (shootingAudioSource == null)
        {
            shootingAudioSource = gameObject.AddComponent<AudioSource>();
        }

        shootingAudioSource.playOnAwake = false;
        shootingAudioSource.spatialBlend = 0f;

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindAnyObjectByType<Camera>();
        }

        // Tự động tìm FirePoint nếu chưa được gán
        if (firePoint == null)
        {
            Transform found = transform.Find("FirePoint");
            firePoint = found != null ? found : transform;
        }
    }

    private void Start()
    {
        ApplyLaserLoadout();
    }

    /// <summary>
    /// Áp dụng vũ khí laser đã chọn từ Loadout
    /// </summary>
    public void ApplyLaserLoadout()
    {
        currentLaserConfig = GameLoadoutManager.CurrentLaser;
        if (currentLaserConfig != null)
        {
            shootCooldown = currentLaserConfig.fireCooldown;
            baseShootCooldown = shootCooldown;

            if (!string.IsNullOrEmpty(currentLaserConfig.spriteResourcePath))
            {
                levelLaserSprite = Resources.Load<Sprite>(currentLaserConfig.spriteResourcePath);
            }
        }
    }

    private void Update()
    {
        shootTimer -= Time.deltaTime;

        bool isFiring = false;

        // Phím Space
        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
        {
            isFiring = true;
        }

        // Chuột Trái (Left Mouse Button)
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            isFiring = true;
        }

        // GIỮ NÚT = BẮN LIÊN TỤC VỚI COOLDOWN
        if (isFiring && shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootCooldown;
        }
    }

    /// <summary>
    /// Bắn laser theo đúng góc ngắm 360 độ hiện tại
    /// </summary>
    private void Shoot()
    {
        if (laserPrefab == null || firePoint == null)
            return;

        Vector3 spawnPos = firePoint.position;
        float aimAngle = 0f;

        // Tính hướng ngắm tới con trỏ chuột
        if (mainCamera != null && Mouse.current != null)
        {
            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(
                new Vector3(mouseScreen.x, mouseScreen.y, Mathf.Abs(mainCamera.transform.position.z))
            );
            Vector2 dir = (Vector2)mouseWorld - (Vector2)spawnPos;
            if (dir.sqrMagnitude > 0.01f)
            {
                aimAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            }
            else
            {
                // Dự phòng: dùng hướng quay của phi thuyền
                aimAngle = transform.eulerAngles.z + 90f;
            }
        }
        else
        {
            aimAngle = transform.eulerAngles.z + 90f;
        }

        Quaternion rotation = Quaternion.Euler(0f, 0f, aimAngle);

        GameObject laserObj = Instantiate(laserPrefab, spawnPos, rotation);

        // Cấu hình Laser
        Laser laserScript = laserObj.GetComponent<Laser>();
        if (laserScript == null)
        {
            laserScript = laserObj.AddComponent<Laser>();
        }

        float bulletSpeed = currentLaserConfig != null ? currentLaserConfig.projectileSpeed : 35f;
        int damage = currentLaserConfig != null ? currentLaserConfig.damage : 1;
        Color color = currentLaserConfig != null ? currentLaserConfig.laserColor : Color.cyan;

        laserScript.Initialize(bulletSpeed, damage, color, levelLaserSprite);

        // Hiệu ứng Muzzle Flash tại đầu nòng
        SpaceCombatVfx.SpawnMuzzleFlash(spawnPos, aimAngle, color);

        // Phát âm thanh tiếng bắn (kèm cao độ tùy loại laser)
        if (shootingAudioSource != null)
        {
            shootingAudioSource.pitch = currentLaserConfig != null ? currentLaserConfig.audioPitch : 1f;
        }
        SpaceGameAudio.PlayLaserShot(shootingAudioSource, shootingVolume);
    }

    public void SetLevelLaserSprite(Sprite sprite)
    {
        if (sprite != null)
        {
            levelLaserSprite = sprite;
        }
    }

    public void SetLevel(int level)
    {
        float fireRateMultiplier = 1f + (Mathf.Max(1, level) - 1) * 0.15f;
        shootCooldown = Mathf.Max(0.08f, baseShootCooldown / fireRateMultiplier);
    }
}