using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Trùm cuối game (End-Game Boss Mothership):
/// - Dùng asset Art/Boss/boss.png và laser energy_waves_04.png.
/// - Hệ thống máu hiển thị thanh máu Sci-Fi trên đỉnh màn hình, tự chia 2 Phase chiến đấu.
/// - Tấn công bằng chùm sóng năng lượng tỏa hình nón và bắn liên hoàn nhắm vào phi thuyền.
/// - Chuỗi nổ đa tầng hoành tráng khi bị tiêu diệt và mở màn hình Victory.
/// </summary>
public class Boss : MonoBehaviour
{
    public static Boss Instance { get; private set; }

    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float attackInterval = 2.2f;

    [Header("Laser Projectile")]
    [SerializeField] private GameObject bossLaserPrefab;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer bossRenderer;

    private int currentHealth;
    private bool isEntering = true;
    private bool isDead;
    private bool isEnraged;
    private float attackTimer;
    private Camera mainCamera;
    private PlayerController player;
    private Vector3 targetCombatPosition;
    private float verticalPatrolDirection = 1f;
    private float originalY;

    // UI Elements
    private GameObject bossBarContainer;
    private RectTransform bossBarRect;
    private Vector2 bossBarOriginalPos;
    private RectTransform healthBarFillRect;
    private RectTransform healthBarGhostRect;
    private Image healthBarFill;
    private Image healthBarGhost;
    private float currentHealthFill = 1f;
    private float ghostHealthFill = 1f;
    private float targetHealthFill = 1f;
    private Coroutine barShakeCoroutine;
    private TMP_Text healthBarText;
    private GameObject warningBanner;

    // Laser Sprites (4 đạn: 2 ngoài dark_matter_07, 2 trong energy_waves_05)
    private Sprite outerLaserSprite;
    private Sprite innerLaserSprite;

