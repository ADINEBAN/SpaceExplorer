using System;
using UnityEngine;

/// <summary>
/// Quản lý cấu hình tập trung cho Loadout (Phi thuyền, Vũ khí) và Độ khó (Difficulty).
/// Lưu trữ trạng thái qua PlayerPrefs và cung cấp dữ liệu tức thì cho mọi Scene.
/// </summary>
public static class GameLoadoutManager
{
    private const string PrefShip = "SpaceExplorer.SelectedShip";
    private const string PrefLaser = "SpaceExplorer.SelectedLaser";
    private const string PrefDifficulty = "SpaceExplorer.SelectedDifficulty";

    // ----------------------------------------------------
    // CẤU HÌNH ĐỘ KHÓ (DIFFICULTY)
    // Thiết lập khác biệt rõ rệt giữa các cấp độ để người chơi cảm nhận rõ ràng
    // ----------------------------------------------------
    [Serializable]
    public class DifficultyData
    {
        public string id;
        public string name;
        public string shortTag;
        public string description;
        public Color themeColor;
        public float spawnInterval;       // Nhịp sinh thiên thạch
        public float asteroidSpeedMult;   // Tốc độ bay của thiên thạch
        public int maxSimultaneousAsteroids; // Số lượng tối đa trên màn hình
        public float starInterval;         // Nhịp sinh sao thưởng
        public float starSpeed;           // Tốc độ sao
        public int collisionPenalty;       // Mức trừ điểm khi va chạm
    }

    public static readonly DifficultyData[] Difficulties = new DifficultyData[]
    {
        new DifficultyData
        {
            id = "EASY",
            name = "EASY (TẬP SỰ)",
            shortTag = "EASY",
            description = "Mật độ thiên thạch thưa thớt, tốc độ chậm rãi. Thích hợp cho tân phi công làm quen.",
            themeColor = new Color(0.2f, 0.95f, 0.45f, 1f),
            spawnInterval = 2.6f,
            asteroidSpeedMult = 0.65f,
            maxSimultaneousAsteroids = 6,
            starInterval = 2.2f,
            starSpeed = 1.3f,
            collisionPenalty = 10
        },
        new DifficultyData
        {
            id = "NORMAL",
            name = "NORMAL (TIÊU CHUẨN)",
            shortTag = "NORMAL",
            description = "Môi trường cân bằng chuẩn arcade. Thử thách vừa phải, tốc độ ổn định.",
            themeColor = new Color(0.18f, 0.88f, 1f, 1f),
            spawnInterval = 1.4f,
            asteroidSpeedMult = 1.0f,
            maxSimultaneousAsteroids = 12,
            starInterval = 2.8f,
            starSpeed = 1.8f,
            collisionPenalty = 25
        },
        new DifficultyData
        {
            id = "HARD",
            name = "HARD (THỬ THÁCH)",
            shortTag = "HARD",
            description = "Thiên thạch lao nhanh, tần suất xuất hiện dồn dập. Đòi hỏi phản xạ và kỹ năng né tránh cao.",
            themeColor = new Color(1f, 0.62f, 0.1f, 1f),
            spawnInterval = 0.8f,
            asteroidSpeedMult = 1.55f,
            maxSimultaneousAsteroids = 20,
            starInterval = 3.4f,
            starSpeed = 2.4f,
            collisionPenalty = 45
        },
        new DifficultyData
        {
            id = "NIGHTMARE",
            name = "NIGHTMARE (ÁC MỘNG)",
            shortTag = "NIGHTMARE",
            description = "Bão mảnh vỡ vũ trụ hủy diệt cực kỳ tàn khốc! Thiên thạch xé gió tốc độ cao tràn ngập màn hình.",
            themeColor = new Color(1f, 0.18f, 0.32f, 1f),
            spawnInterval = 0.38f,
            asteroidSpeedMult = 2.20f,
            maxSimultaneousAsteroids = 32,
            starInterval = 4.0f,
            starSpeed = 3.0f,
            collisionPenalty = 75
        }
    };

    // ----------------------------------------------------
    // CẤU HÌNH PHI THUYỀN (SHIP LOADOUT)
    // ----------------------------------------------------
    [Serializable]
    public class ShipData
    {
        public string id;
        public string name;
        public string title;
        public string description;
        public float moveSpeed;
        public float rotationSpeed;
        public Color accentColor;
        public string spriteResourcePath;
    }

