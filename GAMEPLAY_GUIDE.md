# SPACE EXPLORER — NEBULA SURVIVOR
## Hướng Dẫn Kỹ Thuật, Gameplay, Loadout & Kiểm Thử Toàn Diện

> **Phiên bản Unity**: 6000.5.9f1 (Unity 6)  
> **Render Pipeline**: Universal Render Pipeline (URP 2D)  
> **Hệ thống Input**: Unity Input System Package (`UnityEngine.InputSystem`)  
> **Kiến trúc**: 2D Sci-Fi Arcade Shooter  

---

## 1. Tổng Quan Về Kiến Trúc & Các Cải Tiến Lớn

Dự án đã được nâng cấp toàn diện từ nền tảng hiện có, đáp ứng đầy đủ yêu cầu **Lab 1** cùng 2 tính năng bổ sung bắt buộc: **Tùy biến Loadout & 4 Cấp Độ Khó**, và **Bộ Asset 2D Tự Thiết Kế Hoàn Chỉnh**.

### 1.1 Cơ Chế Di Chuyển & Ngắm Bắn 360° (Ưu Tiên Cao Nhất)
- **Di chuyển**: Phím mũi tên (`Arrow Keys`) hoặc `WASD` di chuyển mượt mà 4 hướng chính và 4 hướng chéo. Sử dụng `Rigidbody2D.MovePosition` với vector chuẩn hóa (`normalized`), có giới hạn biên màn hình (`screenPadding`) không cho phi thuyền bay ra ngoài tầm nhìn camera.
- **Ngắm 360 độ tự do**: Chuyển đổi tọa độ con trỏ chuột (`Mouse.current.position`) sang World Space bằng `mainCamera.ScreenToWorldPoint`. Tính vector hướng ngắm bằng hàm lượng giác `Mathf.Atan2`. Phi thuyền xoay mượt mà 360 độ bằng `Mathf.MoveTowardsAngle` với tốc độ xoay cấu hình (`rotationSpeed`). Xử lý an toàn khi con trỏ chuột nằm ngay trên tàu (`sqrMagnitude < 0.05f`) tránh rung giật hoặc NaN.
- **Bắn Laser mọi hướng**: Nhấp hoặc giữ **Chuột Trái (Left Click)** hoặc **Phím Space** để xả đạn liên tục với fire rate có thể tùy chỉnh. Tia laser xuất hiện tại vị trí `FirePoint` và bay độc lập theo đúng góc ngắm tại thời điểm bắn (`Vector3.right` theo góc xoay cục bộ của đạn). Đạn không bị lệch hướng khi phi thuyền xoay sau khi bắn. Đạn bỏ qua phi thuyền (không gây sát thương cho người chơi) và tự động hủy an toàn khi ra khỏi tầm nhìn camera để bảo vệ FPS.

---

## 2. Hệ Thống Loadout & 4 Cấp Độ Khó (`GameLoadoutManager`)

### 2.1 Ba Mẫu Phi Thuyền Tùy Biến (Player Ships)
1. **ALPHA SCOUT** (`Ship_Scout.png`):
   - *Phân loại*: Tiêm kích trinh sát siêu thanh.
   - *Chỉ số*: Tốc độ di chuyển `27`, Tốc độ xoay `850°/s`.
   - *Đặc điểm*: Nhẹ, cơ động cực cao, màu xanh Cyan & Trắng.
2. **NEBULA VANGUARD** (`Ship_Vanguard.png`):
   - *Phân loại*: Chiến hạm tuần dương titan.
   - *Chỉ số*: Tốc độ di chuyển `23`, Tốc độ xoay `720°/s`.
   - *Đặc điểm*: Thân vỏ giáp titan đầm chắc, màu Tím Nebula & Vàng Ánh Kim.
3. **VOID PHANTOM** (`Ship_Phantom.png`):
   - *Phân loại*: Tiêm kích tàng hình hư không.
   - *Chỉ số*: Tốc độ di chuyển `29`, Tốc độ xoay `950°/s`.
   - *Đặc điểm*: Cánh vát ngược tàng hình góc cạnh, màu Đỏ Thẫm Crimson & Đen Obsidian.

