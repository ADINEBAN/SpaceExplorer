using UnityEngine;

/// <summary>
/// Hệ thống hiệu ứng chiến đấu không gian chất lượng cao:
/// 1. Hiệu ứng Muzzle Flash tại nòng súng: Chớp sáng chói lọi + chùm tia lửa phun theo hướng bắn.
/// 2. Hiệu ứng Vụ Nổ Lửa Bùng (Fiery Blast Explosion) đa tầng:
///    - Khối cầu lửa bùng nổ trung tâm (Lõi nhiệt trắng vàng -> Đỏ cam rực cháy -> Khói than)
///    - Lưỡi lửa bung xòe tứ phía
///    - Mưa tàn lửa bắn tung tóe với sức cản không gian
///    - Vòng sóng nhiệt xung kích lan tỏa
/// </summary>
public static class SpaceCombatVfx
{
    private static Material flameMaterial;
    private static Material sparkMaterial;
    private static bool initialized;

    private static void EnsureMaterials()
    {
        if (initialized && flameMaterial != null && sparkMaterial != null)
            return;

        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        Texture2D flameTex = Resources.Load<Texture2D>("VFX/Vfx_FlamePuff");
        Texture2D sparkTex = Resources.Load<Texture2D>("VFX/Vfx_FierySpark");

        flameMaterial = new Material(shader)
        {
            name = "VFX_Flame_RuntimeMat",
            mainTexture = flameTex
        };

        sparkMaterial = new Material(shader)
        {
            name = "VFX_Spark_RuntimeMat",
            mainTexture = sparkTex
        };

        initialized = true;
    }

    /// <summary>
    /// Hiệu ứng nòng súng cơ bản (fallback)
    /// </summary>
    public static void SpawnMuzzleFlash(Vector3 position)
    {
        SpawnMuzzleFlash(position, 0f, new Color(1f, 0.6f, 0.1f, 1f));
    }