    public static readonly ShipData[] Ships = new ShipData[]
    {
        new ShipData
        {
            id = "PHOENIX",
            name = "PHOENIX MK-I",
            title = "Chiến Hạm Cân Bằng",
            description = "Chiến hạm tiên phong cân bằng toàn diện về vận tốc, độ đầm và hỏa lực phòng thủ.",
            moveSpeed = 42f,
            rotationSpeed = 950f,
            accentColor = new Color(1f, 0.68f, 0.1f, 1f),
            spriteResourcePath = "Ships/SpaceShip"
        },
        new ShipData
        {
            id = "VIPER",
            name = "VIPER STRIKER",
            title = "Tiêm Kích Siêu Thanh",
            description = "Thân vỏ khí động học cực nhẹ, động cơ phản lực đôi cho khả năng luồn lách đỉnh cao.",
            moveSpeed = 50f,
            rotationSpeed = 1100f,
            accentColor = new Color(0f, 0.9f, 1f, 1f),
            spriteResourcePath = "Ships/SpaceShip1"
        },
        new ShipData
        {
            id = "FALCON",
            name = "TITAN FALCON",
            title = "Chiến Cơ Titan",
            description = "Được gia cố bằng giáp titan nguyên khối, bay cực kỳ vững vàng qua mọi bão thiên thạch.",
            moveSpeed = 36f,
            rotationSpeed = 850f,
            accentColor = new Color(0.2f, 0.55f, 1f, 1f),
            spriteResourcePath = "Ships/SpaceShip2"
        },
        new ShipData
        {
            id = "HAWK",
            name = "SOLAR HAWK",
            title = "Pháo Hạm Hỏa Long",
            description = "Hấp thụ năng lượng mặt trời để gia tốc, khả năng đổi hướng nhanh và uy lực mạnh mẽ.",
            moveSpeed = 40f,
            rotationSpeed = 900f,
            accentColor = new Color(1f, 0.35f, 0.15f, 1f),
            spriteResourcePath = "Ships/SpaceShip3"
        },
        new ShipData
        {
            id = "SPECTRE",
            name = "VOID SPECTRE",
            title = "Oanh Tạc Hư Không",
            description = "Khung tàng hình góc cạnh tối tân, triệt tiêu quán tính và bẻ lái 360 độ siêu nhạy.",
            moveSpeed = 46f,
            rotationSpeed = 1050f,
            accentColor = new Color(0.75f, 0.25f, 1f, 1f),
            spriteResourcePath = "Ships/SpaceShip4"
        }
    };

    // ----------------------------------------------------
    // CẤU HÌNH VŨ KHÍ (LASER LOADOUT - DÙNG ASSET CỦA NGƯỜI CHƠI)
    // ----------------------------------------------------
    [Serializable]
    public class LaserData
    {
        public string id;
        public string name;
        public string title;
        public string description;
        public float projectileSpeed;
        public float fireCooldown;
        public int damage;
        public Color laserColor;
        public float audioPitch;
        public string spriteResourcePath;
    }