    private void Awake()
    {
        Instance = this;

        if (bossRenderer == null)
        {
            bossRenderer = GetComponent<SpriteRenderer>();
            if (bossRenderer == null)
            {
                bossRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        // Tự động nạp sprite boss nếu chưa gán
        if (bossRenderer.sprite == null)
        {
            Sprite loadedSprite = Resources.Load<Sprite>("Boss/boss");
            if (loadedSprite != null)
            {
                bossRenderer.sprite = loadedSprite;
            }
        }

        // Nạp 2 loại sprite đạn của Boss
        outerLaserSprite = Resources.Load<Sprite>("Lasers/dark_matter_07");
        innerLaserSprite = Resources.Load<Sprite>("Lasers/energy_waves_05");
        if (innerLaserSprite == null)
        {
            innerLaserSprite = Resources.Load<Sprite>("Lasers/energy_waves_04");
        }

        bossRenderer.sortingOrder = 14;

        // Xoay 90 độ để mũi pháo và móng vuốt hướng thẳng sang trái về phía Player
        transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        transform.localScale = new Vector3(2.2f, 2.2f, 1f);

        // Cấu hình Collider bọc thân tàu Boss
        PolygonCollider2D polyCol = GetComponent<PolygonCollider2D>();
        if (polyCol == null)
        {
            BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
            if (boxCol == null)
            {
                boxCol = gameObject.AddComponent<BoxCollider2D>();
            }
            boxCol.isTrigger = true;
            boxCol.size = new Vector2(4.5f, 4.0f);
        }

        // Rigidbody2D Kinematic
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        try
        {
            gameObject.tag = "Boss";
        }
        catch (System.Exception)
        {
            // Bỏ qua nếu Unity chưa đăng ký tag, hệ thống va chạm tự động nhận diện qua GetComponent<Boss>()
        }

        // Thiết lập máu theo độ khó
        ConfigureDifficulty();
    }

    private void ConfigureDifficulty()
    {
        int diffIndex = GameLoadoutManager.SelectedDifficultyIndex;
        maxHealth = diffIndex switch
        {
            0 => 75,   // Easy
            1 => 110,  // Normal
            2 => 160,  // Hard
            _ => 240   // Nightmare
        };
        currentHealth = maxHealth;
        currentHealthFill = 1f;
        ghostHealthFill = 1f;
        targetHealthFill = 1f;
        attackInterval = diffIndex switch
        {
            0 => 2.6f,
            1 => 2.0f,
            2 => 1.5f,
            _ => 1.1f
        };
    }

    private void Start()
    {
        mainCamera = Camera.main;
        player = FindAnyObjectByType<PlayerController>();
        originalY = transform.position.y;
        attackTimer = 1.5f;

        SetupThrusters();
        CreateBossHealthUI();
        StartCoroutine(EntranceRoutine());
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (bossBarContainer != null)
        {
            Destroy(bossBarContainer);
        }
        if (warningBanner != null)
        {
            Destroy(warningBanner);
        }
    }

    private void SetupThrusters()
    {
        // Gắn 2 luồng phản lực đuôi Boss phát sáng
        CreateEngineTrail(new Vector3(1.2f, -1.2f, 0f));
        CreateEngineTrail(new Vector3(-1.2f, -1.2f, 0f));
    }

    private void CreateEngineTrail(Vector3 localPos)
    {
        GameObject trailObj = new GameObject("BossThruster");
        trailObj.transform.SetParent(transform, false);
        trailObj.transform.localPosition = localPos;

        TrailRenderer trail = trailObj.AddComponent<TrailRenderer>();
        trail.time = 0.35f;
        trail.startWidth = 0.7f;
        trail.endWidth = 0.05f;
        trail.material = bossRenderer != null ? bossRenderer.sharedMaterial : null;
        trail.sortingOrder = 13;
        Color col = new Color(1f, 0.45f, 0.1f, 0.9f);
        trail.startColor = col;
        trail.endColor = new Color(col.r, col.g, col.b, 0f);
    }

    private IEnumerator EntranceRoutine()
    {
        isEntering = true;

        // Bật cảnh báo Warning
        if (warningBanner != null)
        {
            warningBanner.SetActive(true);
        }

        // Vị trí chiến đấu: mép phải màn hình (khoảng 80% chiều rộng viewport)
        if (mainCamera != null)
        {
            float targetX = mainCamera.ViewportToWorldPoint(new Vector3(0.80f, 0.5f, 0f)).x;
            float startX = mainCamera.ViewportToWorldPoint(new Vector3(1.35f, 0.5f, 0f)).x;
            transform.position = new Vector3(startX, originalY, 0f);

            float t = 0f;
            while (t < 2.5f)
            {
                t += Time.deltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, t / 2.5f);
                float curTargetX = mainCamera.ViewportToWorldPoint(new Vector3(0.80f, 0.5f, 0f)).x;
                float currentX = Mathf.Lerp(startX, curTargetX, progress);
                transform.position = new Vector3(currentX, originalY, 0f);
                yield return null;
            }
        }

        if (warningBanner != null)
        {
            warningBanner.SetActive(false);
        }

        if (bossBarContainer != null)
        {
            bossBarContainer.SetActive(true);
        }

        isEntering = false;
    }

    private void Update()
    {
        if (isDead) return;

        // Cập nhật thanh máu tụt dần tương ứng mượt mà khi nhận sát thương
        AnimateHealthBar();

        // Giữ vị trí Boss theo camera di chuyển
        MaintainCameraAnchor();

        if (isEntering) return;

        // Di chuyển lượn sóng dọc theo màn hình
        PatrolVertical();

        // Đếm nhịp tấn công
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            ExecuteAttack();
            attackTimer = isEnraged ? attackInterval * 0.65f : attackInterval;
        }
    }

