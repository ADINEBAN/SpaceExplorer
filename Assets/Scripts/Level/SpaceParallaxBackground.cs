using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hệ thống hậu cảnh vũ trụ Parallax đa tầng:
/// 1. Tự động tìm và đưa hình nền không gian gốc (background2_0) về lớp sâu nhất (sortingOrder = -50).
/// 2. Sinh các hành tinh & tiểu hành tinh chân thực:
///    - Sao Mộc (Jupiter) xoay chậm vĩ đại
///    - Sao Hỏa (Mars) với bề mặt rỉ sét và miệng hố va chạm
///    - Sao Thủy (Mercury) màu bạc lơ lửng
///    - Sao Hải Vương (Ice Giant / Neptune) với vành đai phát sáng ngọc lam rực rỡ
/// 3. Lớp Parallax phản hồi chuyển động của Tàu Người Chơi:
///    - Bay lên/xuống (W/S): Các hành tinh và hậu cảnh trôi lệch theo phương dọc tạo chiều sâu không gian.
///    - Bay tới/lui (A/D): Tăng/giảm tốc độ luồng trôi tương quan.
///    - Tự xoay 360 độ liên tục quanh tâm.
///    - Tự động vòng lặp vô tận (Infinite wrap) khi trôi khỏi màn hình.
/// 4. Luồng bụi sao / hạt không gian (Starfield Dust) phản hồi vận tốc tàu và mức độ khó (Nightmare bão táp).
/// </summary>
public class SpaceParallaxBackground : MonoBehaviour
{
    private class CelestialBody
    {
        public Transform transform;
        public SpriteRenderer renderer;
        public float baseSpeed;
        public float spinSpeed;
        public float parallaxFactor;
        public float initialY;
    }

    [Header("Parallax Settings")]
    [SerializeField] private float globalSpeedMultiplier = 1.0f;
    [SerializeField] private float verticalParallaxRatio = 0.4f;

    private Camera mainCamera;
    private PlayerController player;
    private Vector2 lastPlayerPosition;
    private Vector2 playerVelocity;