    public static readonly LaserData[] Lasers = new LaserData[]
    {
        new LaserData
        {
            id = "BLASTER 1",
            name = "FLAME BLASTER 01",
            title = "Pháo Hỏa Diễm Cam",
            description = "Đạn plasma nhiệt rực lửa tốc độ cao, độ chính xác ổn định.",
            projectileSpeed = 65f,
            fireCooldown = 0.15f,
            damage = 1,
            laserColor = new Color(1f, 0.45f, 0.05f, 1f),
            audioPitch = 1.1f,
            spriteResourcePath = "Lasers/1"
        },
        new LaserData
        {
            id = "BLASTER 2",
            name = "PLASMA PULSE 02",
            title = "Tia Plasma Lam",
            description = "Tia plasma xanh ngọc ion hóa nhanh, tầm bắn xa và xé gió cực nhanh.",
            projectileSpeed = 70f,
            fireCooldown = 0.12f,
            damage = 1,
            laserColor = new Color(0f, 0.88f, 1f, 1f),
            audioPitch = 1.25f,
            spriteResourcePath = "Lasers/2"
        },
        new LaserData
        {
            id = "BLASTER 3",
            name = "ION BEAM 03",
            title = "Pháo Ion Lục Bảo",
            description = "Chùm hạt ion xanh lục đậm đặc. Sát thương uy lực phá nát các khối đá lớn.",
            projectileSpeed = 62f,
            fireCooldown = 0.18f,
            damage = 2,
            laserColor = new Color(0.1f, 1f, 0.45f, 1f),
            audioPitch = 0.95f,
            spriteResourcePath = "Lasers/3"
        },
        new LaserData
        {
            id = "BLASTER 4",
            name = "PHOTON BOLT 04",
            title = "Đạn Quang Tử",
            description = "Tần số bắn liên hoàn cực cao! Quét sạch mọi vật thể chặn đường.",
            projectileSpeed = 75f,
            fireCooldown = 0.10f,
            damage = 1,
            laserColor = new Color(1f, 0.75f, 0.1f, 1f),
            audioPitch = 1.4f,
            spriteResourcePath = "Lasers/4"
        },
        new LaserData
        {
            id = "BLASTER 5",
            name = "HYPER RAY 05",
            title = "Siêu Laze Thiên Thanh",
            description = "Tụ năng lượng xuyên phá cực đại. Đạn xé tan chướng ngại trong chớp mắt.",
            projectileSpeed = 80f,
            fireCooldown = 0.20f,
            damage = 2,
            laserColor = new Color(0.1f, 0.75f, 1f, 1f),
            audioPitch = 0.9f,
            spriteResourcePath = "Lasers/5"
        },
        new LaserData
        {
            id = "BLASTER 6",
            name = "JADE SPARK 06",
            title = "Tia Xung Ngọc Bích",
            description = "Chùm tia ngọc bích cộng hưởng xung động, cân bằng xuất sắc giữa sát thương và tốc độ.",
            projectileSpeed = 66f,
            fireCooldown = 0.16f,
            damage = 2,
            laserColor = new Color(0.35f, 1f, 0.65f, 1f),
            audioPitch = 1.15f,
            spriteResourcePath = "Lasers/6"
        },
        new LaserData
        {
            id = "CRIMSON",
            name = "CRIMSON TWIN",
            title = "Tia Song Sinh Hủy Diệt",
            description = "Cặp tia song song đỏ thẫm xé toạc không gian. Sát thương cực lớn (3 điểm) hủy diệt thiên thạch.",
            projectileSpeed = 72f,
            fireCooldown = 0.22f,
            damage = 3,
            laserColor = new Color(1f, 0.2f, 0.25f, 1f),
            audioPitch = 0.85f,
            spriteResourcePath = "Lasers/SPR_Laser_CrimsonTwin_02"
        },
        new LaserData
        {
            id = "OVERCHARGE",
            name = "OVERCHARGE",
            title = "Pháo Quá Tải Hư Không",
            description = "Xung điện từ trường cực mạnh. Bắn ra luồng điện quá tải phá hủy mọi mục tiêu.",
            projectileSpeed = 78f,
            fireCooldown = 0.24f,
            damage = 3,
            laserColor = new Color(0.75f, 0.7f, 1f, 1f),
            audioPitch = 0.8f,
            spriteResourcePath = "Lasers/SPR_Laser_Overcharge_07"
        },
        new LaserData
        {
            id = "VOID WAVE",
            name = "VOID WAVE",
            title = "Sóng Xung Kích Tử Thần",
            description = "Làn sóng xung kích tím ma mị lan tỏa, càn quét diện rộng ở tiền phương.",
            projectileSpeed = 58f,
            fireCooldown = 0.15f,
            damage = 2,
            laserColor = new Color(0.85f, 0.3f, 1f, 1f),
            audioPitch = 1.05f,
            spriteResourcePath = "Lasers/SPR_Laser_VoidWave_04"
        }
    };

    // ----------------------------------------------------
    // GETTERS & SETTERS
    // ----------------------------------------------------
    public static int SelectedShipIndex
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(PrefShip, 0), 0, Ships.Length - 1);
        set
        {
            PlayerPrefs.SetInt(PrefShip, Mathf.Clamp(value, 0, Ships.Length - 1));
            PlayerPrefs.Save();
        }
    }

    public static int SelectedLaserIndex
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(PrefLaser, 0), 0, Lasers.Length - 1);
        set
        {
            PlayerPrefs.SetInt(PrefLaser, Mathf.Clamp(value, 0, Lasers.Length - 1));
            PlayerPrefs.Save();
        }
    }

    public static int SelectedDifficultyIndex
    {
        get => Mathf.Clamp(PlayerPrefs.GetInt(PrefDifficulty, 1), 0, Difficulties.Length - 1); // Mặc định Normal (1)
        set
        {
            PlayerPrefs.SetInt(PrefDifficulty, Mathf.Clamp(value, 0, Difficulties.Length - 1));
            PlayerPrefs.Save();
        }
    }

    public static ShipData CurrentShip => Ships[SelectedShipIndex];
    public static LaserData CurrentLaser => Lasers[SelectedLaserIndex];
    public static DifficultyData CurrentDifficulty => Difficulties[SelectedDifficultyIndex];
}
