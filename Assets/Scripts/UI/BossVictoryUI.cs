using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Bảng Popup Chúc Mừng Chiến Thắng Diệt Boss (Sci-Fi Boss Victory Congratulations Panel):
/// - Tự động hiển thị hoành tráng khi người chơi bắn hạ Trùm Cuối (Boss Mothership).
/// - Hiệu ứng âm thanh Victory Fanfare hào hùng và vệt sao hoa pháo phát sáng lấp lánh.
/// - Đánh giá xếp hạng chiến công (S-Rank, A-Rank, B-Rank) dựa trên số mạng còn lại.
/// - Thống kê chi tiết điểm thưởng (+10,000 PTS), kỷ lục cá nhân, số sao và thiên thạch đã diệt.
/// - Cung cấp 3 nút điều hướng: Chơi Lại (Play Again), Về Menu (Main Menu) và Thoát (Quit).
/// </summary>
public class BossVictoryUI : MonoBehaviour
{
    public static BossVictoryUI Instance { get; private set; }

    private GameObject backdrop;
    private RectTransform modalPanelRect;
    private TMP_FontAsset fontAsset;
    private bool isShowing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        fontAsset = UIFontHelper.GetCrispFont();
    }

    /// <summary>
    /// Kích hoạt mở bảng chúc mừng chiến thắng Boss
    /// </summary>
    public static void Show(int bossBonusScore = 10000)
    {
        if (Instance != null && Instance.isShowing)
            return;

        if (Instance == null)
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("VictoryCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObj.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            GameObject victoryObj = new GameObject("BossVictoryUI", typeof(RectTransform));
            victoryObj.transform.SetParent(canvas.transform, false);
            Instance = victoryObj.AddComponent<BossVictoryUI>();
        }

        Instance.BuildAndShow(bossBonusScore);
    }

    private void BuildAndShow(int bossBonusScore)
    {
        isShowing = true;
        Time.timeScale = 0f; // Dừng game ăn mừng chiến thắng

        // Phát âm thanh khúc khải hoàn chiến thắng hoành tráng
        Vector3 camPos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        SpaceGameAudio.PlayVictoryFanfare(camPos, 1.0f);

        Canvas canvas = GetComponentInParent<Canvas>();
        transform.SetAsLastSibling();

        // 1. Phông nền mờ vũ trụ (Backdrop)
        backdrop = new GameObject("VictoryBackdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        backdrop.transform.SetParent(transform, false);

        RectTransform bdRect = backdrop.GetComponent<RectTransform>();
        bdRect.anchorMin = Vector2.zero;
        bdRect.anchorMax = Vector2.one;
        bdRect.offsetMin = Vector2.zero;
        bdRect.offsetMax = Vector2.zero;

        Image bdImg = backdrop.GetComponent<Image>();
        bdImg.color = new Color(0.015f, 0.025f, 0.06f, 0.92f);
        bdImg.raycastTarget = true;

        // Sinh 28 hạt sao lấp lánh (Confetti Star Sparks)
        StartCoroutine(SpawnCelebrationStarsRoutine(bdRect));

        // 2. Khung Bảng Chúc Mừng Chính (Sci-Fi Modal Panel)
        GameObject modalObj = new GameObject("ModalPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
        modalObj.transform.SetParent(transform, false);

        modalPanelRect = modalObj.GetComponent<RectTransform>();
        modalPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
        modalPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        modalPanelRect.pivot = new Vector2(0.5f, 0.5f);
        modalPanelRect.anchoredPosition = Vector2.zero;
        modalPanelRect.sizeDelta = new Vector2(660f, 570f);

        Image modalImg = modalObj.GetComponent<Image>();
        modalImg.color = new Color(0.02f, 0.045f, 0.11f, 0.98f);

        Outline modalOutline = modalObj.GetComponent<Outline>();
        modalOutline.effectColor = new Color(1f, 0.84f, 0.18f, 0.95f); // Viền vàng hoàng gia phát sáng
        modalOutline.effectDistance = new Vector2(3.5f, -3.5f);

        // Thanh viền Neon trang trí trên và dưới
        CreateAccentBar(modalPanelRect, new Vector2(0f, 281f), new Vector2(660f, 6f), new Color(0.2f, 0.92f, 1f, 1f));
        CreateAccentBar(modalPanelRect, new Vector2(0f, -281f), new Vector2(660f, 6f), new Color(1f, 0.84f, 0.18f, 1f));

        // 3. Tiêu đề chúc mừng
        CreateText(
            modalPanelRect,
            "CrownHeader",
            "[ VICTORY ] CHIẾN HẠM BOSS ĐÃ BỊ TIÊU DIỆT!",
            22f,
            new Color(1f, 0.88f, 0.22f, 1f),
            new Vector2(0f, 240f),
            new Vector2(620f, 40f),
            FontStyles.Bold
        );

        // 4. Đánh giá xếp hạng (Rank Badge) & Độ khó
        int remainingLives = PlayerCollision.Instance != null ? PlayerCollision.Instance.CurrentLives : 3;
        int maxLives = PlayerCollision.Instance != null ? PlayerCollision.Instance.MaxLives : 3;
        string rankTitle;
        Color rankColor;

        if (remainingLives >= 3)
        {
            rankTitle = "[ S-RANK ACE PILOT ] (3/3 MẠNG TOÀN VẸN)";
            rankColor = new Color(0.22f, 1f, 0.52f, 1f); // Xanh ngọc lục bảo rực rỡ
        }
        else if (remainingLives == 2)
        {
            rankTitle = "[ A-RANK VETERAN ] (2/3 MẠNG)";
            rankColor = new Color(1f, 0.85f, 0.2f, 1f); // Vàng rực rỡ
        }
        else
        {
            rankTitle = "[ B-RANK SURVIVOR ] (1/3 MẠNG)";
            rankColor = new Color(1f, 0.55f, 0.2f, 1f); // Cam lửa
        }

        int diffIndex = GameLoadoutManager.SelectedDifficultyIndex;
        string diffName = diffIndex switch
        {
            0 => "EASY",
            1 => "NORMAL",
            2 => "HARD",
            _ => "NIGHTMARE"
        };

        CreateText(
            modalPanelRect,
            "RankBadge",
            $"{rankTitle}   •   <color=#00F0FF>[{diffName}]</color>",
            15.5f,
            rankColor,
            new Vector2(0f, 196f),
            new Vector2(600f, 28f),
            FontStyles.Bold
        );

        // 5. Thống Kê Chi Tiết Chiến Công (Stats Card Box)
        GameObject statsBox = new GameObject("StatsBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
        statsBox.transform.SetParent(modalPanelRect, false);

        RectTransform boxRect = statsBox.GetComponent<RectTransform>();
        boxRect.anchorMin = new Vector2(0.5f, 0.5f);
        boxRect.anchorMax = new Vector2(0.5f, 0.5f);
        boxRect.anchoredPosition = new Vector2(0f, 82f);
        boxRect.sizeDelta = new Vector2(580f, 156f);

        Image boxImg = statsBox.GetComponent<Image>();
        boxImg.color = new Color(0.012f, 0.025f, 0.06f, 0.94f);

        Outline boxOutline = statsBox.GetComponent<Outline>();
        boxOutline.effectColor = new Color(0.2f, 0.5f, 0.8f, 0.45f);
        boxOutline.effectDistance = new Vector2(1.5f, -1.5f);

        ScoreManager sm = ScoreManager.Instance;
        int currentScore = sm != null ? sm.CurrentScore : 0;
        int bestScore = sm != null ? sm.BestScore : 0;
        int stars = sm != null ? sm.StarsCollected : 0;
        int kills = sm != null ? sm.AsteroidsDestroyed : 0;

        string recordTag = (currentScore >= bestScore && currentScore > 0)
            ? " <color=#FFE500>[KỶ LỤC MỚI!]</color>"
            : "";

        CreateText(boxRect, "BonusRow", $"[+] THƯỞNG HẠ BOSS:   +{bossBonusScore:N0} PTS", 16f, new Color(1f, 0.88f, 0.15f), new Vector2(0f, 46f), new Vector2(540f, 26f), FontStyles.Bold);
        CreateText(boxRect, "ScoreRow", $"[>] TỔNG ĐIỂM CHIẾN ĐẤU:   {currentScore:N0} PTS", 18f, new Color(0.2f, 0.92f, 1f), new Vector2(0f, 15f), new Vector2(540f, 28f), FontStyles.Bold);
        CreateText(boxRect, "BestRow", $"[*] KỶ LỤC CÁ NHÂN:   {bestScore:N0} PTS{recordTag}", 14.5f, new Color(0.9f, 0.95f, 1f), new Vector2(0f, -16f), new Vector2(540f, 24f), FontStyles.Normal);
        CreateText(boxRect, "StatsRow", $"SỐ SAO: {stars}      |      THIÊN THẠCH ĐÃ DIỆT: {kills}", 14f, new Color(0.55f, 0.95f, 0.65f), new Vector2(0f, -46f), new Vector2(540f, 24f), FontStyles.Normal);

        // 6. Các Nút Tương Tác (Hỗ trợ Chơi Cấp Độ Tiếp Theo)
        int currentLvl = sm != null ? sm.CurrentLevel : 1;
        string nextBtnLabel;
        Color nextBtnColor;
        Color nextBtnOutline;

        if (currentLvl < 3)
        {
            int nextLvl = currentLvl + 1;
            string areaName = nextLvl switch
            {
                2 => "LEVEL 2: VÀNH ĐAI TIỂU HÀNH TINH",
                _ => "LEVEL 3: KHÔNG GIAN SÂU"
            };
            nextBtnLabel = $"CẤP ĐỘ TIẾP THEO ({areaName})";
            nextBtnColor = new Color(0.08f, 0.68f, 0.36f, 1f); // Neon Emerald rực sáng
            nextBtnOutline = new Color(0.4f, 1f, 0.6f, 0.95f);
        }
        else
        {
            // Đã vượt qua Level 3 (Deep Space) - hoàn thành toàn bộ 3 cấp độ vũ trụ!
            if (diffIndex < GameLoadoutManager.Difficulties.Length - 1)
            {
                string nextDiffName = GameLoadoutManager.Difficulties[diffIndex + 1].name;
                nextBtnLabel = $"CẤP ĐỘ KHÓ TIẾP THEO (ĐỘ KHÓ: {nextDiffName})";
                nextBtnColor = new Color(0.88f, 0.48f, 0.08f, 1f); // Cam lửa Cyber rực rỡ
                nextBtnOutline = new Color(1f, 0.88f, 0.25f, 0.95f);
            }
            else
            {
                nextBtnLabel = "VÒNG LẶP TIẾP THEO (NEW GAME+: ÁC MỘNG)";
                nextBtnColor = new Color(0.75f, 0.12f, 0.28f, 1f); // Đỏ thẫm Crimson hoàng gia
                nextBtnOutline = new Color(1f, 0.4f, 0.55f, 0.95f);
            }
        }

        CreateButton(modalPanelRect, "NextLevelBtn", nextBtnLabel, new Vector2(0f, -32f), nextBtnColor, OnNextLevelClicked, nextBtnOutline, new Vector2(520f, 48f));
        CreateButton(modalPanelRect, "PlayAgainBtn", "PLAY AGAIN (CHƠI LẠI CẤP ĐỘ NÀY)", new Vector2(0f, -88f), new Color(0.08f, 0.52f, 0.74f, 1f), OnPlayAgainClicked, new Color(0.2f, 0.75f, 1f, 0.4f), new Vector2(520f, 48f));
        CreateButton(modalPanelRect, "MainMenuBtn", "MAIN MENU (VỀ MENU CHÍNH)", new Vector2(0f, -144f), new Color(0.14f, 0.22f, 0.42f, 1f), OnMainMenuClicked, new Color(0.4f, 0.5f, 0.8f, 0.35f), new Vector2(520f, 48f));
        CreateButton(modalPanelRect, "QuitBtn", "QUIT (THOÁT GAME)", new Vector2(0f, -200f), new Color(0.35f, 0.14f, 0.20f, 1f), OnQuitClicked, new Color(0.8f, 0.25f, 0.35f, 0.35f), new Vector2(520f, 48f));

        // 7. Hiệu ứng phóng to mượt mà (Pop-in Bounce Animation)
        StartCoroutine(AnimateModalPopIn(modalPanelRect));
    }

    private IEnumerator AnimateModalPopIn(RectTransform targetRect)
    {
        targetRect.localScale = Vector3.one * 0.35f;
        float elapsed = 0f;
        float duration = 0.42f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            // Easing punch bounce
            float scale = Mathf.Sin(t * Mathf.PI * 0.5f);
            if (t > 0.7f)
            {
                float overshoot = Mathf.Sin((t - 0.7f) / 0.3f * Mathf.PI) * 0.08f;
                scale += overshoot;
            }
            targetRect.localScale = Vector3.one * scale;
            yield return null;
        }

        targetRect.localScale = Vector3.one;
    }

    private IEnumerator SpawnCelebrationStarsRoutine(RectTransform parentRect)
    {
        Color[] sparkColors = new Color[]
        {
            new Color(1f, 0.88f, 0.2f, 0.85f),  // Gold
            new Color(0.2f, 0.94f, 1f, 0.85f),  // Cyan
            new Color(0.35f, 1f, 0.55f, 0.85f), // Emerald
            new Color(1f, 0.45f, 0.85f, 0.85f), // Neon Magenta
            new Color(1f, 1f, 1f, 0.95f)        // Pure White
        };

        for (int i = 0; i < 36; i++)
        {
            GameObject sparkObj = new GameObject("CelebrationSpark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            sparkObj.transform.SetParent(parentRect, false);

            RectTransform sRect = sparkObj.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0.5f, 0.5f);
            sRect.anchorMax = new Vector2(0.5f, 0.5f);

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(180f, 540f);
            sRect.anchoredPosition = new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
            float sparkSize = Random.Range(8f, 18f);
            sRect.sizeDelta = new Vector2(sparkSize, sparkSize);
            sRect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(15f, 75f));

            Image img = sparkObj.GetComponent<Image>();
            img.color = sparkColors[Random.Range(0, sparkColors.Length)];
            img.raycastTarget = false;
        }

        yield return null;
    }

    private static void CreateAccentBar(Transform parent, Vector2 pos, Vector2 size, Color color)
    {
        GameObject bar = new GameObject("AccentBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bar.transform.SetParent(parent, false);

        RectTransform r = bar.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.5f, 0.5f);
        r.anchorMax = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = pos;
        r.sizeDelta = size;

        Image img = bar.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    private TMP_Text CreateText(
        Transform parent,
        string name,
        string content,
        float fontSize,
        Color color,
        Vector2 pos,
        Vector2 size,
        FontStyles fontStyle = FontStyles.Normal
    )
    {
        GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(parent, false);

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
        text.text = content;
        text.font = fontAsset;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = fontSize * 0.75f;
        text.fontSizeMax = fontSize;
        text.raycastTarget = false;

        return text;
    }

    private void CreateButton(
        Transform parent,
        string name,
        string label,
        Vector2 pos,
        Color bgColor,
        UnityEngine.Events.UnityAction onClick,
        Color? outlineColor = null,
        Vector2? customSize = null
    )
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(Outline));
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = customSize ?? new Vector2(520f, 48f);

        Image img = btnObj.GetComponent<Image>();
        img.color = bgColor;

        Outline outline = btnObj.GetComponent<Outline>();
        outline.effectColor = outlineColor ?? new Color(1f, 1f, 1f, 0.25f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        Button btn = btnObj.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        ColorBlock colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = Color.Lerp(bgColor, Color.white, 0.25f);
        colors.pressedColor = Color.Lerp(bgColor, Color.black, 0.25f);
        btn.colors = colors;

        TMP_Text btnText = CreateText(rect, "Label", label, 16.5f, Color.white, Vector2.zero, new Vector2(500f, 44f), FontStyles.Bold);
        btnText.raycastTarget = false;
    }

    private void OnNextLevelClicked()
    {
        Time.timeScale = 1f;

        int currentLvl = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentLevel : 1;
        int diffIndex = GameLoadoutManager.SelectedDifficultyIndex;

        if (currentLvl < 3)
        {
            // Tiến sang cấp độ khu vực kế tiếp (Level 2 hoặc Level 3)
            PlayerPrefs.SetInt(ScoreManager.StartLevelKey, currentLvl + 1);
            PlayerPrefs.Save();
        }
        else
        {
            // Đã vượt qua Level 3 (Deep Space) -> Tiến lên Cấp độ khó cao hơn
            if (diffIndex < GameLoadoutManager.Difficulties.Length - 1)
            {
                GameLoadoutManager.SelectedDifficultyIndex = diffIndex + 1;
            }
            // Khởi động lại từ Level 1 với độ khó mới
            PlayerPrefs.SetInt(ScoreManager.StartLevelKey, 1);
            PlayerPrefs.Save();
        }

        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(activeSceneIndex);
    }

    private void OnPlayAgainClicked()
    {
        Time.timeScale = 1f;
        int currentLvl = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentLevel : 1;
        PlayerPrefs.SetInt(ScoreManager.StartLevelKey, currentLvl);
        PlayerPrefs.Save();

        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(activeSceneIndex);
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt(ScoreManager.StartLevelKey, 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainMenu");
    }

    private void OnQuitClicked()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