    private readonly List<CelestialBody> celestialBodies = new List<CelestialBody>();
    private ParticleSystem starfieldParticles;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindAnyObjectByType<Camera>();
        }

        player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            lastPlayerPosition = player.transform.position;
        }

        // Đảm bảo background hiện hữu nằm ở lớp sâu nhất (-50) để không che khuất các hành tinh
        EnsureBackgroundSorting();

        SpawnCelestialPlanets();
        CreateSpaceDustStarfield();
    }

    /// <summary>
    /// Đưa tất cả background tĩnh/scrolling về sortingOrder = -50
    /// </summary>
    private void EnsureBackgroundSorting()
    {
        SpriteRenderer[] renderers = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include);
        foreach (var sr in renderers)
        {
            if (sr == null || sr.gameObject == null)
                continue;

            string name = sr.gameObject.name.ToLower();
            if (name.Contains("background") || name.Contains("backdrop"))
            {
                sr.sortingOrder = -50;
            }
        }
    }

    /// <summary>
    /// Sinh các hành tinh (Sao Mộc, Sao Hỏa, Sao Thủy, Sao Hải Vương) trôi xoay ở hậu cảnh
    /// </summary>
    private void SpawnCelestialPlanets()
    {
        string[] planetPaths = new string[]
        {
            "Planets/Planet_Jupiter",
            "Planets/Planet_Mars",
            "Planets/Planet_Mercury",
            "Planets/Planet_IceGiant"
        };

        // Kích thước, tốc độ trôi, tốc độ xoay và vị trí ban đầu
        float[] scales = new float[] { 3.6f, 2.2f, 1.8f, 3.2f };
        float[] baseSpeeds = new float[] { 0.40f, 0.70f, 0.55f, 0.50f };
        float[] spinSpeeds = new float[] { 3.5f, -7.5f, 5.0f, 4.0f };
        float[] startXOffsets = new float[] { 8f, 22f, 36f, 52f };
        float[] yPositions = new float[] { 2.4f, -2.6f, -0.5f, 1.8f };

        for (int i = 0; i < planetPaths.Length; i++)
        {
            Sprite planetSprite = Resources.Load<Sprite>(planetPaths[i]);
            if (planetSprite == null)
                continue;

            GameObject planetObj = new GameObject($"Celestial_{planetSprite.name}");
            planetObj.transform.SetParent(transform, false);

            SpriteRenderer sr = planetObj.AddComponent<SpriteRenderer>();
            sr.sprite = planetSprite;
            sr.sortingOrder = -30; // Nằm trước background (-50) và sau gameplay (0)
            sr.color = new Color(0.92f, 0.95f, 1f, 0.95f);

            float scale = scales[i];
            planetObj.transform.localScale = new Vector3(scale, scale, 1f);

            float startX = (mainCamera != null ? mainCamera.transform.position.x : 0f) + startXOffsets[i];
            float startY = yPositions[i];
            planetObj.transform.position = new Vector3(startX, startY, 2f);

            CelestialBody body = new CelestialBody
            {
                transform = planetObj.transform,
                renderer = sr,
                baseSpeed = baseSpeeds[i],
                spinSpeed = spinSpeeds[i],
                parallaxFactor = 0.2f + i * 0.12f,
                initialY = startY
            };

            celestialBodies.Add(body);
        }
    }

    /// <summary>
    /// Tạo luồng bụi sao vũ trụ (Starfield Dust) phản hồi vận tốc di chuyển của người chơi
    /// </summary>
    private void CreateSpaceDustStarfield()
    {
        GameObject dustObj = new GameObject("SpaceDustParticleSystem");
        dustObj.transform.SetParent(transform, false);
        dustObj.transform.localPosition = new Vector3(15f, 0f, 1f);

        starfieldParticles = dustObj.AddComponent<ParticleSystem>();
        starfieldParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = starfieldParticles.main;
        main.duration = 10f;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(3.5f, 6.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(10f, 22f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.22f);
        main.startColor = new Color(0.85f, 0.95f, 1f, 0.9f);
        main.maxParticles = 160;

        ParticleSystem.EmissionModule emission = starfieldParticles.emission;
        emission.rateOverTime = 30f;

        ParticleSystem.ShapeModule shape = starfieldParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(2f, 26f, 1f);
        shape.rotation = new Vector3(0f, 90f, 0f); // Phun sang trái (-X)

        ParticleSystemRenderer renderer = dustObj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder = -20; // Nằm trước các hành tinh (-30) và sau gameplay (0)

        starfieldParticles.Play();
    }

    private void Update()
    {
        if (mainCamera == null)
            return;

        // Tính toán vận tốc tàu người chơi để áp dụng Parallax tương tác
        if (player != null)
        {
            Vector2 currentPlayerPos = player.transform.position;
            playerVelocity = (currentPlayerPos - lastPlayerPosition) / Mathf.Max(0.001f, Time.deltaTime);
            lastPlayerPosition = currentPlayerPos;
        }

        UpdateCelestialPlanets();
        UpdateStarfieldParticles();
    }

    private void UpdateCelestialPlanets()
    {
        float camX = mainCamera.transform.position.x;
        float viewportWidth = mainCamera.orthographicSize * 2f * mainCamera.aspect;
        float leftEdge = camX - viewportWidth * 0.75f;
        float rightEdge = camX + viewportWidth * 0.75f;

        // Vận tốc tiến/lùi và lên/xuống của tàu người chơi ảnh hưởng tới góc nhìn
        float playerForwardInfluence = playerVelocity.x * 0.10f;

        foreach (var body in celestialBodies)
        {
            if (body == null || body.transform == null)
                continue;

            // 1. Tự xoay 360 độ quanh tâm
            body.transform.Rotate(Vector3.forward, body.spinSpeed * Time.deltaTime);

            // 2. Trôi dạt sang trái với tốc độ cơ bản + phản hồi gia tốc tàu
            float driftAmount = (body.baseSpeed + playerForwardInfluence) * globalSpeedMultiplier * Time.deltaTime;
            Vector3 pos = body.transform.position;
            pos.x -= driftAmount;

            // 3. Parallax phương dọc: Tàu bay lên (Y dương) -> các hành tinh hơi trôi lệch xuống (Y âm)
            float playerY = player != null ? player.transform.position.y : 0f;
            float targetY = body.initialY - playerY * body.parallaxFactor * verticalParallaxRatio;
            pos.y = Mathf.Lerp(pos.y, targetY, 4f * Time.deltaTime);

            // 4. Khi hành tinh trôi khuất mép trái -> tái xuất hiện bên mép phải (vòng lặp vũ trụ vô tận)
            if (pos.x < leftEdge - 10f)
            {
                pos.x = rightEdge + Random.Range(12f, 28f);
                body.initialY = Random.Range(-4.5f, 4.5f);
            }

            body.transform.position = pos;
        }
    }

    private void UpdateStarfieldParticles()
    {
        if (starfieldParticles == null || mainCamera == null)
            return;

        // Giữ vị trí nguồn phun bụi sao luôn ở mép phải của camera
        float camX = mainCamera.transform.position.x;
        float camY = mainCamera.transform.position.y;
        float halfWidth = mainCamera.orthographicSize * mainCamera.aspect;

        starfieldParticles.transform.position = new Vector3(camX + halfWidth + 5f, camY, 1f);

        // Khi người chơi di chuyển nhanh hoặc độ khó cao -> luồng hạt phóng vút nhanh hơn
        ParticleSystem.MainModule main = starfieldParticles.main;
        float speedBoost = Mathf.Max(0f, playerVelocity.x * 0.5f);
        float diffBoost = GameLoadoutManager.CurrentDifficulty.asteroidSpeedMult * 0.25f;
        main.simulationSpeed = 1f + speedBoost + diffBoost;
    }
}
