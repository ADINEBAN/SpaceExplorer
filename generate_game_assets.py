import os
import math
from PIL import Image, ImageDraw, ImageFilter

def ensure_dir(path):
    if not os.path.exists(path):
        os.makedirs(path, exist_ok=True)

# -------------------------------------------------------------
# SHIP 1: ALPHA SCOUT (Cyan & White Interceptor)
# Sleek, triangular stealth fighter with dual thrusters and wing guns
# -------------------------------------------------------------
def create_ship_scout(size=(512, 512)):
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    w, h = size
    cx, cy = w / 2, h / 2

    # Engine Glow (Cyan / Electric Blue)
    glow_img = Image.new('RGBA', size, (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow_img)
    # Left engine plume
    glow_draw.ellipse([cx - 70, cy + 140, cx - 20, cy + 240], fill=(0, 229, 255, 180))
    glow_draw.ellipse([cx - 60, cy + 150, cx - 30, cy + 210], fill=(255, 255, 255, 240))
    # Right engine plume
    glow_draw.ellipse([cx + 20, cy + 140, cx + 70, cy + 240], fill=(0, 229, 255, 180))
    glow_draw.ellipse([cx + 30, cy + 150, cx + 60, cy + 210], fill=(255, 255, 255, 240))
    glow_img = glow_img.filter(ImageFilter.GaussianBlur(12))
    img.alpha_composite(glow_img)

    # Main Wings (Dark alloy titanium)
    wing_poly = [
        (cx, cy - 200),        # Nose
        (cx + 45, cy - 60),
        (cx + 190, cy + 130),   # Right wingtip
        (cx + 170, cy + 160),
        (cx + 80, cy + 140),
        (cx + 50, cy + 180),    # Right thruster mount
        (cx + 20, cy + 180),
        (cx, cy + 150),         # Center rear
        (cx - 20, cy + 180),
        (cx - 50, cy + 180),    # Left thruster mount
        (cx - 80, cy + 140),
        (cx - 170, cy + 160),
        (cx - 190, cy + 130),   # Left wingtip
        (cx - 45, cy - 60)
    ]
    draw.polygon(wing_poly, fill=(28, 36, 52, 255), outline=(0, 229, 255, 255), width=4)

    # Wing Armor Plates (Light Cyan-Silver)
    right_plate = [
        (cx + 20, cy - 80),
        (cx + 150, cy + 110),
        (cx + 90, cy + 130),
        (cx + 15, cy + 60)
    ]
    draw.polygon(right_plate, fill=(45, 65, 95, 255), outline=(0, 200, 230, 200), width=2)

    left_plate = [
        (cx - 20, cy - 80),
        (cx - 150, cy + 110),
        (cx - 90, cy + 130),
        (cx - 15, cy + 60)
    ]
    draw.polygon(left_plate, fill=(45, 65, 95, 255), outline=(0, 200, 230, 200), width=2)

    # Central Fuselage (Aerodynamic Arrow)
    fuse_poly = [
        (cx, cy - 210),        # Sharp Nose tip
        (cx + 32, cy - 50),
        (cx + 28, cy + 130),
        (cx, cy + 155),
        (cx - 28, cy + 130),
        (cx - 32, cy - 50)
    ]
    draw.polygon(fuse_poly, fill=(225, 240, 255, 255), outline=(0, 229, 255, 255), width=3)

    # Cockpit Glass (Glowing Neon Cyan with gradient-like specular)
    cockpit_poly = [
        (cx, cy - 130),
        (cx + 16, cy - 40),
        (cx + 12, cy + 30),
        (cx, cy + 45),
        (cx - 12, cy + 30),
        (cx - 16, cy - 40)
    ]
    draw.polygon(cockpit_poly, fill=(0, 229, 255, 255), outline=(255, 255, 255, 255), width=2)
    # Specular glint
    draw.line([(cx - 6, cy - 110), (cx + 8, cy - 20)], fill=(255, 255, 255, 220), width=3)

    # Wingtip Laser Cannons
    draw.rectangle([cx - 194, cy + 90, cx - 186, cy + 140], fill=(0, 229, 255, 255))
    draw.rectangle([cx + 186, cy + 90, cx + 194, cy + 140], fill=(0, 229, 255, 255))

    return img

# -------------------------------------------------------------
# SHIP 2: NEBULA VANGUARD (Heavy Armored Cruiser, Violet & Gold)
# -------------------------------------------------------------
def create_ship_vanguard(size=(512, 512)):
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    w, h = size
    cx, cy = w / 2, h / 2

    # Engine Glow (Violet / Magenta)
    glow_img = Image.new('RGBA', size, (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow_img)
    glow_draw.ellipse([cx - 85, cy + 130, cx - 25, cy + 240], fill=(186, 85, 211, 180))
    glow_draw.ellipse([cx - 70, cy + 140, cx - 40, cy + 210], fill=(255, 220, 255, 240))
    glow_draw.ellipse([cx + 25, cy + 130, cx + 85, cy + 240], fill=(186, 85, 211, 180))
    glow_draw.ellipse([cx + 40, cy + 140, cx + 70, cy + 210], fill=(255, 220, 255, 240))
    glow_img = glow_img.filter(ImageFilter.GaussianBlur(12))
    img.alpha_composite(glow_img)

    # Heavy Outrigger Wings
    outer_poly = [
        (cx, cy - 170),
        (cx + 70, cy - 70),
        (cx + 205, cy + 40),
        (cx + 215, cy + 120),
        (cx + 175, cy + 160),
        (cx + 120, cy + 125),
        (cx + 90, cy + 175),
        (cx + 35, cy + 175),
        (cx, cy + 145),
        (cx - 35, cy + 175),
        (cx - 90, cy + 175),
        (cx - 120, cy + 125),
        (cx - 175, cy + 160),
        (cx - 215, cy + 120),
        (cx - 205, cy + 40),
        (cx - 70, cy - 70)
    ]
    draw.polygon(outer_poly, fill=(35, 22, 55, 255), outline=(255, 200, 50, 255), width=4)

    # Armor Plating (Nebula Violet)
    right_armor = [
        (cx + 40, cy - 50),
        (cx + 170, cy + 50),
        (cx + 155, cy + 120),
        (cx + 60, cy + 90)
    ]
    draw.polygon(right_armor, fill=(88, 45, 135, 255), outline=(255, 215, 0, 180), width=3)

    left_armor = [
        (cx - 40, cy - 50),
        (cx - 170, cy + 50),
        (cx - 155, cy + 120),
        (cx - 60, cy + 90)
    ]
    draw.polygon(left_armor, fill=(88, 45, 135, 255), outline=(255, 215, 0, 180), width=3)

    # Center Hull (Heavy Battle Frame)
    center_poly = [
        (cx, cy - 205),
        (cx + 42, cy - 70),
        (cx + 38, cy + 130),
        (cx, cy + 160),
        (cx - 38, cy + 130),
        (cx - 42, cy - 70)
    ]
    draw.polygon(center_poly, fill=(215, 195, 240, 255), outline=(255, 215, 0, 255), width=4)

    # Bridge / Cockpit (Amber Gold Core)
    cockpit_poly = [
        (cx, cy - 120),
        (cx + 20, cy - 30),
        (cx + 15, cy + 40),
        (cx, cy + 60),
        (cx - 15, cy + 40),
        (cx - 20, cy - 30)
    ]
    draw.polygon(cockpit_poly, fill=(255, 185, 0, 255), outline=(255, 255, 220, 255), width=2)
    draw.line([(cx - 8, cy - 100), (cx + 8, cy - 15)], fill=(255, 255, 255, 230), width=3)

    return img

# -------------------------------------------------------------
# SHIP 3: VOID PHANTOM (Crimson & Obsidian Stealth Fighter)
# -------------------------------------------------------------
def create_ship_phantom(size=(512, 512)):
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    w, h = size
    cx, cy = w / 2, h / 2

    # Engine Glow (Blood Crimson / Hot Orange)
    glow_img = Image.new('RGBA', size, (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow_img)
    glow_draw.ellipse([cx - 50, cy + 130, cx + 50, cy + 245], fill=(255, 30, 60, 200))
    glow_draw.ellipse([cx - 25, cy + 140, cx + 25, cy + 220], fill=(255, 200, 100, 255))
    glow_img = glow_img.filter(ImageFilter.GaussianBlur(14))
    img.alpha_composite(glow_img)

    # Forward-Swept Wing Geometry
    wing_poly = [
        (cx, cy - 225),        # Needle Nose
        (cx + 35, cy - 110),
        (cx + 90, cy - 50),
        (cx + 200, cy - 30),    # Forward Swept tip right
        (cx + 180, cy + 90),
        (cx + 130, cy + 140),
        (cx + 60, cy + 160),
        (cx, cy + 175),
        (cx - 60, cy + 160),
        (cx - 130, cy + 140),
        (cx - 180, cy + 90),
        (cx - 200, cy - 30),    # Forward Swept tip left
        (cx - 90, cy - 50),
        (cx - 35, cy - 110)
    ]
    draw.polygon(wing_poly, fill=(20, 20, 26, 255), outline=(255, 35, 65, 255), width=4)

    # Stealth Facets (Crimson Carbon Fiber)
    right_facet = [
        (cx + 25, cy - 90),
        (cx + 175, cy - 10),
        (cx + 120, cy + 100),
        (cx + 35, cy + 50)
    ]
    draw.polygon(right_facet, fill=(90, 15, 25, 255), outline=(255, 60, 80, 200), width=2)

    left_facet = [
        (cx - 25, cy - 90),
        (cx - 175, cy - 10),
        (cx - 120, cy + 100),
        (cx - 35, cy + 50)
    ]
    draw.polygon(left_facet, fill=(90, 15, 25, 255), outline=(255, 60, 80, 200), width=2)

    # Spine & Cockpit
    spine_poly = [
        (cx, cy - 230),
        (cx + 22, cy - 70),
        (cx + 18, cy + 120),
        (cx, cy + 160),
        (cx - 18, cy + 120),
        (cx - 22, cy - 70)
    ]
    draw.polygon(spine_poly, fill=(45, 45, 55, 255), outline=(255, 30, 60, 255), width=3)

    # Evil Ruby Cockpit Slit
    cockpit_poly = [
        (cx, cy - 140),
        (cx + 12, cy - 50),
        (cx + 9, cy + 20),
        (cx, cy + 35),
        (cx - 9, cy + 20),
        (cx - 12, cy - 50)
    ]
    draw.polygon(cockpit_poly, fill=(255, 25, 50, 255), outline=(255, 220, 220, 255), width=2)
    draw.line([(cx - 4, cy - 120), (cx + 4, cy - 30)], fill=(255, 200, 200, 255), width=3)

    return img

# -------------------------------------------------------------
# 4 LASER WEAPONS (Drawn horizontally facing Right +X)
# -------------------------------------------------------------
def create_laser(color_core, color_glow, size=(256, 64)):
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    glow = Image.new('RGBA', size, (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow)
    w, h = size
    cy = h / 2

    # Outer Glow Pill
    glow_draw.rounded_rectangle([15, cy - 20, w - 15, cy + 20], radius=20, fill=color_glow)
    glow = glow.filter(ImageFilter.GaussianBlur(8))
    img.alpha_composite(glow)

    # Inner Intense Beam
    draw = ImageDraw.Draw(img)
    draw.rounded_rectangle([25, cy - 10, w - 20, cy + 10], radius=10, fill=color_glow)
    # Core White Hot Center
    draw.rounded_rectangle([35, cy - 4, w - 25, cy + 4], radius=4, fill=color_core)

    return img

# -------------------------------------------------------------
# 4 ASTEROID VARIANTS (Rock, Crystal, Fire, Dark Iron)
# -------------------------------------------------------------
def create_asteroid(base_color, shade_color, highlight_color, crater_color, seed=1, size=(256, 256)):
    import random
    random.seed(seed)
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = size[0] / 2, size[1] / 2
    r_base = size[0] * 0.38

    # Generate irregular jagged polygon
    num_pts = 16
    points = []
    for i in range(num_pts):
        angle = (2 * math.pi * i) / num_pts
        r = r_base * random.uniform(0.78, 1.18)
        px = cx + r * math.cos(angle)
        py = cy + r * math.sin(angle)
        points.append((px, py))

    # Base body
    draw.polygon(points, fill=base_color)

    # Shading on one side (Darker facet)
    shade_pts = [(cx, cy)]
    for i in range(num_pts // 2, num_pts + 1):
        idx = i % num_pts
        shade_pts.append(points[idx])
    draw.polygon(shade_pts, fill=shade_color)

    # Outer Outline
    draw.polygon(points, outline=highlight_color, width=3)

    # Craters / facets
    for _ in range(5):
        cr = random.uniform(12, 28)
        cpx = cx + random.uniform(-r_base * 0.55, r_base * 0.55)
        cpy = cy + random.uniform(-r_base * 0.55, r_base * 0.55)
        draw.ellipse([cpx - cr, cpy - cr, cpx + cr, cpy + cr], fill=crater_color, outline=highlight_color, width=2)
        # Inner crater shadow
        draw.arc([cpx - cr + 2, cpy - cr + 2, cpx + cr - 2, cpy + cr - 2], 45, 225, fill=shade_color, width=3)

    return img

# -------------------------------------------------------------
# GLOWING STAR & PICKUP
# -------------------------------------------------------------
def create_star(size=(256, 256)):
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    glow = Image.new('RGBA', size, (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow)
    w, h = size
    cx, cy = w / 2, h / 2

    # Aura Glow
    glow_draw.ellipse([cx - 80, cy - 80, cx + 80, cy + 80], fill=(255, 225, 50, 160))
    glow = glow.filter(ImageFilter.GaussianBlur(16))
    img.alpha_composite(glow)

    draw = ImageDraw.Draw(img)
    # 5-pointed star
    points = []
    r_outer = 75
    r_inner = 35
    for i in range(10):
        r = r_outer if i % 2 == 0 else r_inner
        ang = -math.pi / 2 + (2 * math.pi * i) / 10
        points.append((cx + r * math.cos(ang), cy + r * math.sin(ang)))

    draw.polygon(points, fill=(255, 220, 0, 255), outline=(255, 255, 255, 255), width=3)

    # Specular Inner Star
    inner_points = []
    for i in range(10):
        r = (r_outer if i % 2 == 0 else r_inner) * 0.55
        ang = -math.pi / 2 + (2 * math.pi * i) / 10
        inner_points.append((cx + r * math.cos(ang), cy + r * math.sin(ang)))
    draw.polygon(inner_points, fill=(255, 255, 220, 255))

    return img

def main():
    base_art = "Assets/Art"
    base_res = "Assets/Resources"

    dirs = [
        f"{base_art}/Player", f"{base_res}/Ships",
        f"{base_art}/Laser", f"{base_res}/Lasers",
        f"{base_art}/Asteroids", f"{base_res}/Asteroids",
        f"{base_art}/Collectibles", f"{base_res}/Collectibles",
        f"{base_art}/UI", f"{base_res}/UI"
    ]
    for d in dirs:
        ensure_dir(d)

    print("Generating Ships...")
    ship_scout = create_ship_scout()
    ship_scout.save(f"{base_art}/Player/Ship_Scout.png")
    ship_scout.save(f"{base_res}/Ships/Ship_Scout.png")

    ship_vanguard = create_ship_vanguard()
    ship_vanguard.save(f"{base_art}/Player/Ship_Vanguard.png")
    ship_vanguard.save(f"{base_res}/Ships/Ship_Vanguard.png")

    ship_phantom = create_ship_phantom()
    ship_phantom.save(f"{base_art}/Player/Ship_Phantom.png")
    ship_phantom.save(f"{base_res}/Ships/Ship_Phantom.png")

    print("Generating Lasers...")
    laser_cyan = create_laser((255, 255, 255, 255), (0, 229, 255, 220))
    laser_cyan.save(f"{base_art}/Laser/Laser_Cyan.png")
    laser_cyan.save(f"{base_res}/Lasers/Laser_Cyan.png")

    laser_crimson = create_laser((255, 240, 240, 255), (255, 23, 68, 230))
    laser_crimson.save(f"{base_art}/Laser/Laser_Crimson.png")
    laser_crimson.save(f"{base_res}/Lasers/Laser_Crimson.png")

    laser_emerald = create_laser((240, 255, 240, 255), (0, 230, 118, 230))
    laser_emerald.save(f"{base_art}/Laser/Laser_Emerald.png")
    laser_emerald.save(f"{base_res}/Lasers/Laser_Emerald.png")

    laser_gold = create_laser((255, 255, 240, 255), (255, 215, 0, 240))
    laser_gold.save(f"{base_art}/Laser/Laser_Gold.png")
    laser_gold.save(f"{base_res}/Lasers/Laser_Gold.png")

    print("Generating Asteroids...")
    ast_rock = create_asteroid((100, 105, 115, 255), (55, 60, 70, 255), (160, 170, 185, 255), (40, 45, 52, 255), seed=10)
    ast_rock.save(f"{base_art}/Asteroids/Asteroid_Rock.png")
    ast_rock.save(f"{base_res}/Asteroids/Asteroid_Rock.png")

    ast_crystal = create_asteroid((40, 120, 180, 255), (20, 65, 110, 255), (0, 240, 255, 255), (15, 45, 80, 255), seed=22)
    ast_crystal.save(f"{base_art}/Asteroids/Asteroid_Crystal.png")
    ast_crystal.save(f"{base_res}/Asteroids/Asteroid_Crystal.png")

    ast_fire = create_asteroid((170, 60, 30, 255), (95, 30, 15, 255), (255, 120, 20, 255), (60, 18, 10, 255), seed=33)
    ast_fire.save(f"{base_art}/Asteroids/Asteroid_Fire.png")
    ast_fire.save(f"{base_res}/Asteroids/Asteroid_Fire.png")

    ast_dark = create_asteroid((45, 45, 55, 255), (25, 25, 32, 255), (120, 120, 140, 255), (18, 18, 24, 255), seed=44)
    ast_dark.save(f"{base_art}/Asteroids/Asteroid_Dark.png")
    ast_dark.save(f"{base_res}/Asteroids/Asteroid_Dark.png")

    print("Generating Collectibles...")
    star = create_star()
    star.save(f"{base_art}/Collectibles/Star_Glowing.png")
    star.save(f"{base_res}/Collectibles/Star_Glowing.png")

    print("Assets successfully generated!")

if __name__ == '__main__':
    main()
