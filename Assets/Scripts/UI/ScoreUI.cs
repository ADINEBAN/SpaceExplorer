using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Quản lý thanh HUD thông số thời gian thực:
/// Thiết kế nhỏ gọn, tinh tế, bán trong suốt (Semi-transparent Glass Pill),
/// hiển thị Score, Sector, Stars, Kills, hệ thống 3 Trái Tim và Nút Pause Game.
/// </summary>
public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    private int lastScore;
    private int lastStars;
    private int lastKills;

    // 3 Hearts UI Elements
    private GameObject livesPod;
    private Image[] heartImages = new Image[3];
    private Sprite heartFullSprite;
    private Sprite heartEmptySprite;
    private bool livesBound;
    private int currentLivesCount = 3;

    // Pause UI Elements
    private GameObject pauseButtonObj;
    private GameObject pauseModal;
    private TMP_Text pauseStatsText;
    private bool isPaused;

    private void Start()
    {
        Time.timeScale = 1f;

        CanvasScaler scaler = GetComponentInParent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // Nạp sprite trái tim từ Resources/UI
        heartFullSprite = Resources.Load<Sprite>("UI/Heart_Full");
        heartEmptySprite = Resources.Load<Sprite>("UI/Heart_Empty");

        if (scoreText != null)
        {
            RectTransform rect = scoreText.rectTransform;

            // Neo ở chính giữa đỉnh màn hình, thu nhỏ kích thước gọn gàng, tinh tế
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -12f);
            rect.sizeDelta = new Vector2(520f, 32f);

            scoreText.font = UIFontHelper.GetCrispFont();
            scoreText.fontSize = 14f;
            scoreText.fontStyle = FontStyles.Bold;
            scoreText.alignment = TextAlignmentOptions.Center;
            scoreText.color = new Color(0.9f, 0.96f, 1f, 1f);
            scoreText.raycastTarget = false;

            // Thêm khung nền kính đen mờ viền neon siêu mỏng
            SetupHudBackground(scoreText.transform);

            // Dựng mô-đun 3 Trái Tim (3 Lives HUD) ở góc trên bên trái
            SetupLivesHUD(scoreText.transform.parent);

            // Dựng Nút Pause Game và Hộp thoại Pause Modal
            SetupPauseHUD(scoreText.transform.parent);
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScore;
            ScoreManager.Instance.OnStatsChanged += UpdateStats;

            UpdateScore(ScoreManager.Instance.CurrentScore);
            UpdateStats(ScoreManager.Instance.StarsCollected, ScoreManager.Instance.AsteroidsDestroyed);
        }

        TryBindPlayerLives();
    }

    private void TryBindPlayerLives()
    {
        if (livesBound) return;

        PlayerCollision pc = PlayerCollision.Instance != null
            ? PlayerCollision.Instance
            : FindAnyObjectByType<PlayerCollision>();

        if (pc != null)
        {
            pc.OnLivesChanged += UpdateLivesDisplay;
            UpdateLivesDisplay(pc.CurrentLives, pc.MaxLives);
            livesBound = true;
        }
    }

    private void SetupLivesHUD(Transform parent)
    {
        if (parent == null) return;

        // Container kính mờ bo tròn hiển thị 3 Trái Tim
        livesPod = new GameObject("LivesHudPod", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
        livesPod.transform.SetParent(parent, false);

        RectTransform podRect = livesPod.GetComponent<RectTransform>();
        podRect.anchorMin = new Vector2(0f, 1f);
        podRect.anchorMax = new Vector2(0f, 1f);
        podRect.pivot = new Vector2(0f, 1f);
        podRect.anchoredPosition = new Vector2(28f, -12f);
        podRect.sizeDelta = new Vector2(175f, 34f);

        Image podBg = livesPod.GetComponent<Image>();
        podBg.color = new Color(0.015f, 0.04f, 0.1f, 0.85f);

        Outline outline = livesPod.GetComponent<Outline>();
        outline.effectColor = new Color(1f, 0.25f, 0.45f, 0.65f); // Viền neon ánh hồng đỏ
        outline.effectDistance = new Vector2(1.2f, -1.2f);

        // Nhãn "LIVES:"
        GameObject labelObj = new GameObject("LivesLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObj.transform.SetParent(livesPod.transform, false);

        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0.5f);
        labelRect.anchorMax = new Vector2(0f, 0.5f);
        labelRect.pivot = new Vector2(0f, 0.5f);
        labelRect.anchoredPosition = new Vector2(12f, 0f);
        labelRect.sizeDelta = new Vector2(55f, 26f);

        TextMeshProUGUI labelText = labelObj.GetComponent<TextMeshProUGUI>();
        labelText.text = "LIVES";
        labelText.font = UIFontHelper.GetCrispFont();
        labelText.fontSize = 13f;
        labelText.fontStyle = FontStyles.Bold;
        labelText.color = new Color(1f, 0.35f, 0.5f, 1f);
        labelText.alignment = TextAlignmentOptions.MidlineLeft;

        // Tạo 3 biểu tượng Trái Tim (Hearts)
        for (int i = 0; i < 3; i++)
        {
            GameObject heartObj = new GameObject($"Heart_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            heartObj.transform.SetParent(livesPod.transform, false);

            RectTransform hRect = heartObj.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0f, 0.5f);
            hRect.anchorMax = new Vector2(0f, 0.5f);
            hRect.pivot = new Vector2(0.5f, 0.5f);
            hRect.anchoredPosition = new Vector2(78f + i * 32f, 0f);
            hRect.sizeDelta = new Vector2(24f, 24f);

            Image hImg = heartObj.GetComponent<Image>();
            hImg.sprite = heartFullSprite;
            hImg.color = Color.white;
            hImg.raycastTarget = false;
            heartImages[i] = hImg;
        }
    }

    private void UpdateLivesDisplay(int current, int max)
    {
        currentLivesCount = current;
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;

            bool isHeartActive = i < current;
            if (isHeartActive)
            {
                heartImages[i].sprite = heartFullSprite;
                heartImages[i].color = Color.white;
            }
            else
            {
                heartImages[i].sprite = heartEmptySprite;
                heartImages[i].color = new Color(0.7f, 0.75f, 0.85f, 0.7f); // Mờ đi khi mất mạng
            }
        }

        // Hiệu ứng giật nhẹ (Punch animation) cho tim vừa bị mất
        if (current < 3 && current >= 0 && heartImages[current] != null)
        {
            StartCoroutine(PunchHeart(heartImages[current].rectTransform));
        }

        RefreshUI();
    }

    private System.Collections.IEnumerator PunchHeart(RectTransform target)
    {
        if (target == null) yield break;

        Vector3 originalScale = Vector3.one;
        Vector3 punchScale = new Vector3(1.35f, 1.35f, 1f);

        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            target.localScale = Vector3.Lerp(originalScale, punchScale, t / 0.15f);
            yield return null;
        }

        t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            target.localScale = Vector3.Lerp(punchScale, originalScale, t / 0.2f);
            yield return null;
        }

        target.localScale = originalScale;
    }

    private void SetupHudBackground(Transform textTransform)
    {
        Transform parent = textTransform.parent;
        if (parent == null)
            return;

        GameObject bgObj = new GameObject("CompactHudBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
        bgObj.transform.SetParent(parent, false);
        bgObj.transform.SetSiblingIndex(textTransform.GetSiblingIndex()); // Nằm ngay dưới text

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 1f);
        bgRect.anchorMax = new Vector2(0.5f, 1f);
        bgRect.pivot = new Vector2(0.5f, 1f);
        bgRect.anchoredPosition = new Vector2(0f, -11f);
        bgRect.sizeDelta = new Vector2(536f, 34f);

        Image img = bgObj.GetComponent<Image>();
        img.color = new Color(0.015f, 0.04f, 0.1f, 0.82f); // Kính đen mờ nhẹ nhàng

        Outline outline = bgObj.GetComponent<Outline>();
        outline.effectColor = new Color(0.18f, 0.85f, 1f, 0.45f); // Viền neon mỏng
        outline.effectDistance = new Vector2(1.2f, -1.2f);
    }

    private void SetupPauseHUD(Transform parent)
    {
        if (parent == null) return;

        // 1. Nút Pause góc trên bên phải màn hình
        pauseButtonObj = new GameObject("PauseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(Outline));
        pauseButtonObj.transform.SetParent(parent, false);

        RectTransform btnRect = pauseButtonObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1f, 1f);
        btnRect.anchorMax = new Vector2(1f, 1f);
        btnRect.pivot = new Vector2(1f, 1f);
        btnRect.anchoredPosition = new Vector2(-28f, -12f);
        btnRect.sizeDelta = new Vector2(125f, 34f);

        Image btnImg = pauseButtonObj.GetComponent<Image>();
        btnImg.color = new Color(0.015f, 0.04f, 0.1f, 0.85f);

        Outline btnOutline = pauseButtonObj.GetComponent<Outline>();
        btnOutline.effectColor = new Color(0.18f, 0.85f, 1f, 0.65f);
        btnOutline.effectDistance = new Vector2(1.2f, -1.2f);

        Button btn = pauseButtonObj.GetComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(TogglePause);

        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.015f, 0.04f, 0.1f, 0.85f);
        colors.highlightedColor = new Color(0.08f, 0.25f, 0.5f, 0.95f);
        colors.pressedColor = new Color(0.15f, 0.4f, 0.75f, 1f);
        btn.colors = colors;

        GameObject btnTextObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        btnTextObj.transform.SetParent(pauseButtonObj.transform, false);

        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI btnText = btnTextObj.GetComponent<TextMeshProUGUI>();
        btnText.text = "|| PAUSE";
        btnText.font = UIFontHelper.GetCrispFont();
        btnText.fontSize = 13f;
        btnText.fontStyle = FontStyles.Bold;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = new Color(0.2f, 0.92f, 1f, 1f);
        btnText.raycastTarget = false;

        // 2. Bảng Hộp Thoại Pause Modal (Toàn màn hình)
        pauseModal = new GameObject("PauseModal", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        pauseModal.transform.SetParent(parent, false);

        RectTransform modalRect = pauseModal.GetComponent<RectTransform>();
        modalRect.anchorMin = Vector2.zero;
        modalRect.anchorMax = Vector2.one;
        modalRect.offsetMin = Vector2.zero;
        modalRect.offsetMax = Vector2.zero;

        Image modalBackdrop = pauseModal.GetComponent<Image>();
        modalBackdrop.color = new Color(0.01f, 0.025f, 0.07f, 0.85f);
        modalBackdrop.raycastTarget = true;

        // Card chính giữa
        GameObject cardObj = new GameObject("PauseCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
        cardObj.transform.SetParent(pauseModal.transform, false);

        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.sizeDelta = new Vector2(500f, 480f);

        Image cardImg = cardObj.GetComponent<Image>();
        cardImg.color = new Color(0.025f, 0.065f, 0.14f, 0.98f);

        Outline cardOutline = cardObj.GetComponent<Outline>();
        cardOutline.effectColor = new Color(0.13f, 0.82f, 1f, 0.9f);
        cardOutline.effectDistance = new Vector2(2.5f, -2.5f);

        // Top Accent
        GameObject accentObj = new GameObject("TopAccent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        accentObj.transform.SetParent(cardObj.transform, false);
        RectTransform accRect = accentObj.GetComponent<RectTransform>();
        accRect.anchorMin = new Vector2(0.5f, 1f);
        accRect.anchorMax = new Vector2(0.5f, 1f);
        accRect.anchoredPosition = new Vector2(0f, -4f);
        accRect.sizeDelta = new Vector2(500f, 8f);
        accentObj.GetComponent<Image>().color = new Color(0.13f, 0.82f, 1f, 1f);

        // Title
        CreateModalText(cardObj.transform, "Title", "SYSTEM PAUSED", 30f, new Color(0.18f, 0.92f, 1f, 1f), new Vector2(0f, 175f), new Vector2(460f, 45f), FontStyles.Bold);

        // Subtitle
        CreateModalText(cardObj.transform, "Subtitle", "Chiến dịch thám hiểm vũ trụ đang tạm dừng", 14f, new Color(0.68f, 0.82f, 0.94f, 1f), new Vector2(0f, 138f), new Vector2(460f, 30f));

        // Stats Box
        GameObject statsBoxObj = new GameObject("StatsBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
        statsBoxObj.transform.SetParent(cardObj.transform, false);
        RectTransform sRect = statsBoxObj.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0.5f, 0.5f);
        sRect.anchorMax = new Vector2(0.5f, 0.5f);
        sRect.anchoredPosition = new Vector2(0f, 80f);
        sRect.sizeDelta = new Vector2(440f, 52f);
        statsBoxObj.GetComponent<Image>().color = new Color(0.035f, 0.08f, 0.18f, 0.92f);
        Outline sOutline = statsBoxObj.GetComponent<Outline>();
        sOutline.effectColor = new Color(0.15f, 0.45f, 0.75f, 0.4f);
        sOutline.effectDistance = new Vector2(1.2f, -1.2f);

        pauseStatsText = CreateModalText(statsBoxObj.transform, "StatsContent", "SCORE: 0   •   LIVES: 3/3   •   STARS: 0", 14f, Color.white, Vector2.zero, new Vector2(420f, 40f), FontStyles.Bold);

        // Buttons
        CreateModalButton(cardObj.transform, "ResumeBtn", "RESUME (TIẾP TỤC)", new Vector2(0f, 10f), new Color(0.08f, 0.58f, 0.75f, 1f), ResumeGame);
        CreateModalButton(cardObj.transform, "RestartBtn", "RESTART (CHƠI LẠI)", new Vector2(0f, -54f), new Color(0.14f, 0.32f, 0.55f, 1f), RestartGame);
        CreateModalButton(cardObj.transform, "MainMenuBtn", "MAIN MENU", new Vector2(0f, -118f), new Color(0.12f, 0.22f, 0.36f, 1f), GoToMainMenu);
        CreateModalButton(cardObj.transform, "QuitBtn", "QUIT (THOÁT GAME)", new Vector2(0f, -182f), new Color(0.35f, 0.16f, 0.22f, 1f), QuitGame);

        pauseModal.transform.SetAsLastSibling();
        pauseModal.SetActive(false);
    }

    private TMP_Text CreateModalText(Transform parent, string name, string text, float fontSize, Color color, Vector2 pos, Vector2 size, FontStyles style = FontStyles.Normal)
    {
        GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(parent, false);

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.font = UIFontHelper.GetCrispFont();
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }

    private void CreateModalButton(Transform parent, string name, string label, Vector2 position, Color color, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(400f, 48f);

        Image img = btnObj.GetComponent<Image>();
        img.color = color;

        Button btn = btnObj.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        ColorBlock colors = btn.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.22f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.22f);
        btn.colors = colors;

        TMP_Text lbl = CreateModalText(btnObj.transform, "Label", label, 17f, Color.white, Vector2.zero, new Vector2(380f, 40f), FontStyles.Bold);
    }

    /// <summary>
    /// Bật/Tắt trạng thái Pause Game
    /// </summary>
    public void TogglePause()
    {
        // Không cho phép pause nếu màn chơi đã kết thúc (Game Over hoặc Victory)
        GameOverUI gameOverUI = FindAnyObjectByType<GameOverUI>();
        if (gameOverUI != null && Time.timeScale == 0f && !isPaused)
        {
            return;
        }

        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseStatsText != null)
        {
            int level = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentLevel : 1;
            string area = level switch
            {
                1 => "EARTH",
                2 => "BELT",
                _ => "SPACE"
            };
            pauseStatsText.text = $"SCORE: <color=#00E5FF>{lastScore:N0}</color>   •   LIVES: <color=#FF3366>{currentLivesCount}/3</color>   •   STARS: <color=#FFD700>{lastStars}</color>   •   SECTOR: <color=#70FF70>{area}</color>";
        }

        if (pauseModal != null)
        {
            pauseModal.transform.SetAsLastSibling();
            pauseModal.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseModal != null)
        {
            pauseModal.SetActive(false);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(sceneIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    private void Update()
    {
        if (!livesBound)
        {
            TryBindPlayerLives();
        }

        // Phím ESC hoặc phím P để tạm dừng game nhanh
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame ||
             UnityEngine.InputSystem.Keyboard.current.pKey.wasPressedThisFrame))
        {
            TogglePause();
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScore;
            ScoreManager.Instance.OnStatsChanged -= UpdateStats;
        }

        if (PlayerCollision.Instance != null)
        {
            PlayerCollision.Instance.OnLivesChanged -= UpdateLivesDisplay;
        }
    }

    private void UpdateScore(int score)
    {
        lastScore = score;
        RefreshUI();
    }

    private void UpdateStats(int stars, int kills)
    {
        lastStars = stars;
        lastKills = kills;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (scoreText == null)
            return;

        int level = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentLevel : 1;
        string area = level switch
        {
            1 => "EARTH ORBIT",
            2 => "ASTEROID BELT",
            _ => "DEEP SPACE"
        };

        GameLoadoutManager.DifficultyData diff = GameLoadoutManager.CurrentDifficulty;
        string hexColor = ColorUtility.ToHtmlStringRGB(diff.themeColor);

        // Chuỗi HUD nhỏ gọn, thanh thoát, chia ngăn tinh tế
        scoreText.text = $"SCORE: <color=#00E5FF>{lastScore:N0}</color>   •   <color=#{hexColor}>[{diff.shortTag}]</color> {area}   •   STARS: <color=#FFD700>{lastStars}</color>   •   KILLS: <color=#FF5555>{lastKills}</color>";
    }
}