    /// <summary>
    /// Hiệu ứng nòng súng khi bắn: Chớp sáng bùng nổ + chùm tia lửa bắn vút theo góc ngắm
    /// </summary>
    public static void SpawnMuzzleFlash(Vector3 position, float aimAngleDegrees, Color laserColor)
    {
        EnsureMaterials();

        GameObject root = new GameObject("MuzzleFlash_VFX");
        root.transform.position = position;
        root.transform.rotation = Quaternion.Euler(0f, 0f, aimAngleDegrees);

        // 1. Chớp sáng nòng súng trung tâm (Core Flash)
        GameObject flashObj = new GameObject("CoreFlash");
        flashObj.transform.SetParent(root.transform, false);
        ParticleSystem flashPs = CreateStoppedParticleSystem(flashObj);

        var fMain = flashPs.main;
        fMain.duration = 0.08f;
        fMain.loop = false;
        fMain.playOnAwake = false;
        fMain.startLifetime = 0.08f;
        fMain.startSpeed = 0f;
        fMain.startSize = new ParticleSystem.MinMaxCurve(0.65f, 1.1f);
        fMain.startColor = Color.Lerp(Color.white, laserColor, 0.45f);
        fMain.simulationSpace = ParticleSystemSimulationSpace.World;

        var fEmission = flashPs.emission;
        fEmission.rateOverTime = 0;
        fEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 2) });

        var fRenderer = flashObj.GetComponent<ParticleSystemRenderer>();
        fRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        fRenderer.material = flameMaterial != null ? flameMaterial : fRenderer.material;
        fRenderer.sortingOrder = 25;

        // 2. Chùm tia lửa nòng súng phun về phía trước theo hướng đạn (Conical Spark Spray)
        GameObject sparkObj = new GameObject("MuzzleSparks");
        sparkObj.transform.SetParent(root.transform, false);
        ParticleSystem sparkPs = CreateStoppedParticleSystem(sparkObj);

        var sMain = sparkPs.main;
        sMain.duration = 0.10f;
        sMain.loop = false;
        sMain.playOnAwake = false;
        sMain.startLifetime = new ParticleSystem.MinMaxCurve(0.06f, 0.12f);
        sMain.startSpeed = new ParticleSystem.MinMaxCurve(12f, 26f);
        sMain.startSize = new ParticleSystem.MinMaxCurve(0.14f, 0.35f);
        sMain.startColor = laserColor;
        sMain.simulationSpace = ParticleSystemSimulationSpace.World;

        var sShape = sparkPs.shape;
        sShape.shapeType = ParticleSystemShapeType.Cone;
        sShape.angle = 15f; // Chùm góc hẹp 15 độ phun thẳng theo mũi súng
        sShape.radius = 0.08f;
        sShape.rotation = new Vector3(0f, 90f, 0f);

        var sEmission = sparkPs.emission;
        sEmission.rateOverTime = 0;
        sEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 12) });

        var sRenderer = sparkObj.GetComponent<ParticleSystemRenderer>();
        sRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        sRenderer.material = sparkMaterial != null ? sparkMaterial : sRenderer.material;
        sRenderer.sortingOrder = 26;

        flashPs.Play();
        sparkPs.Play();

        Object.Destroy(root, 0.25f);
    }

    /// <summary>
    /// Hiệu ứng đạn trúng đích
    /// </summary>
    public static void SpawnHit(Vector3 position)
    {
        EnsureMaterials();

        GameObject root = new GameObject("Hit_VFX");
        root.transform.position = position;

        ParticleSystem ps = CreateStoppedParticleSystem(root);
        var main = ps.main;
        main.duration = 0.18f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 16f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.32f);
        main.startColor = new Color(1f, 0.88f, 0.3f, 1f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 16) });

        var renderer = root.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = sparkMaterial != null ? sparkMaterial : renderer.material;
        renderer.sortingOrder = 25;

        ps.Play();
        Object.Destroy(root, 0.3f);
    }

    /// <summary>
    /// HIỆU ỨNG NỔ LỬA BÙNG (Fiery Blast Explosion):
    /// Bùng nổ quả cầu lửa rực sáng, lưỡi lửa xòe rộng, tàn than lửa bay tứ phía và khói đen cuộn tan.
    /// </summary>
    public static void SpawnExplosion(Vector3 position, float scale = 1f)
    {
        EnsureMaterials();

        GameObject root = new GameObject("FieryExplosion_VFX");
        root.transform.position = position;

        // -------------------------------------------------------------
        // TẦNG 1: CẦU LỬA BÙNG NỔ TRUNG TÂM (CORE FIREBALL ERUPTION)
        // -------------------------------------------------------------
        GameObject coreObj = new GameObject("FireballCore");
        coreObj.transform.SetParent(root.transform, false);
        ParticleSystem corePs = CreateStoppedParticleSystem(coreObj);

        var cMain = corePs.main;
        cMain.duration = 0.45f;
        cMain.loop = false;
        cMain.playOnAwake = false;
        cMain.startLifetime = new ParticleSystem.MinMaxCurve(0.28f, 0.44f);
        cMain.startSpeed = new ParticleSystem.MinMaxCurve(1.2f * scale, 3.8f * scale);
        cMain.startSize = new ParticleSystem.MinMaxCurve(1.2f * scale, 2.8f * scale);
        cMain.simulationSpace = ParticleSystemSimulationSpace.World;

        // Gradient nhiệt độ: Trắng Vàng Siêu Nóng -> Cam Rực -> Đỏ Lửa -> Tro Tàn
        Gradient fireGradient = new Gradient();
        fireGradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(1f, 0.98f, 0.8f), 0f),      // Trắng vàng cực nóng
                new GradientColorKey(new Color(1f, 0.55f, 0.05f), 0.25f),  // Lửa cam rực
                new GradientColorKey(new Color(0.92f, 0.12f, 0.02f), 0.65f), // Đỏ lửa rực cháy
                new GradientColorKey(new Color(0.2f, 0.06f, 0.03f), 1f)     // Khói than đen
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.92f, 0.4f),
                new GradientAlphaKey(0f, 1f)
            }
        );

        var cCol = corePs.colorOverLifetime;
        cCol.enabled = true;
        cCol.color = new ParticleSystem.MinMaxGradient(fireGradient);

        // Nở to cực nhanh rồi co lại nhẹ khi tan thành khói
        var cSize = corePs.sizeOverLifetime;
        cSize.enabled = true;
        cSize.size = new ParticleSystem.MinMaxCurve(
            1f,
            new AnimationCurve(
                new Keyframe(0f, 0.35f),
                new Keyframe(0.22f, 1.25f),
                new Keyframe(0.75f, 1.1f),
                new Keyframe(1f, 0.15f)
            )
        );

        var cShape = corePs.shape;
        cShape.shapeType = ParticleSystemShapeType.Sphere;
        cShape.radius = 0.25f * scale;

        var cEmission = corePs.emission;
        cEmission.rateOverTime = 0;
        cEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 18) });

        var cRenderer = coreObj.GetComponent<ParticleSystemRenderer>();
        cRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        cRenderer.material = flameMaterial != null ? flameMaterial : cRenderer.material;
        cRenderer.sortingOrder = 24;

        // -------------------------------------------------------------
        // TẦNG 2: MƯA TÀN LỬA BẮN TUNG TÓE (BURNING EMBERS & SPARKS)
        // -------------------------------------------------------------
        GameObject sparkObj = new GameObject("BurningEmbers");
        sparkObj.transform.SetParent(root.transform, false);
        ParticleSystem sparkPs = CreateStoppedParticleSystem(sparkObj);

        var sMain = sparkPs.main;
        sMain.duration = 0.55f;
        sMain.loop = false;
        sMain.playOnAwake = false;
        sMain.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.52f);
        sMain.startSpeed = new ParticleSystem.MinMaxCurve(8f * scale, 22f * scale);
        sMain.startSize = new ParticleSystem.MinMaxCurve(0.18f * scale, 0.45f * scale);
        sMain.simulationSpace = ParticleSystemSimulationSpace.World;

        Gradient sparkGradient = new Gradient();
        sparkGradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(1f, 1f, 0.9f), 0f),
                new GradientColorKey(new Color(1f, 0.65f, 0.1f), 0.35f),
                new GradientColorKey(new Color(1f, 0.18f, 0.05f), 0.75f),
                new GradientColorKey(new Color(0.6f, 0.08f, 0.02f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.6f),
                new GradientAlphaKey(0f, 1f)
            }
        );

        var sCol = sparkPs.colorOverLifetime;
        sCol.enabled = true;
        sCol.color = new ParticleSystem.MinMaxGradient(sparkGradient);

        // Lực cản không gian làm tàn lửa chậm dần tự nhiên
        var sVel = sparkPs.limitVelocityOverLifetime;
        sVel.enabled = true;
        sVel.drag = 2.8f;

        var sShape = sparkPs.shape;
        sShape.shapeType = ParticleSystemShapeType.Sphere;
        sShape.radius = 0.15f * scale;

        var sEmission = sparkPs.emission;
        sEmission.rateOverTime = 0;
        sEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 42) });

        var sRenderer = sparkObj.GetComponent<ParticleSystemRenderer>();
        sRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        sRenderer.material = sparkMaterial != null ? sparkMaterial : sRenderer.material;
        sRenderer.sortingOrder = 26;

        // -------------------------------------------------------------
        // TẦNG 3: VÒNG SÓNG NHIỆT XUNG KÍCH (HEAT SHOCKWAVE RING)
        // -------------------------------------------------------------
        GameObject ringObj = new GameObject("ShockwaveRing");
        ringObj.transform.SetParent(root.transform, false);
        ParticleSystem ringPs = CreateStoppedParticleSystem(ringObj);

        var rMain = ringPs.main;
        rMain.duration = 0.22f;
        rMain.loop = false;
        rMain.playOnAwake = false;
        rMain.startLifetime = 0.22f;
        rMain.startSpeed = 0f;
        rMain.startSize = 0.5f * scale;
        rMain.startColor = new Color(1f, 0.55f, 0.1f, 0.9f);
        rMain.simulationSpace = ParticleSystemSimulationSpace.World;

        var rSize = ringPs.sizeOverLifetime;
        rSize.enabled = true;
        rSize.size = new ParticleSystem.MinMaxCurve(
            1f,
            new AnimationCurve(
                new Keyframe(0f, 0.5f),
                new Keyframe(1f, 4.5f * scale) // Vòng sóng lửa nở to gấp 4.5 lần
            )
        );

        var rCol = ringPs.colorOverLifetime;
        rCol.enabled = true;
        rCol.color = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.65f, 0.15f, 0.95f),
            new Color(0.9f, 0.15f, 0.02f, 0f)
        );

        var rEmission = ringPs.emission;
        rEmission.rateOverTime = 0;
        rEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });

        var rRenderer = ringObj.GetComponent<ParticleSystemRenderer>();
        rRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        rRenderer.material = flameMaterial != null ? flameMaterial : rRenderer.material;
        rRenderer.sortingOrder = 23;

        // Kích hoạt toàn bộ các tầng vụ nổ
        corePs.Play();
        sparkPs.Play();
        ringPs.Play();

        Object.Destroy(root, 0.75f);
    }

    private static ParticleSystem CreateStoppedParticleSystem(GameObject obj)
    {
        ParticleSystem ps = obj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        return ps;
    }
}
