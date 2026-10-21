using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Giao diện màn hình EndGame riêng biệt (Scene EndGame.unity):
/// Hiển thị Điểm số cuối cùng, Kỷ lục, Thống kê, nút Chơi Lại, Về Menu và Thoát.
/// Tự động dựng giao diện sci-fi đẹp mắt khi Scene được tải.
/// </summary>
public class EndGameUI : MonoBehaviour
{
    private const string BestScoreKey = "SpaceExplorer.BestScore";

    [SerializeField] private Sprite endBackground;

    private readonly Color accentCyan = new Color(0.18f, 0.88f, 1f, 1f);
    private readonly Color panelBgColor = new Color(0.02f, 0.05f, 0.12f, 0.96f);

    private void Start()
    {
        Time.timeScale = 1f;
        EnsureEventSystem();
        BuildEndGameScreen();
    }

    private void EnsureEventSystem()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule)
            );

            InputSystemUIInputModule inputModule = eventSystemObject.GetComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }
    }

    private void BuildEndGameScreen()
    {
        GameObject canvasObject = new GameObject(
            "EndGameCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Background
        Image backdrop = CreateImage(
            canvasObject.transform,
            "Backdrop",
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            new Color(0.015f, 0.035f, 0.09f, 1f)
        );
        StretchToParent(backdrop.rectTransform);
        if (endBackground != null)
        {
            backdrop.sprite = endBackground;
            backdrop.color = new Color(0.55f, 0.72f, 0.95f, 1f);
        }

        Image tint = CreateImage(
            canvasObject.transform,
            "Tint",
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            new Color(0.01f, 0.03f, 0.08f, 0.6f)
        );
        StretchToParent(tint.rectTransform);
        tint.raycastTarget = false;

        // Result Card
        RectTransform card = CreateRect(
            canvasObject.transform,
            "ResultCard",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Vector2(720f, 780f)
        );

        Image cardImage = card.gameObject.AddComponent<Image>();
        cardImage.color = panelBgColor;

        Outline outline = card.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(accentCyan.r, accentCyan.g, accentCyan.b, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);

        CreateImage(card, "TopAccent", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), accentCyan, new Vector2(580f, 6f));

        int bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);

        CreateText(card, "Title", "MISSION DEBRIEFING", 44f, Color.white, new Vector2(0f, 310f), new Vector2(640f, 60f), FontStyles.Bold);
        CreateText(card, "Subtitle", "Chuyến thám hiểm không gian đã hoàn tất.", 17f, new Color(0.65f, 0.85f, 1f, 1f), new Vector2(0f, 255f), new Vector2(640f, 32f));

        // Score Card
        RectTransform scoreBox = CreateRect(card, "ScoreBox", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(580f, 180f));
        Image scoreBoxBg = scoreBox.gameObject.AddComponent<Image>();
        scoreBoxBg.color = new Color(0.04f, 0.09f, 0.19f, 0.95f);
        Outline boxOutline = scoreBox.gameObject.AddComponent<Outline>();
        boxOutline.effectColor = new Color(0.2f, 0.6f, 0.9f, 0.5f);

        CreateText(scoreBox, "PersonalBest", $"PERSONAL BEST: {bestScore:N0}", 28f, accentCyan, new Vector2(0f, 30f), new Vector2(540f, 45f), FontStyles.Bold);

        GameLoadoutManager.DifficultyData diff = GameLoadoutManager.CurrentDifficulty;
        string hexColor = ColorUtility.ToHtmlStringRGB(diff.themeColor);
        CreateText(scoreBox, "DiffBadge", $"DIFFICULTY LEVEL: <color=#{hexColor}>[{diff.name}]</color>", 18f, Color.white, new Vector2(0f, -30f), new Vector2(540f, 35f));

        // Buttons
        CreateMenuButton(card, "RetryBtn", "PLAY AGAIN (CHƠI LẠI)", new Vector2(0f, -60f), new Vector2(540f, 66f), accentCyan, RetryMission);
        CreateMenuButton(card, "MainMenuBtn", "MAIN MENU", new Vector2(0f, -145f), new Vector2(540f, 64f), new Color(0.12f, 0.28f, 0.48f, 1f), GoToMainMenu);
        CreateMenuButton(card, "QuitBtn", "QUIT GAME (THOÁT)", new Vector2(0f, -230f), new Vector2(540f, 60f), new Color(0.35f, 0.15f, 0.22f, 1f), Application.Quit);
    }

    private void RetryMission()
    {
        SceneManager.LoadScene("Level1_EarthOrbit");
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private static GameObject CreateButtonObject(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = color;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.22f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.25f);
        button.colors = colors;

        return buttonObject;
    }

    private static void CreateMenuButton(Transform parent, string name, string label, Vector2 position, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = CreateButtonObject(parent, name, position, size, color);
        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(onClick);

        CreateText(buttonObject.transform, "Label", label, 20f, Color.white, Vector2.zero, size, FontStyles.Bold);
    }

    private static RectTransform CreateRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        GameObject child = new GameObject(name, typeof(RectTransform));
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static Image CreateImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Color color, Vector2? size = null)
    {
        GameObject child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = position;
        if (size.HasValue) rect.sizeDelta = size.Value;
        Image image = child.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, float fontSize, Color color, Vector2 position, Vector2 size, FontStyles style = FontStyles.Normal)
    {
        GameObject child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = UIFontHelper.GetCrispFont();
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = fontSize * 0.7f;
        text.fontSizeMax = fontSize;
        text.raycastTarget = false;
        return text;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.anchoredPosition = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
