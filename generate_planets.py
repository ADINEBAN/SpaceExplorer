import os
import math
import random
from PIL import Image, ImageDraw, ImageFilter

def ensure_dir(path):
    if not os.path.exists(path):
        os.makedirs(path, exist_ok=True)

# -------------------------------------------------------------
# 1. PLANET JUPITER (Gas Giant with Swirling Bands & Red Spot)
# -------------------------------------------------------------
def create_jupiter(size=(512, 512)):
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    w, h = size
    cx, cy = w / 2, h / 2
    radius = w * 0.42

    disc = Image.new('RGBA', size, (0, 0, 0, 0))
    bands_colors = [
        (195, 120, 75),  # Rust orange
        (225, 185, 140), # Cream beige
        (160, 95, 60),   # Deep brown
        (235, 205, 170), # Light tan
        (185, 110, 70),  # Amber
        (140, 80, 50),   # Dark ochre
        (215, 165, 125), # Warm peach
        (175, 105, 65)   # Brick
    ]

    for y in range(int(cy - radius), int(cy + radius) + 1):
        dy = y - cy
        dx_max = math.sqrt(max(0, radius**2 - dy**2))
        if dx_max <= 0:
            continue

        band_idx = int((y + math.sin(y * 0.08) * 12) / 24) % len(bands_colors)
        base_rgb = bands_colors[band_idx]
        
        for x in range(int(cx - dx_max), int(cx + dx_max) + 1):
            turb = math.sin(x * 0.05 + y * 0.02) * 15
            r = min(255, max(0, int(base_rgb[0] + turb)))
            g = min(255, max(0, int(base_rgb[1] + turb * 0.8)))
            b = min(255, max(0, int(base_rgb[2] + turb * 0.5)))
            disc.putpixel((x, y), (r, g, b, 255))

    spot_draw = ImageDraw.Draw(disc)
    spot_draw.ellipse([cx + 40, cy + 50, cx + 110, cy + 95], fill=(185, 45, 30, 240), outline=(130, 30, 20, 200), width=3)
    spot_draw.ellipse([cx + 55, cy + 62, cx + 95, cy + 83], fill=(215, 75, 50, 255))

    shade = Image.new('RGBA', size, (0, 0, 0, 0))
    for y in range(int(cy - radius), int(cy + radius) + 1):
        dy = y - cy
        dx_max = math.sqrt(max(0, radius**2 - dy**2))
        if dx_max <= 0:
            continue
        for x in range(int(cx - dx_max), int(cx + dx_max) + 1):
            nx = (x - cx) / radius
            ny = (y - cy) / radius
            nz = math.sqrt(max(0, 1.0 - nx**2 - ny**2))
            dot = nx * (-0.65) + ny * (-0.45) + nz * 0.61
            if dot < 0.1:
                shadow_alpha = int(min(240, (0.1 - dot) * 220))
                shade.putpixel((x, y), (5, 5, 12, shadow_alpha))

    glow = Image.new('RGBA', size, (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow)
    glow_draw.ellipse([cx - radius - 6, cy - radius - 6, cx + radius + 6, cy + radius + 6], outline=(255, 180, 100, 120), width=8)
    glow = glow.filter(ImageFilter.GaussianBlur(6))

    img.alpha_composite(disc)
    img.alpha_composite(shade)
    img.alpha_composite(glow)
    return img

# -------------------------------------------------------------
# 2. PLANET MARS (Sao Hỏa - Rocky Rust Planet)
# -------------------------------------------------------------
def create_mars(size=(384, 384)):
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    w, h = size
    cx, cy = w / 2, h / 2
    radius = w * 0.42

    disc = Image.new('RGBA', size, (0, 0, 0, 0))
    disc_draw = ImageDraw.Draw(disc)
    disc_draw.ellipse([cx - radius, cy - radius, cx + radius, cy + radius], fill=(195, 75, 40, 255))

    random.seed(42)
    for _ in range(18):
        cr = random.uniform(8, 28)
        ang = random.uniform(0, math.pi * 2)
        dist = random.uniform(0, radius * 0.8)
        px = cx + math.cos(ang) * dist
        py = cy + math.sin(ang) * dist
        color = (130, 45, 25, 230) if random.random() < 0.6 else (225, 110, 60, 200)
        disc_draw.ellipse([px - cr, py - cr, px + cr, py + cr], fill=color)

    shade = Image.new('RGBA', size, (0, 0, 0, 0))
    for y in range(int(cy - radius), int(cy + radius) + 1):
        dy = y - cy
        dx_max = math.sqrt(max(0, radius**2 - dy**2))
        if dx_max <= 0:
            continue
        for x in range(int(cx - dx_max), int(cx + dx_max) + 1):
            nx = (x - cx) / radius
            ny = (y - cy) / radius
            nz = math.sqrt(max(0, 1.0 - nx**2 - ny**2))
            dot = nx * (-0.7) + ny * (-0.4) + nz * 0.59
            if dot < 0.15:
                shadow_alpha = int(min(245, (0.15 - dot) * 230))
                shade.putpixel((x, y), (10, 4, 6, shadow_alpha))

    glow = Image.new('RGBA', size, (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow)
    glow_draw.ellipse([cx - radius - 4, cy - radius - 4, cx + radius + 4, cy + radius + 4], outline=(255, 100, 50, 100), width=6)
    glow = glow.filter(ImageFilter.GaussianBlur(5))

    img.alpha_composite(disc)
    img.alpha_composite(shade)
    img.alpha_composite(glow)
    return img

# -------------------------------------------------------------
# 3. PLANET MERCURY (Sao Thủy - Silver Moon / Terrestrial Cratered)
# -------------------------------------------------------------
def create_mercury(size=(320, 320)):
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    w, h = size
    cx, cy = w / 2, h / 2
    radius = w * 0.42

    disc = Image.new('RGBA', size, (0, 0, 0, 0))
    disc_draw = ImageDraw.Draw(disc)
    disc_draw.ellipse([cx - radius, cy - radius, cx + radius, cy + radius], fill=(160, 165, 175, 255))

    random.seed(101)
    for _ in range(22):
        cr = random.uniform(6, 22)
        ang = random.uniform(0, math.pi * 2)
        dist = random.uniform(0, radius * 0.85)
        px = cx + math.cos(ang) * dist
        py = cy + math.sin(ang) * dist
        color = (100, 105, 115, 220) if random.random() < 0.7 else (205, 210, 220, 200)
        disc_draw.ellipse([px - cr, py - cr, px + cr, py + cr], fill=color)

    shade = Image.new('RGBA', size, (0, 0, 0, 0))
    for y in range(int(cy - radius), int(cy + radius) + 1):
        dy = y - cy
        dx_max = math.sqrt(max(0, radius**2 - dy**2))
        if dx_max <= 0:
            continue
        for x in range(int(cx - dx_max), int(cx + dx_max) + 1):
            nx = (x - cx) / radius
            ny = (y - cy) / radius
            nz = math.sqrt(max(0, 1.0 - nx**2 - ny**2))
            dot = nx * (-0.7) + ny * (-0.4) + nz * 0.59
            if dot < 0.15:
                shadow_alpha = int(min(245, (0.15 - dot) * 230))
                shade.putpixel((x, y), (8, 10, 14, shadow_alpha))

    glow = Image.new('RGBA', size, (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow)
    glow_draw.ellipse([cx - radius - 3, cy - radius - 3, cx + radius + 3, cy + radius + 3], outline=(200, 220, 255, 90), width=5)
    glow = glow.filter(ImageFilter.GaussianBlur(4))

    img.alpha_composite(disc)
    img.alpha_composite(shade)
    img.alpha_composite(glow)
    return img

# -------------------------------------------------------------
# 4. PLANET ICE GIANT / NEPTUNE (Sao Hải Vương - Cyan with Glowing Rings)
# -------------------------------------------------------------
def create_ice_giant(size=(512, 512)):
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    w, h = size
    cx, cy = w / 2, h / 2
    radius = w * 0.32

    # Back half of planetary rings
    rings_back = Image.new('RGBA', size, (0, 0, 0, 0))
    rings_draw = ImageDraw.Draw(rings_back)
    for r_offset in range(0, 45, 3):
        rx = radius * 2.1 + r_offset
        ry = radius * 0.52 + r_offset * 0.25
        alpha = int(140 - r_offset * 2)
        rings_draw.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], outline=(0, 220, 255, alpha), width=2)
    for y in range(int(cy), h):
        for x in range(w):
            rings_back.putpixel((x, y), (0, 0, 0, 0))
    img.alpha_composite(rings_back)

    # Planet Body
    disc = Image.new('RGBA', size, (0, 0, 0, 0))
    disc_draw = ImageDraw.Draw(disc)
    disc_draw.ellipse([cx - radius, cy - radius, cx + radius, cy + radius], fill=(20, 110, 190, 255))

    for i in range(5):
        by = cy - radius * 0.6 + i * (radius * 0.3)
        b_w = math.sqrt(max(0, radius**2 - (by - cy)**2))
        if b_w > 10:
            disc_draw.line([(cx - b_w + 5, by), (cx + b_w - 5, by)], fill=(40, 185, 245, 180), width=6)

    shade = Image.new('RGBA', size, (0, 0, 0, 0))
    for y in range(int(cy - radius), int(cy + radius) + 1):
        dy = y - cy
        dx_max = math.sqrt(max(0, radius**2 - dy**2))
        if dx_max <= 0:
            continue
        for x in range(int(cx - dx_max), int(cx + dx_max) + 1):
            nx = (x - cx) / radius
            ny = (y - cy) / radius
            nz = math.sqrt(max(0, 1.0 - nx**2 - ny**2))
            dot = nx * (-0.65) + ny * (-0.45) + nz * 0.61
            if dot < 0.12:
                shadow_alpha = int(min(245, (0.12 - dot) * 230))
                shade.putpixel((x, y), (3, 8, 20, shadow_alpha))

    disc.alpha_composite(shade)
    img.alpha_composite(disc)

    # Front half of planetary rings
    rings_front = Image.new('RGBA', size, (0, 0, 0, 0))
    r_front_draw = ImageDraw.Draw(rings_front)
    for r_offset in range(0, 45, 3):
        rx = radius * 2.1 + r_offset
        ry = radius * 0.52 + r_offset * 0.25
        alpha = int(170 - r_offset * 2.5)
        r_front_draw.ellipse([cx - rx, cy - ry, cx + rx, cy + ry], outline=(100, 240, 255, alpha), width=2)
    for y in range(0, int(cy)):
        for x in range(w):
            rings_front.putpixel((x, y), (0, 0, 0, 0))
    img.alpha_composite(rings_front)

    glow = Image.new('RGBA', size, (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow)
    glow_draw.ellipse([cx - radius - 5, cy - radius - 5, cx + radius + 5, cy + radius + 5], outline=(0, 229, 255, 120), width=6)
    glow = glow.filter(ImageFilter.GaussianBlur(6))
    img.alpha_composite(glow)
    return img

def main():
    art_path = "Assets/Art/Background"
    res_path = "Assets/Resources/Planets"
    ensure_dir(art_path)
    ensure_dir(res_path)

    print("Generating Planet Jupiter...")
    jupiter = create_jupiter()
    jupiter.save(f"{art_path}/Planet_Jupiter.png")
    jupiter.save(f"{res_path}/Planet_Jupiter.png")

    print("Generating Planet Mars...")
    mars = create_mars()
    mars.save(f"{art_path}/Planet_Mars.png")
    mars.save(f"{res_path}/Planet_Mars.png")

    print("Generating Planet Mercury...")
    mercury = create_mercury()
    mercury.save(f"{art_path}/Planet_Mercury.png")
    mercury.save(f"{res_path}/Planet_Mercury.png")

    print("Generating Planet Ice Giant...")
    ice_giant = create_ice_giant()
    ice_giant.save(f"{art_path}/Planet_IceGiant.png")
    ice_giant.save(f"{res_path}/Planet_IceGiant.png")

    print("All celestial planets generated successfully!")

if __name__ == '__main__':
    main()
