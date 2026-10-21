using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Điều khiển tàu vũ trụ: Di chuyển 8 hướng (Arrow Keys / WASD),
/// Xoay mượt 360 độ ngắm theo con trỏ chuột, giới hạn trong màn hình,
/// nạp mẫu phi thuyền từ Loadout và kích hoạt hệ thống hậu cảnh Parallax không gian.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 42f;
    [SerializeField] private float rotationSpeed = 950f;
    [SerializeField] private float angleOffset = -90f; // Mũi sprite vẽ hướng lên (+Y), bù trừ -90 độ để khớp góc ngắm (+X)
    [SerializeField] private float accelerationTime = 0.08f; // Thời gian đạt tốc độ tối đa (80ms): cực nhạy nhưng mượt mà tự nhiên, triệt tiêu giật cục khi đổi hướng tiến/lùi

    [Header("Screen Boundary")]
    [SerializeField] private float screenPadding = 1.0f;
    [SerializeField, Range(0.02f, 0.3f)] private float minRetreatViewportX = 0.08f; // Mức giới hạn lùi an toàn: 8% từ mép trái màn hình
    [SerializeField, Range(0.6f, 0.95f)] private float maxAdvanceViewportX = 0.86f; // Mức bay tiến lên phía trước tối đa trên màn hình: 86%

    [Header("Camera Follow")]
    [SerializeField, Range(0.1f, 0.9f)] private float cameraFollowViewportX = 0.35f; // Player neo ở 35% chiều ngang màn hình
    [SerializeField] private float cameraFollowSpeed = 65f;
    [SerializeField] private float cameraSmoothTime = 0.22f; // Thời gian hãm mượt (0.2s - 0.25s) triệt tiêu hoàn toàn hiện tượng khựng/giật

    // Dữ liệu vận tốc & dịch chuyển camera theo từng frame để các đối tượng khác (Asteroid, Star, Parallax) đồng bộ mượt mà
    public static float CameraDeltaX { get; private set; }
    public static float CameraVelocityX { get; private set; }
    private float cameraSmoothVelocityX;
    private float maxCameraTargetX;
    private Vector2 currentVelocity;

    [Header("Background Parallax")]
    [SerializeField] private Transform scrollingBackground;
    [SerializeField] private float backgroundParallaxRatio = 0.45f; // Tỉ lệ trôi Parallax của hậu cảnh sâu

    private Rigidbody2D rb;
    private Vector2 movement;
    private Camera mainCamera;
    private SpriteRenderer playerRenderer;
    private float cameraStartX;
    private SpriteRenderer backgroundRenderer;
    private readonly List<Transform> backgroundTiles = new List<Transform>();
    private float backgroundTileWidth;
    private Vector3 backgroundAnchorPosition;
    private bool backgroundStartsFlipped;
    private bool backgroundInitialized;

    // 360 Aiming Data
    private float currentZAngle = -90f;
    private Vector2 currentAimDirection = Vector2.right;

    public Vector2 AimDirection => currentAimDirection;
    public float AimAngleDegrees => currentZAngle - angleOffset;
    public Vector2 Velocity => currentVelocity;

    private TrailRenderer engineTrail;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Bật Interpolate để chuyển động của Player giữa các nhịp FixedUpdate (50Hz) và Render Frame (60-144Hz) luôn mượt tuyệt đối, triệt tiêu vi giật (micro-stutter)
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        CameraDeltaX = 0f;
        CameraVelocityX = 0f;
        cameraSmoothVelocityX = 0f;
        maxCameraTargetX = 0f;

        playerRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            mainCamera = FindAnyObjectByType<Camera>();
        }

        // Tự động tìm background nếu chưa được gán sẵn
        if (scrollingBackground == null)
        {
            SpriteRenderer[] renderers = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include);
            foreach (var sr in renderers)
            {
                if (sr == null || sr.gameObject == null) continue;
                string n = sr.gameObject.name.ToLower();
                if (n.Contains("background") || n.Contains("backdrop"))
                {
                    scrollingBackground = sr.transform;
                    break;
                }
            }
        }

        // Tự động kích hoạt hệ thống Parallax hậu cảnh (các hành tinh và bụi sao) nếu chưa có
        if (FindAnyObjectByType<SpaceParallaxBackground>() == null)
        {
            GameObject bgObj = new GameObject("SpaceParallaxBackgroundController");
            bgObj.AddComponent<SpaceParallaxBackground>();
        }
    }

    private void Start()
    {
        ApplyShipLoadout();
        currentZAngle = transform.eulerAngles.z;

        if (mainCamera != null)
        {
            mainCamera.backgroundColor = new Color(0.012f, 0.02f, 0.045f, 1f); // Đen không gian vũ trụ sâu
        }

        if (mainCamera != null && rb != null)
        {
            float viewportWidth = mainCamera.orthographicSize * 2f * mainCamera.aspect;
            // Canh vị trí camera ngay từ đầu sao cho player ở đúng mốc cameraFollowViewportX (35% từ mép trái).
            // Nhờ đó khi player vừa di chuyển thì camera và background chuyển động ngay lập tức,
            // không còn tình trạng phải bay tới tận mép bên kia mới bắt đầu cuộn màn hình.
            float initialCameraX = rb.position.x + viewportWidth * (0.5f - cameraFollowViewportX);
            Vector3 camPos = mainCamera.transform.position;
            camPos.x = initialCameraX;
            mainCamera.transform.position = camPos;

            cameraStartX = initialCameraX;
            maxCameraTargetX = initialCameraX;

            if (scrollingBackground != null && !backgroundInitialized)
            {
                InitializeBackground();
            }
        }
    }

    /// <summary>
    /// Nạp và áp dụng mẫu phi thuyền đã chọn từ Menu/Loadout
    /// </summary>
    public void ApplyShipLoadout()
    {
        GameLoadoutManager.ShipData shipData = GameLoadoutManager.CurrentShip;
        if (shipData != null)
        {
            moveSpeed = shipData.moveSpeed;
            rotationSpeed = shipData.rotationSpeed;
            cameraFollowSpeed = Mathf.Max(cameraFollowSpeed, moveSpeed * 1.15f);

            if (playerRenderer != null && !string.IsNullOrEmpty(shipData.spriteResourcePath))
            {
                Sprite loadedSprite = Resources.Load<Sprite>(shipData.spriteResourcePath);
                if (loadedSprite != null)
                {
                    playerRenderer.sprite = loadedSprite;
                    playerRenderer.color = Color.white;
                    playerRenderer.sortingOrder = 10; // Luôn hiển thị trên cùng so với background & planets

                    // Chuẩn hóa kích thước hiển thị đồng đều trên màn hình giữa các mẫu tàu của người chơi
                    float targetHeight = 8.5f;
                    float normScale = Mathf.Clamp(targetHeight / loadedSprite.bounds.size.y, 0.85f, 2.2f);
                    transform.localScale = new Vector3(normScale, normScale, 1f);

                    // Tự động cập nhật Collider vừa khít với kích thước sprite phi thuyền mới
                    CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
                    if (col != null)
                    {
                        col.size = new Vector2(loadedSprite.bounds.size.x * 0.72f, loadedSprite.bounds.size.y * 0.72f);
                    }
                }
            }

            SetupEngineTrail(shipData.accentColor);
        }
    }

    private void SetupEngineTrail(Color accentColor)
    {
        if (engineTrail == null)
        {
            GameObject trailObj = new GameObject("EngineTrail");
            trailObj.transform.SetParent(transform, false);
            trailObj.transform.localPosition = new Vector3(0f, -1.2f, 0.1f);

            engineTrail = trailObj.AddComponent<TrailRenderer>();
            engineTrail.time = 0.25f;
            engineTrail.startWidth = 0.5f;
            engineTrail.endWidth = 0.05f;
            engineTrail.material = playerRenderer != null ? playerRenderer.sharedMaterial : null;
        }

        engineTrail.startColor = accentColor;
        engineTrail.endColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0f);
    }

    private void Update()
    {
        ReadInput();
        AimTowardsMouse();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void LateUpdate()
    {
        UpdateCameraAndBackground();
    }

    /// <summary>
    /// Đọc phím điều khiển (Arrow Keys hoặc WASD)
    /// </summary>
    private void ReadInput()
    {
        movement = Vector2.zero;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            movement.x = -1f;
        }

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            movement.x = 1f;
        }

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            movement.y = 1f;
        }

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            movement.y = -1f;
        }

        movement = movement.normalized;
    }

    /// <summary>
    /// Xoay mượt 360 độ theo vị trí con trỏ chuột trên màn hình
    /// </summary>
    private void AimTowardsMouse()
    {
        if (mainCamera == null || Mouse.current == null)
            return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(mainCamera.transform.position.z))
        );

        Vector2 toMouse = (Vector2)mouseWorldPos - rb.position;

        // Tránh giật hoặc lỗi NaN nếu con trỏ chuột trùng vị trí phi thuyền
        if (toMouse.sqrMagnitude < 0.05f)
            return;

        currentAimDirection = toMouse.normalized;

        float targetAimAngle = Mathf.Atan2(currentAimDirection.y, currentAimDirection.x) * Mathf.Rad2Deg;
        float targetZAngle = targetAimAngle + angleOffset;

        currentZAngle = Mathf.MoveTowardsAngle(
            currentZAngle,
            targetZAngle,
            rotationSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Euler(0f, 0f, currentZAngle);
    }

    private void MovePlayer()
    {
        if (mainCamera == null || rb == null)
            return;

        // Nội suy gia tốc mượt mà (Inertia Smoothing):
        // Giúp phi thuyền đạt tốc độ tối đa trong 80ms, vừa cực kỳ nhạy vừa triệt tiêu cảm giác giật cục khi bấm phím tiến (D) hoặc lùi (A)
        Vector2 targetVelocity = movement * moveSpeed;
        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            targetVelocity,
            (moveSpeed / Mathf.Max(0.01f, accelerationTime)) * Time.fixedDeltaTime
        );

        Vector2 nextPosition = rb.position + currentVelocity * Time.fixedDeltaTime;

        float cameraDistance = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, cameraDistance));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, cameraDistance));
        float viewportWidth = topRight.x - bottomLeft.x;

        // 1. Giới hạn trục Y (lên / xuống màn hình)
        nextPosition.y = Mathf.Clamp(
            nextPosition.y,
            bottomLeft.y + screenPadding,
            topRight.y - screenPadding
        );

        // 2. Giới hạn trục X (Tiến / Lùi):
        // - LÙI ĐƯỢC MỘT MỨC NÀO ĐÓ:
        //   Tàu được tự do lùi về phía sau để né tránh thiên thạch và hỏa lực,
        //   nhưng có mức giới hạn an toàn ở mép trái màn hình (không lùi vô hạn hay biến mất khỏi màn hình)
        float leftPadding = Mathf.Max(screenPadding, viewportWidth * minRetreatViewportX);
        if (playerRenderer != null && playerRenderer.sprite != null)
        {
            leftPadding = Mathf.Max(leftPadding, playerRenderer.bounds.extents.x);
        }
        float minAllowedX = bottomLeft.x + leftPadding;

        // - TIẾN LÊN THOẢI MÁI:
        //   Tàu được bay vút lên phía trước tự do khắp màn hình (lên đến 86% chiều rộng khung hình),
        //   và hệ thống camera sẽ tự động cuộn tiến theo mượt mà không giới hạn
        float rightPadding = Mathf.Max(screenPadding, viewportWidth * (1f - maxAdvanceViewportX));
        if (playerRenderer != null && playerRenderer.sprite != null)
        {
            rightPadding = Mathf.Max(rightPadding, playerRenderer.bounds.extents.x);
        }
        float maxAllowedX = topRight.x - rightPadding;

        nextPosition.x = Mathf.Clamp(nextPosition.x, minAllowedX, maxAllowedX);

        rb.MovePosition(nextPosition);
    }

    private void UpdateCameraAndBackground()
    {
        if (mainCamera == null || rb == null)
            return;

        float viewportWidth = mainCamera.orthographicSize * 2f * mainCamera.aspect;

        // Vị trí camera mong muốn khi người chơi bay tiến về phía trước
        float desiredCameraX = rb.position.x + viewportWidth * (0.5f - cameraFollowViewportX);

        // NGUYÊN TẮC TIẾN / LÙI:
        // - TIẾN LÊN THOẢI MÁI: Khi người chơi bay tiến lên, camera ghi nhận mốc xa nhất và cuộn theo mượt mà
        // - LÙI LẠI ĐỨNG YÊN: Tuyệt đối không kéo lùi camera về sau khi người chơi bấm A (lùi),
        //   nhờ đó sao và thiên thạch tiếp tục trôi tự nhiên sang trái, không bị đảo chiều hay giật cục
        if (desiredCameraX > maxCameraTargetX)
        {
            maxCameraTargetX = desiredCameraX;
        }

        float targetCameraX = Mathf.Max(cameraStartX, maxCameraTargetX);

        Transform cameraTransform = mainCamera.transform;
        Vector3 cameraPosition = cameraTransform.position;
        float prevCamX = cameraPosition.x;

        cameraPosition.x = Mathf.SmoothDamp(
            cameraPosition.x,
            targetCameraX,
            ref cameraSmoothVelocityX,
            cameraSmoothTime,
            cameraFollowSpeed,
            Time.deltaTime
        );
        cameraTransform.position = cameraPosition;

        CameraDeltaX = Mathf.Max(0f, cameraPosition.x - prevCamX);
        CameraVelocityX = Mathf.Max(0f, cameraSmoothVelocityX);

        UpdateBackgroundTiles(viewportWidth);
    }

    private void EnsureBackgroundFit(Transform tile, SpriteRenderer sr)
    {
        if (tile == null || sr == null || sr.sprite == null || mainCamera == null)
            return;

        // Chiều cao viewport camera trong world space
        float cameraHeight = mainCamera.orthographicSize * 2f;
        // Chiều cao mong muốn cho background: lớn hơn camera 35% để luôn full tràn màn hình kể cả khi tàu bay sát mép
        float desiredHeight = cameraHeight * 1.35f;

        float baseSpriteHeight = sr.sprite.rect.height / Mathf.Max(1f, sr.sprite.pixelsPerUnit);
        if (baseSpriteHeight <= 0.01f)
            baseSpriteHeight = 36f;

        float scaleY = desiredHeight / baseSpriteHeight;
        tile.localScale = new Vector3(scaleY, scaleY, 1f);
    }

    private void InitializeBackground()
    {
        backgroundRenderer = scrollingBackground.GetComponent<SpriteRenderer>();

        if (backgroundRenderer == null)
        {
            scrollingBackground = null;
            return;
        }

        backgroundRenderer.sortingOrder = -50; // Nằm ở lớp sâu nhất để không che hành tinh
        EnsureBackgroundFit(scrollingBackground, backgroundRenderer);

        backgroundTileWidth = backgroundRenderer.bounds.size.x;

        if (backgroundTileWidth <= 0f)
        {
            scrollingBackground = null;
            backgroundRenderer = null;
            return;
        }

        backgroundAnchorPosition = scrollingBackground.position;
        backgroundStartsFlipped = backgroundRenderer.flipX;
        backgroundTiles.Add(scrollingBackground);
        backgroundInitialized = true;
    }

    public void SetBackgroundSprite(Sprite sprite)
    {
        if (sprite == null || scrollingBackground == null)
            return;

        if (!backgroundInitialized)
        {
            InitializeBackground();
        }

        if (backgroundRenderer == null)
            return;

        foreach (Transform tile in backgroundTiles)
        {
            if (tile == null)
                continue;

            SpriteRenderer tileRenderer = tile.GetComponent<SpriteRenderer>();
            if (tileRenderer != null)
            {
                tileRenderer.sprite = sprite;
                EnsureBackgroundFit(tile, tileRenderer);
            }
        }

        backgroundTileWidth = backgroundRenderer.bounds.size.x;
        backgroundAnchorPosition = scrollingBackground.position;
    }

    private void UpdateBackgroundTiles(float viewportWidth)
    {
        if (scrollingBackground == null || backgroundRenderer == null || backgroundTileWidth <= 0f)
            return;

        int requiredTileCount = Mathf.CeilToInt(viewportWidth / backgroundTileWidth) + 3;

        while (backgroundTiles.Count < requiredTileCount)
        {
            Transform tile = Instantiate(scrollingBackground, scrollingBackground.parent);
            tile.name = scrollingBackground.name + " (Tile " + backgroundTiles.Count + ")";
            EnsureBackgroundFit(tile, tile.GetComponent<SpriteRenderer>());
            backgroundTiles.Add(tile);
        }

        // Parallax phản hồi chuyển động camera & tàu:
        // Khi camera tiến lên, hậu cảnh trôi theo tỉ lệ Parallax mượt mà
        float camOffsetFromStart = mainCamera.transform.position.x - cameraStartX;
        float parallaxShiftX = camOffsetFromStart * (1f - backgroundParallaxRatio);
        float currentAnchorX = backgroundAnchorPosition.x + parallaxShiftX;

        float cameraLeftEdge = mainCamera.transform.position.x - viewportWidth * 0.5f;
        float backgroundLeftEdge = currentAnchorX - backgroundTileWidth * 0.5f;

        int firstTileIndex = Mathf.FloorToInt((cameraLeftEdge - backgroundLeftEdge) / backgroundTileWidth);

        for (int i = 0; i < backgroundTiles.Count; i++)
        {
            int tileIndex = firstTileIndex + i;
            Transform tile = backgroundTiles[i];
            if (tile == null)
                continue;

            Vector3 tilePosition = backgroundAnchorPosition;
            tilePosition.x = currentAnchorX + tileIndex * backgroundTileWidth;

            // Căn giữa background theo trục Y của camera, có trôi Parallax nhẹ theo chuyển động của tàu:
            // Background cao hơn viewport 35% nên luôn che kín hoàn toàn mép trên và mép dưới
            float camCenterY = mainCamera.transform.position.y;
            tilePosition.y = camCenterY - (rb.position.y - camCenterY) * 0.035f;
            tile.position = tilePosition;

            SpriteRenderer tileRenderer = tile.GetComponent<SpriteRenderer>();
            if (tileRenderer != null)
            {
                tileRenderer.sortingOrder = -50;
                tileRenderer.flipX = backgroundStartsFlipped ^ (tileIndex % 2 != 0);
            }
        }
    }
}