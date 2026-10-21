using UnityEngine;

/// <summary>
/// Quản lý điểm số, kỷ lục cá nhân, thống kê số sao và thiên thạch đã hạ,
/// chuyển đổi khu vực chơi và điều tiết độ khó theo thời gian.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    private const string BestScoreKey = "SpaceExplorer.BestScore";
    public const string StartLevelKey = "SpaceExplorer.StartLevel";

    public static ScoreManager Instance { get; private set; }

    [SerializeField] private int startingScore = 0;
    [SerializeField] private int pointsPerLevel = 10000;
    [SerializeField] private int levelCount = 3;
    [SerializeField] private Sprite[] levelBackgrounds;
    [SerializeField] private Sprite[] levelLaserSprites;
    [SerializeField] private float difficultyRampSeconds = 150f;
    [SerializeField] private float maximumDifficultyMultiplier = 2.5f;

    public int CurrentScore { get; private set; }
    public int BestScore { get; private set; }
    public int CurrentLevel { get; private set; } = 1;
    public int StarsCollected { get; private set; }
    public int AsteroidsDestroyed { get; private set; }

    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnLevelChanged;
    public System.Action<int, int> OnStatsChanged; // (stars, asteroids)

    private WorldSpawner worldSpawner;
    private StarSpawner starSpawner;
    private float currentDifficultyMultiplier = 1f;
    private bool bossSpawned = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        int requestedLevel = Mathf.Clamp(
            PlayerPrefs.GetInt(StartLevelKey, 1),
            1,
            Mathf.Max(1, levelCount)
        );

        CurrentLevel = requestedLevel;
        CurrentScore = startingScore + (requestedLevel - 1) * Mathf.Max(1, pointsPerLevel);
        BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        StarsCollected = 0;
        AsteroidsDestroyed = 0;
    }

    private void Start()
    {
        worldSpawner = FindAnyObjectByType<WorldSpawner>();
        starSpawner = FindAnyObjectByType<StarSpawner>();
        ApplyLevel(CurrentLevel);
        OnScoreChanged?.Invoke(CurrentScore);
        OnStatsChanged?.Invoke(StarsCollected, AsteroidsDestroyed);
    }

    private void Update()
    {
        float rampSeconds = Mathf.Max(1f, difficultyRampSeconds);
        float difficulty = Mathf.Min(
            maximumDifficultyMultiplier,
            1f + Time.timeSinceLevelLoad / rampSeconds
        );

        if (Mathf.Abs(difficulty - currentDifficultyMultiplier) >= 0.01f)
        {
            currentDifficultyMultiplier = difficulty;
            ApplyDifficulty();
        }

        // Phím tắt 'B' để kích hoạt Boss ngay lập tức (phục vụ test và biểu diễn)
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.bKey.wasPressedThisFrame)
        {
            TriggerBossEncounter();
        }
    }

    public void AddScore(int amount)
    {
        CurrentScore = Mathf.Max(0, CurrentScore + amount);

        if (CurrentScore > BestScore)
        {
            BestScore = CurrentScore;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();
        }

        OnScoreChanged?.Invoke(CurrentScore);

        int reachedLevel = Mathf.Min(
            levelCount,
            CurrentScore / Mathf.Max(1, pointsPerLevel) + 1
        );

        if (reachedLevel > CurrentLevel)
        {
            ApplyLevel(reachedLevel);
        }

        // Khi đạt mốc điểm cuối game: Kích hoạt Đại Chiến Trùm Cuối (Boss Battle)
        if (CurrentScore >= pointsPerLevel * levelCount && !bossSpawned)
        {
            TriggerBossEncounter();
        }
    }

    /// <summary>
    /// Kích hoạt cơ chế Boss xuất hiện ở cuối game
    /// </summary>
    public void TriggerBossEncounter()
    {
        if (Boss.Instance != null)
            return;

        bossSpawned = true;

        // Tạm dừng sinh thiên thạch để người chơi tập trung đánh Boss
        if (worldSpawner != null)
        {
            worldSpawner.IsSpawningActive = false;
        }

        Camera cam = Camera.main;
        Vector3 spawnPos = Vector3.zero;
        if (cam != null)
        {
            spawnPos = cam.ViewportToWorldPoint(new Vector3(1.35f, 0.5f, 0f));
            spawnPos.z = 0f;
        }
        else
        {
            spawnPos = new Vector3(30f, 0f, 0f);
        }

        GameObject bossObj = new GameObject("EndGameBoss");
        bossObj.transform.position = spawnPos;
        bossObj.AddComponent<Boss>();

        Debug.Log("[ScoreManager] === KÍCH HOẠT TRẬN ĐÁNH BOSS CUỐI GAME! ===");
    }

    public void RecordStarCollected()
    {
        StarsCollected++;
        OnStatsChanged?.Invoke(StarsCollected, AsteroidsDestroyed);
    }

    public void RecordAsteroidDestroyed()
    {
        AsteroidsDestroyed++;
        OnStatsChanged?.Invoke(StarsCollected, AsteroidsDestroyed);
    }

    public void ResetScore()
    {
        CurrentScore = startingScore;
        StarsCollected = 0;
        AsteroidsDestroyed = 0;
        OnScoreChanged?.Invoke(CurrentScore);
        OnStatsChanged?.Invoke(StarsCollected, AsteroidsDestroyed);
    }

    private void ApplyLevel(int level)
    {
        CurrentLevel = Mathf.Clamp(level, 1, Mathf.Max(1, levelCount));

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null && levelBackgrounds != null && levelBackgrounds.Length >= CurrentLevel)
        {
            player.SetBackgroundSprite(levelBackgrounds[CurrentLevel - 1]);
        }

        PlayerShooting shooting = FindAnyObjectByType<PlayerShooting>();
        if (shooting != null)
        {
            shooting.SetLevel(CurrentLevel);
            if (levelLaserSprites != null && levelLaserSprites.Length >= CurrentLevel)
            {
                shooting.SetLevelLaserSprite(levelLaserSprites[CurrentLevel - 1]);
            }
        }

        OnLevelChanged?.Invoke(CurrentLevel);
        OnScoreChanged?.Invoke(CurrentScore);
        ApplyDifficulty();
    }

    private void ApplyDifficulty()
    {
        float multiplier = currentDifficultyMultiplier * StageSpawnMultiplier(CurrentLevel);

        Asteroid.SetDifficultyMultiplier(multiplier);
        Star.SetDifficultyMultiplier(multiplier);
        worldSpawner?.SetDifficultyMultiplier(multiplier);
        starSpawner?.SetDifficultyMultiplier(multiplier);
    }

    private static float StageSpawnMultiplier(int level)
    {
        return level switch
        {
            1 => 1f,
            2 => 1.25f,
            _ => 1.5f
        };
    }
}