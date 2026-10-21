using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Giao diện kết thúc màn chơi (Game Over & Victory):
/// Hiển thị Điểm số cuối cùng, Kỷ lục cá nhân, Thống kê số sao và thiên thạch đã diệt,
/// cung cấp các nút: Chơi Lại (Play Again), Về Menu (Main Menu) và Thoát (Quit).
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;

    private TMP_Text titleText;
    private TMP_Text subtitleText;
    private TMP_Text bestScoreText;
    private TMP_Text statsText;
    private TMP_FontAsset interfaceFont;
    private GameObject backdrop;
    private bool isVictory;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (gameOverPanel == null)
        {
            Debug.LogError("GameOverUI: Cần gán Game Over panel trong Inspector.");
            return;
        }

        interfaceFont = UIFontHelper.GetCrispFont();

        BuildPanel();
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        isVictory = false;
        ShowResult();
    }

    public void ShowVictory()
    {
        BossVictoryUI.Show();
    }

    private void ShowResult()
    {
        if (gameOverPanel == null || titleText == null || subtitleText == null ||
            finalScoreText == null || bestScoreText == null)
        {
            Debug.LogError("GameOverUI: Bảng kết quả chưa được khởi tạo đầy đủ.");
            return;
        }

        ScoreManager scoreManager = ScoreManager.Instance;
        if (scoreManager != null)
        {
            titleText.text = isVictory ? "MISSION COMPLETE" : "MISSION FAILED";
            titleText.color = isVictory
                ? new Color(0.2f, 0.95f, 0.4f, 1f)
                : new Color(1f, 0.25f, 0.35f, 1f);

            subtitleText.text = isVictory
                ? "Chiến hạm Alien Dreadnought đã bị bắn hạ! Thiên hà đã an toàn."
                : "Phi thuyền đã cạn kiệt 3 mạng sống trong không gian nguy hiểm.";

            finalScoreText.text = $"FINAL SCORE   {scoreManager.CurrentScore:N0}";
            bestScoreText.text = $"PERSONAL BEST   {scoreManager.BestScore:N0}";

            if (statsText != null)
            {
                statsText.text = $"STARS: {scoreManager.StarsCollected}    |    KILLS: {scoreManager.AsteroidsDestroyed}";
            }
        }
        else
        {
            titleText.text = isVictory ? "MISSION COMPLETE" : "MISSION FAILED";
            subtitleText.text = "Dữ liệu điểm số không khả dụng.";
            finalScoreText.text = "FINAL SCORE   --";
            bestScoreText.text = "PERSONAL BEST   --";
        }

        if (backdrop != null)
        {
            backdrop.SetActive(true);
        }

        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void BuildPanel()
    {
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        if (panelRect == null)
        {
            Debug.LogError("GameOverUI: GameOver panel cần có RectTransform.");
            return;
        }

        Transform canvasTransform = panelRect.parent;
        if (canvasTransform != null)
        {
            backdrop = new GameObject(
                "GameOverBackdrop",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );
            backdrop.layer = canvasTransform.gameObject.layer;

            RectTransform backdropRect = backdrop.GetComponent<RectTransform>();
            backdropRect.SetParent(canvasTransform, false);
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.offsetMin = Vector2.zero;
            backdropRect.offsetMax = Vector2.zero;
            backdropRect.SetAsFirstSibling();

            Image backdropImage = backdrop.GetComponent<Image>();
            backdropImage.color = new Color(0.015f, 0.035f, 0.09f, 0.85f);
            backdropImage.raycastTarget = true;
            backdrop.SetActive(false);

            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();
            gameOverPanel.SetActive(false);
        }

        panelRect.sizeDelta = new Vector2(520f, 480f);

        Image panelImage = gameOverPanel.GetComponent<Image>();
        if (panelImage == null)
        {
            panelImage = gameOverPanel.AddComponent<Image>();
        }

        panelImage.color = new Color(0.025f, 0.065f, 0.14f, 0.98f);
        panelImage.raycastTarget = true;

        Outline outline = gameOverPanel.GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameOverPanel.AddComponent<Outline>();
        }

        outline.effectColor = new Color(0.13f, 0.82f, 1f, 0.9f);
        outline.effectDistance = new Vector2(3f, -3f);

        for (int i = gameOverPanel.transform.childCount - 1; i >= 0; i--)
        {
            gameOverPanel.transform.GetChild(i).gameObject.SetActive(false);
        }

        CreateBar(panelRect, "TopAccent", new Vector2(0f, 230f), new Vector2(520f, 8f),
            new Color(0.13f, 0.82f, 1f, 1f));

        titleText = CreateText(
            panelRect,
            "ResultTitle",
            "MISSION FAILED",
            32f,
            new Color(1f, 0.25f, 0.35f, 1f),
            new Vector2(0f, 180f),
            new Vector2(480f, 50f)
        );

        subtitleText = CreateText(
            panelRect,
            "ResultSubtitle",
            "Phi thuyền bị hư hại nghiêm trọng trong vành đai thiên thạch.",
            15f,
            new Color(0.68f, 0.82f, 0.94f, 1f),
            new Vector2(0f, 140f),
            new Vector2(480f, 36f)
        );

        finalScoreText = CreateText(
            panelRect,
            "FinalScore",
            "FINAL SCORE   0",
            25f,
            new Color(0.2f, 0.92f, 1f, 1f),
            new Vector2(0f, 85f),
            new Vector2(480f, 44f)
        );

        bestScoreText = CreateText(
            panelRect,
            "BestScore",
            "PERSONAL BEST   0",
            17f,
            new Color(1f, 0.82f, 0.25f, 1f),
            new Vector2(0f, 45f),
            new Vector2(480f, 32f)
        );

        statsText = CreateText(
            panelRect,
            "StatsSummary",
            "STARS: 0    |    KILLS: 0",
            16f,
            new Color(0.55f, 0.95f, 0.65f, 1f),
            new Vector2(0f, 10f),
            new Vector2(480f, 30f)
        );

        CreateButton(panelRect, "RetryButton", "PLAY AGAIN (CHƠI LẠI)", new Vector2(0f, -48f),
            new Color(0.08f, 0.58f, 0.75f, 1f), Retry);
        CreateButton(panelRect, "MainMenuButton", "MAIN MENU", new Vector2(0f, -112f),
            new Color(0.14f, 0.22f, 0.38f, 1f), MainMenu);
        CreateButton(panelRect, "QuitButton", "QUIT (THOÁT)", new Vector2(0f, -176f),
            new Color(0.32f, 0.16f, 0.22f, 1f), Quit);
    }

    private TMP_Text CreateText(
        Transform parent,
        string objectName,
        string value,
        float fontSize,
        Color color,
        Vector2 position,
        Vector2 size
    )
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        textObject.layer = parent.gameObject.layer;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = interfaceFont;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = fontSize * 0.75f;
        text.fontSizeMax = fontSize;
        text.raycastTarget = false;
        return text;
    }

    private static void CreateBar(
        Transform parent,
        string objectName,
        Vector2 position,
        Vector2 size,
        Color color
    )
    {
        GameObject bar = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        bar.layer = parent.gameObject.layer;

        RectTransform rect = bar.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        bar.GetComponent<Image>().color = color;
    }

    private void CreateButton(
        Transform parent,
        string objectName,
        string label,
        Vector2 position,
        Color color,
        UnityEngine.Events.UnityAction onClick
    )
    {
        GameObject buttonObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );
        buttonObject.layer = parent.gameObject.layer;

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(380f, 52f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = color;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.22f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.22f);
        button.colors = colors;

        TMP_Text buttonLabel = CreateText(
            rect,
            "Label",
            label,
            19f,
            Color.white,
            Vector2.zero,
            new Vector2(360f, 48f)
        );
        buttonLabel.fontStyle = FontStyles.Bold;
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(sceneIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt(ScoreManager.StartLevelKey, 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
