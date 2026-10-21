import os
import shutil
import uuid
import math
from PIL import Image, ImageDraw, ImageFilter

def generate_guid():
    return uuid.uuid4().hex

SPRITE_META_TEMPLATE = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 1
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: 100
  spriteBorder: {x: 0, y: 0, z: 0, w: 0}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings: []
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: 
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    nameFileIdTable: []
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

FOLDER_META_TEMPLATE = """fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

def write_meta_if_needed(meta_path):
    if not os.path.exists(meta_path) or os.path.getsize(meta_path) == 0:
        with open(meta_path, 'w', encoding='utf-8') as f:
            f.write(SPRITE_META_TEMPLATE.replace("{guid}", generate_guid()))
        print(f"Created meta: {meta_path}")

def ensure_folder_meta(folder_path):
    os.makedirs(folder_path, exist_ok=True)
    meta_path = folder_path.rstrip('/\\') + ".meta"
    if not os.path.exists(meta_path) or os.path.getsize(meta_path) == 0:
        with open(meta_path, 'w', encoding='utf-8') as f:
            f.write(FOLDER_META_TEMPLATE.replace("{guid}", generate_guid()))
        print(f"Created folder meta: {meta_path}")

def process_ships():
    src_dir = "Assets/Art/Player"
    dst_dir = "Assets/Resources/Ships"
    ensure_folder_meta(dst_dir)

    # All user ship files
    user_ships = ["SpaceShip.png", "SpaceShip1.png", "SpaceShip2.png", "SpaceShip3.png", "SpaceShip4.png"]

    for ship_name in user_ships:
        src = os.path.join(src_dir, ship_name)
        if not os.path.exists(src):
            print(f"Warning: {src} not found")
            continue

        # Ensure src meta exists
        write_meta_if_needed(src + ".meta")

        # Copy to Resources/Ships
        dst = os.path.join(dst_dir, ship_name)
        shutil.copy2(src, dst)
        write_meta_if_needed(dst + ".meta")
        print(f"Copied ship to Resources: {ship_name}")

def process_lasers():
    src_dir = "Assets/Art/Laser"
    dst_dir = "Assets/Resources/Lasers"
    ensure_folder_meta(dst_dir)

    user_lasers = [
        "1.png", "2.png", "3.png", "4.png", "5.png", "6.png",
        "SPR_Laser_CrimsonTwin_02.png", "SPR_Laser_Overcharge_07.png", "SPR_Laser_VoidWave_04.png"
    ]

    for laser_name in user_lasers:
        src = os.path.join(src_dir, laser_name)
        if not os.path.exists(src):
            print(f"Warning: {src} not found")
            continue

        write_meta_if_needed(src + ".meta")

        dst = os.path.join(dst_dir, laser_name)

        # If vertical laser, rotate -90 deg (clockwise) so it flies right (+X) uniformly
        if laser_name in ["SPR_Laser_CrimsonTwin_02.png", "SPR_Laser_Overcharge_07.png", "SPR_Laser_VoidWave_04.png"]:
            im = Image.open(src).convert('RGBA')
            rotated = im.rotate(-90, expand=True)
            rotated.save(dst)
        else:
            shutil.copy2(src, dst)

        write_meta_if_needed(dst + ".meta")
        print(f"Processed laser to Resources: {laser_name}")

def generate_vfx_textures():
    vfx_dir = "Assets/Resources/VFX"
    ensure_folder_meta(vfx_dir)

    # 1. Soft Flame Puff (Radial Gaussian Falloff for fireball clouds & muzzle glow)
    size = (128, 128)
    img_puff = Image.new('RGBA', size, (0, 0, 0, 0))
    cx, cy = 64, 64
    for y in range(128):
        for x in range(128):
            d = math.hypot(x - cx, y - cy) / 64.0
            if d < 1.0:
                # Smooth bell curve
                alpha = int(255 * math.pow(1.0 - d, 2.0))
                img_puff.putpixel((x, y), (255, 255, 255, alpha))
    puff_path = os.path.join(vfx_dir, "Vfx_FlamePuff.png")
    img_puff.save(puff_path)
    write_meta_if_needed(puff_path + ".meta")
    print("Generated Vfx_FlamePuff.png")

    # 2. Fiery Radiant Spark (4-pointed sharp diamond star with glow)
    img_spark = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img_spark)
    # Horizontal flare
    draw.polygon([(64, 52), (124, 64), (64, 76), (4, 64)], fill=(255, 255, 255, 220))
    # Vertical flare
    draw.polygon([(52, 64), (64, 124), (76, 64), (64, 4)], fill=(255, 255, 255, 220))
    # Center bright core
    draw.ellipse([(54, 54), (74, 74)], fill=(255, 255, 255, 255))
    img_spark = img_spark.filter(ImageFilter.GaussianBlur(1.5))
    spark_path = os.path.join(vfx_dir, "Vfx_FierySpark.png")
    img_spark.save(spark_path)
    write_meta_if_needed(spark_path + ".meta")
    print("Generated Vfx_FierySpark.png")

if __name__ == '__main__':
    process_ships()
    process_lasers()
    generate_vfx_textures()
    print("Assets processing and VFX generation completed successfully!")