### 2.2 Bốn Loại Vũ Khí Laser (Laser Weapons)
1. **CYAN PLASMA PULSE** (`Laser_Cyan.png`):
   - Tốc độ đạn: `34` | Cooldown: `0.16s` | Sát thương: `1 HP` | Màu: Neon Cyan (#00E5FF).
2. **CRIMSON HYPER-RAY** (`Laser_Crimson.png`):
   - Tốc độ đạn: `42` | Cooldown: `0.26s` | Sát thương: `2 HP` (Gấp đôi) | Màu: Crimson Red (#FF1744).
3. **EMERALD ION BLASTER** (`Laser_Emerald.png`):
   - Tốc độ đạn: `30` | Cooldown: `0.10s` (Bắn liên thanh cực nhanh) | Sát thương: `1 HP` | Màu: Neon Green (#00E676).
4. **SOLAR GOLD BEAM** (`Laser_Gold.png`):
   - Tốc độ đạn: `38` | Cooldown: `0.22s` | Sát thương: `3 HP` (Xuyên phá) | Màu: Solar Gold (#FFD700).

### 2.3 Bốn Mức Độ Khó (Difficulty Modes)
- **EASY (TẬP SỰ)**: Spawn interval `2.2s`, Tốc độ thiên thạch `x0.75`, Mật độ tối đa `8`, Điểm trừ khi va chạm `-15`.
- **NORMAL (TIÊU CHUẨN)**: Spawn interval `1.4s`, Tốc độ thiên thạch `x1.00`, Mật độ tối đa `14`, Điểm trừ khi va chạm `-25`.
- **HARD (THỬ THÁCH)**: Spawn interval `0.85s`, Tốc độ thiên thạch `x1.35`, Mật độ tối đa `20`, Điểm trừ khi va chạm `-40`.
- **NIGHTMARE (ÁC MỘNG)**: Spawn interval `0.5s`, Tốc độ thiên thạch `x1.75`, Mật độ tối đa `28`, Điểm trừ khi va chạm `-60`.

---

## 3. Danh Sách Script & Cấu Trúc File Đã Thêm / Nâng Cấp

| File Script | Trạng Thái | Mô Tả Chức Năng |
|---|---|---|
| `Assets/Scripts/Core/GameLoadoutManager.cs` | **MỚI** | Quản lý tập trung dữ liệu 3 Ship, 4 Laser, 4 Độ Khó, lưu trữ `PlayerPrefs` và truy xuất Static. |
| `Assets/Scripts/Player/PlayerController.cs` | **NÂNG CẤP** | Di chuyển 4 hướng + chéo, xoay mượt 360 độ theo chuột, áp dụng chỉ số phi thuyền đã chọn. |
| `Assets/Scripts/Player/PlayerShooting.cs` | **NÂNG CẤP** | Hỗ trợ bắn Chuột Trái + Space liên tục, định hướng nòng súng 360°, nạp chỉ số đạn từ Loadout. |
| `Assets/Scripts/Player/Laser.cs` | **NÂNG CẤP** | Bay độc lập theo góc ngắm, tính sát thương (Damage), gắn TrailRenderer neon, tự hủy ngoài biên. |
| `Assets/Scripts/Player/PlayerCollision.cs` | **NÂNG CẤP** | Trừ điểm theo độ khó, hiệu ứng nổ, kích hoạt Game Over và dừng nhận input. |
| `Assets/Scripts/Enemies/Asteroid.cs` | **NÂNG CẤP** | 3 biến thể kích cỡ (Small, Medium, Large), tốc độ theo độ khó, nhận sát thương và cộng điểm 1 lần. |
| `Assets/Scripts/Collectibles/Star.cs` | **NÂNG CẤP** | Thêm cờ `isCollected` ngăn ăn điểm trùng lặp, âm thanh nhặt sao, hiệu ứng lấp lánh. |
| `Assets/Scripts/Core/ScoreManager.cs` | **NÂNG CẤP** | Quản lý điểm, kỷ lục, ghi nhận số sao và số thiên thạch tiêu diệt cho HUD và màn hình kết thúc. |
| `Assets/Scripts/Level/WorldSpawner.cs` | **NÂNG CẤP** | Áp dụng chu kỳ spawn độ khó, giới hạn mật độ tối đa để bảo vệ FPS, kiểm tra vùng an toàn người chơi. |
| `Assets/Scripts/Level/StarSpawner.cs` | **NÂNG CẤP** | Sinh sao thưởng theo nhịp độ thử thách và giới hạn số lượng trên màn hình. |
| `Assets/Scripts/UI/ScoreUI.cs` | **NÂNG CẤP** | HUD thời gian thực: Score, Huy hiệu Độ Khó có màu riêng, Sector, Star Counter, Kills Counter. |
| `Assets/Scripts/UI/GameOverUI.cs` | **NÂNG CẤP** | Bảng kết quả đầy đủ: Final Score, Personal Best, Star/Kills, nút Play Again, Main Menu, Quit. |
| `Assets/Scripts/UI/MainMenuUI.cs` | **NÂNG CẤP** | Menu chính tích hợp Launch, Armory & Loadout (Live Preview), Difficulty Selection, Flight Manual. |
| `Assets/Scripts/UI/EndGameUI.cs` | **MỚI** | Script điều khiển cho Scene `EndGame.unity` độc lập khi kết thúc chiến dịch. |

---

## 4. Bộ Asset 2D Tự Thiết Kế Hoàn Chỉnh

Toàn bộ asset đã được tạo trong `Assets/Art` và đồng bộ trong `Assets/Resources` kèm file `.meta` chuẩn Sprite 2D:
- **Phi Thuyền**: `Assets/Resources/Ships/Ship_Scout.png`, `Ship_Vanguard.png`, `Ship_Phantom.png` (512x512, transparent, silhouette sắc nét, động cơ phát quang).
- **Vũ Khí Laser**: `Assets/Resources/Lasers/Laser_Cyan.png`, `Laser_Crimson.png`, `Laser_Emerald.png`, `Laser_Gold.png` (256x64, tia năng lượng neon có tâm sáng rực).
- **Tiểu Hành Tinh**: `Assets/Resources/Asteroids/Asteroid_Rock.png`, `Asteroid_Crystal.png`, `Asteroid_Fire.png`, `Asteroid_Dark.png` (256x256, đổ bóng đa giác, miệng hố thiên thạch chi tiết).
- **Sao Thưởng**: `Assets/Resources/Collectibles/Star_Glowing.png` (256x256, sao vàng đa giác hào quang rực rỡ).

---

## 5. Danh Sách Scene Trong Build Settings

Cấu hình tại `ProjectSettings/EditorBuildSettings.asset`:
1. `Assets/Scenes/MainMenu.unity` (Index 0)
2. `Assets/Scenes/Level1_EarthOrbit.unity` (Index 1)
3. `Assets/Scenes/EndGame.unity` (Index 2)

---

## 6. Hướng Dẫn Kiểm Thử Từng Yêu Cầu Lab (Checklist 12 Test Cases)

1. **TEST 1 — Di chuyển bằng Arrow Keys / WASD**:
   - Nhấn các phím mũi tên hoặc WASD. Tàu di chuyển mượt mà lên, xuống, trái, phải và chéo. Tàu không bị lọt khỏi khung hình camera.
2. **TEST 2 — Xoay 360 độ ngắm theo chuột**:
   - Rê chuột vòng tròn quanh phi thuyền. Mũi tàu xoay mượt mà 360 độ hướng thẳng vào con trỏ chuột mà không bị giới hạn ở 4 góc 0/90/180/270 độ.
3. **TEST 3 — Bắn laser mọi hướng**:
   - Nhấp Chuột Trái hoặc phím Space ở các vị trí chuột khác nhau. Tia laser phóng ra từ đầu nòng súng bay thẳng về phía con trỏ chuột.
4. **TEST 4 — Đạn bay độc lập, không bị cuốn theo góc xoay**:
   - Bắn một tia laser, sau đó lập tức quay chuột sang hướng khác. Tia laser đã bắn vẫn tiếp tục bay thẳng theo quỹ đạo ban đầu, không bị bẻ cong theo hướng mới của tàu.
5. **TEST 5 — Bắn liên tục khi giữ nút (Continuous Fire)**:
   - Giữ chặt Chuột Trái hoặc phím Space. Đạn được bắn liên tục với nhịp độ đều đặn theo đúng `fireCooldown` của vũ khí.
6. **TEST 6 — Asteroid di chuyển và có thể bị phá hủy**:
   - Các thiên thạch xuất hiện từ mép phải với kích thước và tốc độ khác nhau. Bắn trúng thiên thạch sẽ phát tia lửa; khi hết HP, thiên thạch nổ tung và cộng điểm cho người chơi.
7. **TEST 7 — Sao thưởng (Star) ăn đúng một lần**:
   - Bay vào ngôi sao vàng. Âm thanh nhặt sao vang lên, điểm tăng đúng +100 điểm, ngôi sao biến mất và số lượng sao nhặt được (`★ Stars`) trên HUD tăng 1.
8. **TEST 8 — Va chạm Player - Asteroid kích hoạt Game Over**:
   - Để tàu va chạm với thiên thạch. Tàu nổ tung, điểm bị trừ theo mức phạt của độ khó hiện tại, lối chơi dừng lại và bảng Game Over hiển thị kết quả.
9. **TEST 9 — Main Menu và chuyển Scene**:
   - Mở Scene `MainMenu`. Nhấn `LAUNCH MISSION` để vào game mượt mà.
10. **TEST 10 — Tùy biến Ship & Laser Loadout**:
    - Vào Menu chọn `SHIP & WEAPON LOADOUT`. Thử chuyển đổi qua lại giữa 3 tàu và 4 loại laser. Xem hình ảnh Live Preview và chỉ số cập nhật trực tiếp. Nhấn Confirm rồi vào game kiểm tra: phi thuyền và loại đạn bắn ra đổi đúng theo lựa chọn.
11. **TEST 11 — Chọn Độ Khó (Difficulty Selection)**:
    - Vào Menu chọn `DIFFICULTY SELECTION`. Thử chọn `EASY` rồi vào game: thiên thạch thưa và bay chậm. Thử chọn `NIGHTMARE`: thiên thạch bay cực nhanh, dày đặc, HUD hiển thị nhãn `[NIGHTMARE]`.
12. **TEST 12 — Hiệu năng và không rò rỉ bộ nhớ / đạn vô hạn**:
    - Chơi game trong thời gian dài. Đạn và thiên thạch trôi ra ngoài màn hình được tự động dọn dẹp sạch sẽ, console không phát sinh lỗi compile hay missing script.

---

## 7. Cách Chạy Game Trong Unity Editor

1. Mở Unity Hub, mở dự án `SpaceExplorer` (phiên bản Unity 6000.5.9f1 hoặc tương đương).
2. Trong cửa sổ **Project**, mở Scene `Assets/Scenes/MainMenu.unity`.
3. Nhấn nút **Play (▶)** trên thanh công cụ Unity để bắt đầu trải nghiệm!