    private void MaintainCameraAnchor()
    {
        if (mainCamera == null) return;

        // Neo Boss ở khoảng 80% từ mép trái màn hình (phía trước người chơi)
        float desiredX = mainCamera.ViewportToWorldPoint(new Vector3(0.80f, 0.5f, 0f)).x;
        Vector3 pos = transform.position;
        pos.x = Mathf.MoveTowards(pos.x, desiredX, 25f * Time.deltaTime);
        transform.position = pos;
    }

    private void PatrolVertical()
    {
        if (mainCamera == null) return;

        float topY = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.85f, 0f)).y;
        float bottomY = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.15f, 0f)).y;

        // Theo dõi nhẹ vị trí Y của Player
        float targetY = transform.position.y + verticalPatrolDirection * moveSpeed * (isEnraged ? 1.4f : 1.0f) * Time.deltaTime;

        if (targetY > topY)
        {
            targetY = topY;
            verticalPatrolDirection = -1f;
        }
        else if (targetY < bottomY)
        {
            targetY = bottomY;
            verticalPatrolDirection = 1f;
        }

        Vector3 pos = transform.position;
        pos.y = targetY;
        transform.position = pos;
    }

    private void ExecuteAttack()
    {
        int attackType = Random.Range(0, isEnraged ? 3 : 2);

        switch (attackType)
        {
            case 0:
                // Bắn 1 loạt 4 đạn (2 ngoài dark_matter_07, 2 trong energy_waves_05)
                StartCoroutine(AttackSingleQuad());
                break;
            case 1:
                // Bắn liên hoàn 2 đợt x 4 đạn
                StartCoroutine(AttackDoubleQuad());
                break;
            case 2:
                // Phase 2: Bão đạn 3 đợt x 4 đạn dồn dập
                StartCoroutine(AttackBurstQuad(3));
                break;
        }
    }

    private IEnumerator AttackSingleQuad()
    {
        FireQuadSalvo(1.0f);
        yield return null;
    }

    private IEnumerator AttackDoubleQuad()
    {
        // Loạt 1: Bắn thẳng trực diện vào vị trí hiện tại của Player
        FireQuadSalvo(1.0f);
        yield return new WaitForSeconds(0.35f);
        if (!isDead)
        {
            // Loạt 2: Nhắm bắn bồi tiếp tục (dù đứng yên hay di chuyển đều bị dí theo)
            Vector2 lead = Vector2.zero;
            if (player != null)
            {
                lead = player.Velocity * 0.25f;
            }
            FireQuadSalvo(1.05f, lead);
        }
    }

    private IEnumerator AttackBurstQuad(int bursts)
    {
        for (int b = 0; b < bursts; b++)
        {
            if (isDead) yield break;
            Vector2 lead = Vector2.zero;
            if (b > 0 && player != null)
            {
                lead = player.Velocity * 0.22f;
            }
            FireQuadSalvo(1.0f + b * 0.08f, lead);
            yield return new WaitForSeconds(0.25f);
        }
    }

    /// <summary>
    /// Bắn loạt 4 đạn theo yêu cầu:
    /// - 2 đạn ở ngoài: dùng asset dark_matter_07.png (vật chất tối xoay tròn ma mị, quét cánh trên/dưới)
    /// - 2 đạn ở trong: dùng asset energy_waves_05.png (sóng năng lượng tím ngọc, nhắm thẳng trực diện phi thuyền)
    /// Khi Player đứng yên một chỗ, 2 đạn sóng năng lượng ở giữa CHẮC CHẮN SẼ BẮN TRÚNG!
    /// </summary>
    private void FireQuadSalvo(float spreadMult = 1.0f, Vector2? aimLead = null)
    {
        Vector2 targetCenter = (Vector2)transform.position + Vector2.left * 35f;

        if (player != null && player.gameObject.activeSelf)
        {
            targetCenter = (Vector2)player.transform.position;
            if (aimLead.HasValue)
            {
                targetCenter += aimLead.Value;
            }
        }

        float speed = isEnraged ? 30f : 25f;

        // Cấu hình 4 đạn theo tọa độ mục tiêu:
        // Đạn 0 (Ngoài cùng trên): dark_matter_07 - quét cánh trên (+3.0f) để chặn người chơi né lên
        // Đạn 1 (Ở trong trên): energy_waves_05 - nhắm trực diện thân tàu (+0.4f)
        // Đạn 2 (Ở trong dưới): energy_waves_05 - nhắm trực diện thân tàu (-0.4f)
        // Đạn 3 (Ngoài cùng dưới): dark_matter_07 - quét cánh dưới (-3.0f) để chặn người chơi né xuống
        //
        // targetYOffsets: Độ lệch đích đến tại vị trí X của Player
        // Hai đạn giữa nhắm chính xác vào thân tàu (+0.4f và -0.4f) nên nếu đứng yên CHẮC CHẮN SẼ BỊ BẮN TRÚNG!
        float[] targetYOffsets = new float[]
        {
            +3.0f * spreadMult,
            +0.4f * spreadMult,
            -0.4f * spreadMult,
            -3.0f * spreadMult
        };

        // Vị trí nòng pháo trên thân tàu Boss
        float[] muzzleYOffsets = new float[] { +1.5f, +0.5f, -0.5f, -1.5f };

        for (int i = 0; i < 4; i++)
        {
            bool isOuter = (i == 0 || i == 3);
            Vector3 spawnPos = transform.position + new Vector3(-1.9f, muzzleYOffsets[i], 0f);

            // Điểm đích đến thực tế của viên đạn tại tọa độ X của người chơi
            Vector2 bulletTarget = targetCenter + new Vector2(0f, targetYOffsets[i]);
            Vector2 dir = (bulletTarget - (Vector2)spawnPos).normalized;

            Sprite bulletSprite = isOuter ? outerLaserSprite : innerLaserSprite;
            Color trailColor = isOuter
                ? new Color(0.72f, 0.2f, 1f, 0.95f)    // Tím đậm ma mị cho dark_matter_07
                : new Color(0.35f, 0.95f, 1f, 0.95f);  // Lam ngọc tím sáng cho energy_waves_05

            float scale = isOuter ? 1.4f : 1.75f;
            bool spin = isOuter; // dark_matter_07 xoay vòng tròn khi bay

            SpawnBossBullet(spawnPos, dir, speed, bulletSprite, trailColor, spin, scale);
        }

        // Muzzle flash tại nòng súng
        SpaceCombatVfx.SpawnMuzzleFlash(transform.position + Vector3.left * 2f, 180f, new Color(0.75f, 0.35f, 1f));
    }

    private void SpawnBossBullet(Vector3 position, Vector2 direction, float speed, Sprite sprite, Color trailColor, bool spin, float scale)
    {
        GameObject laserObj = new GameObject("BossLaser");
        laserObj.transform.position = position;

        BossLaser laserScript = laserObj.AddComponent<BossLaser>();
        laserScript.Initialize(direction, speed, 1, sprite, Color.white, spin, scale, trailColor);
    }

    private static Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }

    /// <summary>
    /// Nhận sát thương khi bị đạn laser người chơi bắn trúng
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (isDead || isEntering) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthBar();

        // Rung giật nhẹ thanh máu (UI Impact Shake)
        if (barShakeCoroutine != null) StopCoroutine(barShakeCoroutine);
        barShakeCoroutine = StartCoroutine(ShakeHealthBarRoutine());

        // Chớp sáng đỏ báo hiệu trúng đòn
        StartCoroutine(HitFlashRoutine());

        // Kích hoạt Enraged (Phase 2) khi máu dưới 50%
        if (!isEnraged && currentHealth <= maxHealth * 0.5f)
        {
            EnterEnragedMode();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator ShakeHealthBarRoutine()
    {
        if (bossBarRect == null) yield break;

        for (int i = 0; i < 4; i++)
        {
            float offsetX = Random.Range(-2.5f, 2.5f);
            float offsetY = Random.Range(-1.5f, 1.5f);
            bossBarRect.anchoredPosition = bossBarOriginalPos + new Vector2(offsetX, offsetY);
            yield return new WaitForSeconds(0.025f);
        }

        bossBarRect.anchoredPosition = bossBarOriginalPos;
        barShakeCoroutine = null;
    }

    private IEnumerator HitFlashRoutine()
    {
        if (bossRenderer != null)
        {
            bossRenderer.color = isEnraged ? new Color(1f, 0.8f, 0.8f) : new Color(1f, 0.3f, 0.3f);
            yield return new WaitForSeconds(0.08f);
            bossRenderer.color = isEnraged ? new Color(1f, 0.5f, 0.4f) : Color.white;
        }
    }

    private void EnterEnragedMode()
    {
        isEnraged = true;
        moveSpeed *= 1.35f;
        if (bossRenderer != null)
        {
            bossRenderer.color = new Color(1f, 0.55f, 0.45f);
        }
        Debug.Log("[Boss] CẢNH BÁO: Boss bước vào PHASE 2 (QUÁ TẢI NĂNG LƯỢNG)!");
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (bossBarContainer != null)
        {
            bossBarContainer.SetActive(false);
        }

        StartCoroutine(DefeatSequence());
    }

    private IEnumerator DefeatSequence()
    {
        // Chuỗi 8 vụ nổ liên hoàn khắp thân tàu
        for (int i = 0; i < 8; i++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-2.0f, 2.0f),
                Random.Range(-1.8f, 1.8f),
                0f
            );
            Vector3 explosionPos = transform.position + randomOffset;
            SpaceCombatVfx.SpawnExplosion(explosionPos, Random.Range(1.4f, 2.4f));
            SpaceGameAudio.PlayAsteroidDestroyed(explosionPos, 0.7f);

            yield return new WaitForSeconds(0.24f);
        }

        // Vụ nổ hủy diệt siêu tân tinh (Supernova blast)
        SpaceCombatVfx.SpawnExplosion(transform.position, 4.2f);
        SpaceGameAudio.PlayPlayerDestroyed(transform.position, 1.0f);

        // Cộng 10,000 điểm thưởng hạ gục Boss
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(10000);
        }

        Debug.Log("[Boss] CHIẾN HẠM BOSS ĐÃ BỊ TIÊU DIỆT! MISSION ACCOMPLISHED!");

        yield return new WaitForSeconds(0.8f);

        // Hiện bảng chúc mừng chiến thắng tiêu diệt Boss
        BossVictoryUI.Show(10000);

        Destroy(gameObject);
    }

    // ----------------------------------------------------
    // GIAO DIỆN THANH MÁU BOSS (SCI-FI BOSS HP HUD)
    // ----------------------------------------------------
    private void CreateBossHealthUI()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        // 1. Warning Alert Banner
        warningBanner = new GameObject("BossWarningBanner", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        warningBanner.transform.SetParent(canvas.transform, false);

        RectTransform warnRect = warningBanner.GetComponent<RectTransform>();
        warnRect.anchorMin = new Vector2(0.5f, 0.5f);
        warnRect.anchorMax = new Vector2(0.5f, 0.5f);
        warnRect.anchoredPosition = new Vector2(0f, 120f);
        warnRect.sizeDelta = new Vector2(650f, 60f);

        Image warnImg = warningBanner.GetComponent<Image>();
        warnImg.color = new Color(0.8f, 0.08f, 0.15f, 0.88f);

        Outline warnOutline = warningBanner.AddComponent<Outline>();
        warnOutline.effectColor = new Color(1f, 0.7f, 0.1f, 0.95f);
        warnOutline.effectDistance = new Vector2(2f, -2f);

        GameObject warnTextObj = new GameObject("WarnText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        warnTextObj.transform.SetParent(warningBanner.transform, false);
        RectTransform warnTextRect = warnTextObj.GetComponent<RectTransform>();
        warnTextRect.sizeDelta = new Vector2(630f, 50f);

        TextMeshProUGUI warnText = warnTextObj.GetComponent<TextMeshProUGUI>();
        warnText.text = "WARNING: ALIEN DREADNOUGHT MOTHERSHIP DETECTED";
        warnText.font = UIFontHelper.GetCrispFont();
        warnText.fontSize = 20f;
        warnText.fontStyle = FontStyles.Bold;
        warnText.alignment = TextAlignmentOptions.Center;
        warnText.color = Color.white;
        warningBanner.SetActive(false);

        // 2. Boss Health Bar Pod (ở đỉnh màn hình, thanh máu mượt mà liền mạch)
        bossBarContainer = new GameObject("BossHealthPod", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
        bossBarContainer.transform.SetParent(canvas.transform, false);

        bossBarRect = bossBarContainer.GetComponent<RectTransform>();
        bossBarRect.anchorMin = new Vector2(0.5f, 1f);
        bossBarRect.anchorMax = new Vector2(0.5f, 1f);
        bossBarRect.pivot = new Vector2(0.5f, 1f);
        bossBarRect.anchoredPosition = new Vector2(0f, -54f);
        bossBarRect.sizeDelta = new Vector2(580f, 32f);
        bossBarOriginalPos = bossBarRect.anchoredPosition;

        Image barBg = bossBarContainer.GetComponent<Image>();
        barBg.color = new Color(0.015f, 0.035f, 0.08f, 0.95f);

        Outline barOutline = bossBarContainer.GetComponent<Outline>();
        barOutline.effectColor = new Color(1f, 0.25f, 0.35f, 0.85f);
        barOutline.effectDistance = new Vector2(2f, -2f);

        // Track nền tối bên trong thanh máu (lộ rõ rãnh khi thanh máu tụt dần)
        GameObject trackObj = new GameObject("HealthTrack", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        trackObj.transform.SetParent(bossBarContainer.transform, false);

        RectTransform trackRect = trackObj.GetComponent<RectTransform>();
        trackRect.anchorMin = Vector2.zero;
        trackRect.anchorMax = Vector2.one;
        trackRect.offsetMin = new Vector2(3f, 3f);
        trackRect.offsetMax = new Vector2(-3f, -3f);

        Image trackImg = trackObj.GetComponent<Image>();
        trackImg.color = new Color(0.12f, 0.04f, 0.06f, 0.92f); // Rãnh máu tối

        currentHealthFill = Mathf.Clamp01((float)currentHealth / maxHealth);
        ghostHealthFill = currentHealthFill;
        targetHealthFill = currentHealthFill;

        // A. Thanh Máu Đệm Vàng (Damage Ghost Trail: lướt êm ái phía sau thanh chính)
        GameObject ghostObj = new GameObject("HealthGhost", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        ghostObj.transform.SetParent(trackObj.transform, false);

        healthBarGhostRect = ghostObj.GetComponent<RectTransform>();
        healthBarGhostRect.anchorMin = Vector2.zero;
        healthBarGhostRect.anchorMax = new Vector2(ghostHealthFill, 1f);
        healthBarGhostRect.pivot = new Vector2(0f, 0.5f);
        healthBarGhostRect.offsetMin = Vector2.zero;
        healthBarGhostRect.offsetMax = Vector2.zero;

        healthBarGhost = ghostObj.GetComponent<Image>();
        healthBarGhost.color = new Color(1f, 0.75f, 0.15f, 0.9f); // Vàng cam ấm lướt êm phía sau

        // B. Thanh Máu Chính (Primary Health Bar: đỏ neon / cam lửa)
        GameObject fillObj = new GameObject("HealthFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fillObj.transform.SetParent(trackObj.transform, false);

        healthBarFillRect = fillObj.GetComponent<RectTransform>();
        healthBarFillRect.anchorMin = Vector2.zero;
        healthBarFillRect.anchorMax = new Vector2(currentHealthFill, 1f);
        healthBarFillRect.pivot = new Vector2(0f, 0.5f);
        healthBarFillRect.offsetMin = Vector2.zero;
        healthBarFillRect.offsetMax = Vector2.zero;

        healthBarFill = fillObj.GetComponent<Image>();
        healthBarFill.color = new Color(1f, 0.22f, 0.32f, 1f); // Đỏ neon

        // C. Label text
        GameObject textObj = new GameObject("HealthText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(bossBarContainer.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        healthBarText = textObj.GetComponent<TextMeshProUGUI>();
        healthBarText.font = UIFontHelper.GetCrispFont();
        healthBarText.fontSize = 13f;
        healthBarText.fontStyle = FontStyles.Bold;
        healthBarText.alignment = TextAlignmentOptions.Center;
        healthBarText.color = Color.white;
        healthBarText.text = $"DREADNOUGHT MOTHERSHIP   {currentHealth} / {maxHealth} HP";

        bossBarContainer.SetActive(false);
    }

    private void UpdateHealthBar()
    {
        targetHealthFill = Mathf.Clamp01((float)currentHealth / maxHealth);

        if (healthBarText != null)
        {
            string phaseTag = isEnraged ? "<color=#FF5500>[PHASE 2 - ENRAGED]</color> " : "";
            healthBarText.text = $"{phaseTag}DREADNOUGHT MOTHERSHIP   {Mathf.Max(0, currentHealth)} / {maxHealth} HP";
        }
    }

    private void AnimateHealthBar()
    {
        if (healthBarFillRect == null) return;

        // Thanh máu chính tụt dần mượt mà tương ứng với lượng sát thương nhận vào
        if (currentHealthFill > targetHealthFill)
        {
            float diff = currentHealthFill - targetHealthFill;
            float drainSpeed = Mathf.Max(0.35f, diff * 4.5f);
            currentHealthFill = Mathf.MoveTowards(
                currentHealthFill,
                targetHealthFill,
                drainSpeed * Time.deltaTime
            );
        }
        else
        {
            currentHealthFill = targetHealthFill;
        }

        // Cập nhật co giãn thanh máu chính từ phải sang trái
        healthBarFillRect.anchorMin = new Vector2(0f, 0f);
        healthBarFillRect.anchorMax = new Vector2(currentHealthFill, 1f);
        healthBarFillRect.offsetMin = Vector2.zero;
        healthBarFillRect.offsetMax = Vector2.zero;

        if (healthBarFill != null)
        {
            healthBarFill.color = isEnraged ? new Color(1f, 0.5f, 0.1f) : new Color(1f, 0.22f, 0.32f);
        }

        // Thanh đệm vàng phía sau lướt theo sau thanh chính, tạo hiệu ứng vệt máu tụt dần đẹp mắt
        if (healthBarGhostRect != null)
        {
            if (ghostHealthFill > currentHealthFill)
            {
                float ghostDiff = ghostHealthFill - currentHealthFill;
                float ghostSpeed = Mathf.Max(0.18f, ghostDiff * 2.5f);
                ghostHealthFill = Mathf.MoveTowards(
                    ghostHealthFill,
                    currentHealthFill,
                    ghostSpeed * Time.deltaTime
                );
            }
            else
            {
                ghostHealthFill = currentHealthFill;
            }

            // Cập nhật co giãn vệt vàng đệm lướt theo sau
            healthBarGhostRect.anchorMin = new Vector2(0f, 0f);
            healthBarGhostRect.anchorMax = new Vector2(ghostHealthFill, 1f);
            healthBarGhostRect.offsetMin = Vector2.zero;
            healthBarGhostRect.offsetMax = Vector2.zero;
        }
    }
}
