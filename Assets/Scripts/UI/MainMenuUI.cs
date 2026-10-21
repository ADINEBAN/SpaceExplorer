using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Giao diện Menu chính (Main Menu):
/// Thiết kế Sci-Fi sắc nét, tương phản cao, không bị mờ.
/// Tích hợp trực tiếp:
/// 1. Bộ chọn Phi Thuyền (3 mẫu: Alpha Scout, Nebula Vanguard, Void Phantom)
///    kèm Live Preview và các nút chọn trực tiếp ngay màn hình.
/// 2. Bộ chọn Vũ Khí Laser (4 loại: Cyan, Crimson, Emerald, Golden)
///    kèm Live Preview và nút chọn trực tiếp.
/// 3. Bảng chọn 4 Mức Độ Khó (Easy, Normal, Hard, Nightmare) trực quan, đổi tức thì.
/// 4. Nút Launch Mission, Flight Manual và Quit Game.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Sprite menuBackground;

    private readonly Color neonCyan = new Color(0f, 0.92f, 1f, 1f);
    private readonly Color cardBgColor = new Color(0.015f, 0.045f, 0.11f, 0.98f);
    private readonly Color subBoxBgColor = new Color(0.03f, 0.08f, 0.18f, 0.96f);

    // Main Panels
    private GameObject mainPanel;
    private GameObject manualPanel;

    // Direct Ship Selector Elements
    private Image shipPreviewImg;
    private TMP_Text shipNameText;
    private TMP_Text shipStatsText;
    private Button[] shipTabButtons;
    private Image[] shipTabBgs;

    // Direct Laser Selector Elements
    private Image laserPreviewImg;
    private TMP_Text laserNameText;
    private TMP_Text laserStatsText;
    private Button[] laserTabButtons;
    private Image[] laserTabBgs;

    // Direct Difficulty Selector Elements
    private Button[] diffButtons;
    private Image[] diffButtonBgs;
    private TMP_Text diffDescText;

    private void Start()
    {
        EnsureEventSystem();
        BuildCrispMainMenu();
        UpdateAllSelections();
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

    private void BuildCrispMainMenu()
    {
        GameObject canvasObject = new GameObject(
            "MainMenuCanvas",
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

        // Background Space
        Image backdrop = CreateImage(
            canvasObject.transform,
            "SpaceBackdrop",
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            new Color(0.01f, 0.02f, 0.06f, 1f)
        );
        StretchToParent(backdrop.rectTransform);
        if (menuBackground != null)
        {
            backdrop.sprite = menuBackground;
            backdrop.color = new Color(0.65f, 0.8f, 1f, 1f);
        }

        // --- MAIN PANEL ---
        mainPanel = new GameObject("MainPanel", typeof(RectTransform));
        RectTransform mainRect = mainPanel.GetComponent<RectTransform>();
        mainRect.SetParent(canvasObject.transform, false);
        StretchToParent(mainRect);

        // Khung trung tâm sắc nét, tương phản cao
        RectTransform card = CreateRect(
            mainRect,
            "CrispMenuCard",
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Vector2(1080f, 940f)
        );

        Image cardImg = card.gameObject.AddComponent<Image>();
        cardImg.color = cardBgColor;

        Outline cardOutline = card.gameObject.AddComponent<Outline>();
        cardOutline.effectColor = new Color(0f, 0.88f, 1f, 0.95f);
        cardOutline.effectDistance = new Vector2(2.5f, -2.5f);

        CreateImage(card, "TopNeonBar", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), neonCyan, new Vector2(960f, 6f));

        // Tiêu đề game
        CreateText(card, "GameTitle", "SPACE EXPLORER", 48f, Color.white, new Vector2(0f, 415f), new Vector2(850f, 56f), FontStyles.Bold);
        CreateText(card, "GameSubtitle", "NEBULA SURVIVOR — CHỌN PHI THUYỀN, VŨ KHÍ & ĐỘ KHÓ TRƯỚC KHI XUẤT KÍCH", 15f, new Color(0.7f, 0.88f, 1f, 1f), new Vector2(0f, 375f), new Vector2(900f, 28f));

        // =====================================================================
        // SECTION 1: CHỌN PHI THUYỀN TRỰC TIẾP (DIRECT SHIP LOADOUT)
        // =====================================================================
        RectTransform shipBox = CreateRect(card, "ShipBox", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-255f, 175f), new Vector2(490f, 320f));
        Image shipBg = shipBox.gameObject.AddComponent<Image>();
        shipBg.color = subBoxBgColor;
        Outline shipOutline = shipBox.gameObject.AddComponent<Outline>();
        shipOutline.effectColor = new Color(0.18f, 0.8f, 1f, 0.75f);

        CreateText(shipBox, "Header", "1. CHỌN PHI THUYỀN (PILOT CRAFT)", 18f, neonCyan, new Vector2(0f, 138f), new Vector2(460f, 28f), FontStyles.Bold);

        // Preview Phi Thuyền
        RectTransform shipPreviewContainer = CreateRect(shipBox, "ShipImgBox", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 55f), new Vector2(130f, 120f));
        Image shipBoxInner = shipPreviewContainer.gameObject.AddComponent<Image>();
        shipBoxInner.color = new Color(0.01f, 0.03f, 0.08f, 0.9f);

        GameObject shipSpriteObj = new GameObject("ShipSprite", typeof(RectTransform), typeof(Image));
        shipSpriteObj.transform.SetParent(shipPreviewContainer, false);
        shipPreviewImg = shipSpriteObj.GetComponent<Image>();
        shipPreviewImg.rectTransform.sizeDelta = new Vector2(115f, 115f);
        shipPreviewImg.preserveAspect = true;

        CreateMenuButton(shipBox, "PrevShip", "<", new Vector2(-125f, 55f), new Vector2(44f, 44f), new Color(0.1f, 0.35f, 0.6f, 1f), PrevShip);
        CreateMenuButton(shipBox, "NextShip", ">", new Vector2(125f, 55f), new Vector2(44f, 44f), new Color(0.1f, 0.35f, 0.6f, 1f), NextShip);

        shipNameText = CreateText(shipBox, "ShipName", "", 19f, Color.white, new Vector2(0f, -22f), new Vector2(460f, 28f), FontStyles.Bold);

        // 5 Nút chọn trực tiếp 5 phi thuyền của người chơi
        shipTabButtons = new Button[GameLoadoutManager.Ships.Length];
        shipTabBgs = new Image[GameLoadoutManager.Ships.Length];
        float shipBtnWidth = 86f;
        float shipBtnSpacing = 92f;
        float shipStartX = -(GameLoadoutManager.Ships.Length - 1) * 0.5f * shipBtnSpacing;
        for (int i = 0; i < GameLoadoutManager.Ships.Length; i++)
        {
            int shipIdx = i;
            GameLoadoutManager.ShipData s = GameLoadoutManager.Ships[i];
            GameObject sBtn = CreateButtonObject(shipBox, $"ShipBtn_{i}", new Vector2(shipStartX + i * shipBtnSpacing, -60f), new Vector2(shipBtnWidth, 34f), s.accentColor * 0.35f);
            shipTabButtons[i] = sBtn.GetComponent<Button>();
            shipTabBgs[i] = sBtn.GetComponent<Image>();
            shipTabButtons[i].onClick.AddListener(() => SetShip(shipIdx));
            CreateText(sBtn.transform, "Label", s.id, 11.5f, Color.white, Vector2.zero, new Vector2(shipBtnWidth - 4f, 30f), FontStyles.Bold);
        }

        shipStatsText = CreateText(shipBox, "ShipStats", "", 13f, new Color(0.35f, 0.95f, 0.65f, 1f), new Vector2(0f, -108f), new Vector2(460f, 28f), FontStyles.Bold);

        // =====================================================================
        // SECTION 2: CHỌN VŨ KHÍ LASER TRỰC TIẾP (DIRECT LASER LOADOUT)
        // =====================================================================
        RectTransform laserBox = CreateRect(card, "LaserBox", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(255f, 175f), new Vector2(490f, 320f));
        Image laserBg = laserBox.gameObject.AddComponent<Image>();
        laserBg.color = subBoxBgColor;
        Outline laserOutline = laserBox.gameObject.AddComponent<Outline>();
        laserOutline.effectColor = new Color(1f, 0.35f, 0.65f, 0.75f);

        CreateText(laserBox, "Header", "2. CHỌN VŨ KHÍ (LASER CANNON)", 18f, new Color(1f, 0.45f, 0.7f, 1f), new Vector2(0f, 138f), new Vector2(460f, 28f), FontStyles.Bold);

        // Preview Laser
        RectTransform laserPreviewContainer = CreateRect(laserBox, "LaserImgBox", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(180f, 70f));
        Image laserBoxInner = laserPreviewContainer.gameObject.AddComponent<Image>();
        laserBoxInner.color = new Color(0.01f, 0.03f, 0.08f, 0.9f);

        GameObject laserSpriteObj = new GameObject("LaserSprite", typeof(RectTransform), typeof(Image));
        laserSpriteObj.transform.SetParent(laserPreviewContainer, false);
        laserPreviewImg = laserSpriteObj.GetComponent<Image>();
        laserPreviewImg.rectTransform.sizeDelta = new Vector2(160f, 45f);
        laserPreviewImg.preserveAspect = true;

        CreateMenuButton(laserBox, "PrevLaser", "<", new Vector2(-130f, 60f), new Vector2(44f, 44f), new Color(0.4f, 0.18f, 0.38f, 1f), PrevLaser);
        CreateMenuButton(laserBox, "NextLaser", ">", new Vector2(130f, 60f), new Vector2(44f, 44f), new Color(0.4f, 0.18f, 0.38f, 1f), NextLaser);

        laserNameText = CreateText(laserBox, "LaserName", "", 18f, Color.white, new Vector2(0f, -16f), new Vector2(460f, 26f), FontStyles.Bold);

        // 9 Nút chọn trực tiếp 9 loại laser (chia 2 hàng)
        laserTabButtons = new Button[GameLoadoutManager.Lasers.Length];
        laserTabBgs = new Image[GameLoadoutManager.Lasers.Length];

        // Hàng 1: 5 laser đầu (Blaster 1 - 5)
        float row1Spacing = 92f;
        float row1StartX = -2 * row1Spacing;
        for (int i = 0; i < 5 && i < GameLoadoutManager.Lasers.Length; i++)
        {
            int laserIdx = i;
            GameLoadoutManager.LaserData l = GameLoadoutManager.Lasers[i];
            GameObject lBtn = CreateButtonObject(laserBox, $"LaserBtn_{i}", new Vector2(row1StartX + i * row1Spacing, -48f), new Vector2(86f, 26f), l.laserColor * 0.35f);
            laserTabButtons[i] = lBtn.GetComponent<Button>();
            laserTabBgs[i] = lBtn.GetComponent<Image>();
            laserTabButtons[i].onClick.AddListener(() => SetLaser(laserIdx));
            CreateText(lBtn.transform, "Label", $"B0{i + 1}", 11f, Color.white, Vector2.zero, new Vector2(82f, 24f), FontStyles.Bold);
        }

        // Hàng 2: các laser còn lại
        int remaining = GameLoadoutManager.Lasers.Length - 5;
        float row2Spacing = 112f;
        float row2StartX = -(remaining - 1) * 0.5f * row2Spacing;
        for (int i = 5; i < GameLoadoutManager.Lasers.Length; i++)
        {
            int laserIdx = i;
            int rIdx = i - 5;
            GameLoadoutManager.LaserData l = GameLoadoutManager.Lasers[i];
            GameObject lBtn = CreateButtonObject(laserBox, $"LaserBtn_{i}", new Vector2(row2StartX + rIdx * row2Spacing, -78f), new Vector2(106f, 26f), l.laserColor * 0.35f);
            laserTabButtons[i] = lBtn.GetComponent<Button>();
            laserTabBgs[i] = lBtn.GetComponent<Image>();
            laserTabButtons[i].onClick.AddListener(() => SetLaser(laserIdx));
            CreateText(lBtn.transform, "Label", l.id, 10.5f, Color.white, Vector2.zero, new Vector2(102f, 24f), FontStyles.Bold);
        }

        laserStatsText = CreateText(laserBox, "LaserStats", "", 12.5f, new Color(1f, 0.85f, 0.3f, 1f), new Vector2(0f, -108f), new Vector2(460f, 26f), FontStyles.Bold);

        // =====================================================================
        // SECTION 3: CHỌN ĐỘ KHÓ TRỰC TIẾP (4 DIFFICULTY BUTTONS)
        // =====================================================================
        RectTransform diffSection = CreateRect(card, "DiffSection", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(1000f, 142f));
        Image diffSecBg = diffSection.gameObject.AddComponent<Image>();
        diffSecBg.color = subBoxBgColor;
        Outline diffSecOutline = diffSection.gameObject.AddComponent<Outline>();
        diffSecOutline.effectColor = new Color(0.55f, 0.35f, 0.9f, 0.7f);

        CreateText(diffSection, "DiffHeader", "3. CHỌN MỨC ĐỘ KHÓ CHIẾN DỊCH (DIFFICULTY)", 17f, new Color(0.85f, 0.65f, 1f, 1f), new Vector2(0f, 46f), new Vector2(850f, 26f), FontStyles.Bold);

        diffButtons = new Button[GameLoadoutManager.Difficulties.Length];
        diffButtonBgs = new Image[GameLoadoutManager.Difficulties.Length];

        float startBtnX = -345f;
        float btnSpacing = 230f;
        for (int i = 0; i < GameLoadoutManager.Difficulties.Length; i++)
        {
            int diffIdx = i;
            GameLoadoutManager.DifficultyData d = GameLoadoutManager.Difficulties[i];

            GameObject btnObj = CreateButtonObject(
                diffSection,
                $"DiffBtn_{i}",
                new Vector2(startBtnX + i * btnSpacing, 6f),
                new Vector2(215f, 40f),
                d.themeColor * 0.35f
            );

            diffButtons[i] = btnObj.GetComponent<Button>();
            diffButtonBgs[i] = btnObj.GetComponent<Image>();
            diffButtons[i].onClick.AddListener(() => SetDifficulty(diffIdx));

            CreateText(btnObj.transform, "Label", d.id, 16f, Color.white, Vector2.zero, new Vector2(205f, 36f), FontStyles.Bold);
        }

        diffDescText = CreateText(diffSection, "DiffDesc", "", 14f, new Color(0.85f, 0.95f, 1f, 1f), new Vector2(0f, -44f), new Vector2(980f, 44f));
        diffDescText.textWrappingMode = TextWrappingModes.Normal;

        // =====================================================================
        // SECTION 4: HÀNG NÚT ĐIỀU HƯỚNG CHÍNH
        // =====================================================================
        CreateMenuButton(card, "LaunchBtn", "LAUNCH MISSION (XUẤT KÍCH)", new Vector2(0f, -195f), new Vector2(640f, 66f), neonCyan, StartGame);
        CreateMenuButton(card, "ManualBtn", "FLIGHT MANUAL (HƯỚNG DẪN)", new Vector2(0f, -275f), new Vector2(640f, 52f), new Color(0.12f, 0.25f, 0.45f, 1f), ShowManual);
        CreateMenuButton(card, "QuitBtn", "QUIT GAME (THOÁT)", new Vector2(0f, -345f), new Vector2(640f, 48f), new Color(0.35f, 0.15f, 0.22f, 1f), Application.Quit);

        CreateText(card, "Hint", "ĐIỀU KHIỂN: WASD / ARROWS DI CHUYỂN   •   CHUỘT NGẮM 360°   •   CHUỘT TRÁI / SPACE BẮN", 13f, new Color(0.6f, 0.78f, 0.92f, 1f), new Vector2(0f, -425f), new Vector2(900f, 24f));

        // --- MANUAL PANEL ---
        BuildManualPanel(canvasObject.transform);
    }

    private void BuildManualPanel(Transform parent)
    {
        manualPanel = new GameObject("ManualPanel", typeof(RectTransform));
        RectTransform panelRect = manualPanel.GetComponent<RectTransform>();
        panelRect.SetParent(parent, false);
        StretchToParent(panelRect);

        RectTransform card = CreateRect(panelRect, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 850f));
        Image cardImg = card.gameObject.AddComponent<Image>();
        cardImg.color = cardBgColor;
        Outline outline = card.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(neonCyan.r, neonCyan.g, neonCyan.b, 0.95f);

        CreateText(card, "Title", "PILOT FLIGHT MANUAL", 36f, neonCyan, new Vector2(0f, 350f), new Vector2(750f, 50f), FontStyles.Bold);

        string manualContent =
            "<color=#00E5FF><b>DI CHUYỂN 8 HƯỚNG</b></color>\n" +
            "Dùng phím mũi tên (Arrow Keys) hoặc WASD để di chuyển tự do khắp màn chơi.\n\n" +
            "<color=#00E5FF><b>NGẮM BẮN 360 ĐỘ</b></color>\n" +
            "Di chuyển chuột xung quanh tàu để ngắm tự do 360 độ. Mũi tàu xoay mượt theo con trỏ.\n\n" +
            "<color=#00E5FF><b>BẮN LASER LIÊN TỤC</b></color>\n" +
            "Nhấp hoặc giữ Chuột Trái (Left Click) hoặc phím Space để xả đạn liên tục theo tốc độ nạp.\n\n" +
            "<color=#FFD700><b>THU THẬP SAO (STARS)</b></color>\n" +
            "Thu thập các ngôi sao vàng để cộng +100 điểm. Mỗi sao chỉ được ăn một lần.\n\n" +
            "<color=#FF4444><b>TIỂU HÀNH TINH (ASTEROIDS)</b></color>\n" +
            "Bắn hạ thiên thạch để nhận điểm thưởng. Va chạm trực tiếp sẽ trừ điểm và kết thúc lượt bay!\n\n" +
            "<color=#BD00FF><b>HẬU CẢNH VŨ TRỤ PARALLAX & CÁC TIỂU HÀNH TINH</b></color>\n" +
            "Các hành tinh (Sao Mộc, Sao Hỏa, Sao Thủy, Hải Vương) xoay tròn và trôi đa tầng phản hồi theo chuyển động của tàu.";

        CreateText(card, "Body", manualContent, 17f, Color.white, new Vector2(0f, 0f), new Vector2(780f, 560f));

        CreateMenuButton(card, "BackBtn", "BACK TO MENU", new Vector2(0f, -340f), new Vector2(380f, 56f), new Color(0.12f, 0.25f, 0.45f, 1f), ShowMain);

        manualPanel.SetActive(false);
    }

    private void SetShip(int idx)
    {
        GameLoadoutManager.SelectedShipIndex = idx;
        UpdateAllSelections();
    }

    private void PrevShip()
    {
        int cur = GameLoadoutManager.SelectedShipIndex;
        SetShip((cur - 1 + GameLoadoutManager.Ships.Length) % GameLoadoutManager.Ships.Length);
    }

    private void NextShip()
    {
        int cur = GameLoadoutManager.SelectedShipIndex;
        SetShip((cur + 1) % GameLoadoutManager.Ships.Length);
    }

    private void SetLaser(int idx)
    {
        GameLoadoutManager.SelectedLaserIndex = idx;
        UpdateAllSelections();
    }

    private void PrevLaser()
    {
        int cur = GameLoadoutManager.SelectedLaserIndex;
        SetLaser((cur - 1 + GameLoadoutManager.Lasers.Length) % GameLoadoutManager.Lasers.Length);
    }

    private void NextLaser()
    {
        int cur = GameLoadoutManager.SelectedLaserIndex;
        SetLaser((cur + 1) % GameLoadoutManager.Lasers.Length);
    }

    private void SetDifficulty(int idx)
    {
        GameLoadoutManager.SelectedDifficultyIndex = idx;
        UpdateAllSelections();
    }

    private void UpdateAllSelections()
    {
        // 1. Cập nhật Ship
        int curShipIdx = GameLoadoutManager.SelectedShipIndex;
        GameLoadoutManager.ShipData ship = GameLoadoutManager.CurrentShip;
        shipNameText.text = $"{ship.name}  <size=75%><color=#88CCFF>({ship.title})</color></size>";
        shipStatsText.text = $"Speed: {ship.moveSpeed:F0}   |   Agility: {ship.rotationSpeed:F0}°/s";

        Sprite loadedShipSprite = Resources.Load<Sprite>(ship.spriteResourcePath);
        if (loadedShipSprite != null && shipPreviewImg != null)
        {
            shipPreviewImg.sprite = loadedShipSprite;
            shipPreviewImg.color = Color.white;
        }

        if (shipTabButtons != null)
        {
            for (int i = 0; i < shipTabButtons.Length; i++)
            {
                bool isCur = (i == curShipIdx);
                GameLoadoutManager.ShipData s = GameLoadoutManager.Ships[i];
                shipTabBgs[i].color = isCur ? s.accentColor : new Color(s.accentColor.r * 0.25f, s.accentColor.g * 0.25f, s.accentColor.b * 0.25f, 0.9f);
                shipTabButtons[i].transform.localScale = isCur ? Vector3.one * 1.06f : Vector3.one;
            }
        }

        // 2. Cập nhật Laser
        int curLaserIdx = GameLoadoutManager.SelectedLaserIndex;
        GameLoadoutManager.LaserData laser = GameLoadoutManager.CurrentLaser;
        laserNameText.text = $"{laser.name}  <size=75%><color=#FFAACC>({laser.title})</color></size>";
        laserStatsText.text = $"Damage: {laser.damage} HP   |   Fire Rate: {1f / laser.fireCooldown:F1} rps";

        Sprite loadedLaserSprite = Resources.Load<Sprite>(laser.spriteResourcePath);
        if (loadedLaserSprite != null && laserPreviewImg != null)
        {
            laserPreviewImg.sprite = loadedLaserSprite;
            laserPreviewImg.color = laser.laserColor;
        }

        if (laserTabButtons != null)
        {
            for (int i = 0; i < laserTabButtons.Length; i++)
            {
                bool isCur = (i == curLaserIdx);
                GameLoadoutManager.LaserData l = GameLoadoutManager.Lasers[i];
                laserTabBgs[i].color = isCur ? l.laserColor : new Color(l.laserColor.r * 0.25f, l.laserColor.g * 0.25f, l.laserColor.b * 0.25f, 0.9f);
                laserTabButtons[i].transform.localScale = isCur ? Vector3.one * 1.06f : Vector3.one;
            }
        }

        // 3. Cập nhật Difficulty
        int selectedDiff = GameLoadoutManager.SelectedDifficultyIndex;
        if (diffButtons != null)
        {
            for (int i = 0; i < diffButtons.Length; i++)
            {
                bool isSelected = (i == selectedDiff);
                GameLoadoutManager.DifficultyData d = GameLoadoutManager.Difficulties[i];

                diffButtonBgs[i].color = isSelected
                    ? d.themeColor
                    : new Color(d.themeColor.r * 0.28f, d.themeColor.g * 0.28f, d.themeColor.b * 0.28f, 0.92f);

                diffButtons[i].transform.localScale = isSelected ? Vector3.one * 1.08f : Vector3.one;
            }
        }

        GameLoadoutManager.DifficultyData curDiff = GameLoadoutManager.CurrentDifficulty;
        diffDescText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(curDiff.themeColor)}><b>[{curDiff.name}]</b></color>: {curDiff.description} (Tốc độ thiên thạch: x{curDiff.asteroidSpeedMult:F2}, Nhịp spawn: {curDiff.spawnInterval:F1}s)";
    }

    private void StartGame()
    {
        PlayerPrefs.SetInt(ScoreManager.StartLevelKey, 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Level1_EarthOrbit");
    }

    private void ShowManual()
    {
        mainPanel.SetActive(false);
        manualPanel.SetActive(true);
    }

    private void ShowMain()
    {
        manualPanel.SetActive(false);
        mainPanel.SetActive(true);
        UpdateAllSelections();
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
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.25f);
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
        text.fontSizeMin = fontSize * 0.8f;
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
