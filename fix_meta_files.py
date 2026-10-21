import os
import uuid

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

SCRIPT_META_TEMPLATE = """fileFormatVersion: 2
guid: {guid}
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

# Specific GUIDs matching existing scenes
KNOWN_SCRIPT_GUIDS = {
    "MainMenuUI.cs": "a01d6f6b9db74a648fd1c022904671aa",
    "GameOverUI.cs": "d261cdacd61288243a24e4a392a4e144",
    "EndGameUI.cs": "5dc7416ab13b48d28fc79c8f9782e0d2",
    "SpaceParallaxBackground.cs": "afbcf349e57447ccaa3dd803b07dccc6"
}

def fix_all_meta(root_dir="Assets"):
    fixed_count = 0
    for dirpath, dirnames, filenames in os.walk(root_dir):
        # Folder meta
        folder_meta = dirpath + ".meta"
        if dirpath != "Assets":
            if not os.path.exists(folder_meta) or os.path.getsize(folder_meta) == 0:
                with open(folder_meta, "w", encoding="utf-8") as f:
                    f.write(FOLDER_META_TEMPLATE.replace("{guid}", generate_guid()))
                fixed_count += 1
                print(f"Fixed folder meta: {folder_meta}")

        for fname in filenames:
            if fname.endswith(".meta"):
                continue
            fpath = os.path.join(dirpath, fname)
            meta_path = fpath + ".meta"
            needs_fix = not os.path.exists(meta_path) or os.path.getsize(meta_path) == 0

            if needs_fix:
                if fname.lower().endswith(('.png', '.jpg', '.jpeg')):
                    with open(meta_path, "w", encoding="utf-8") as f:
                        f.write(SPRITE_META_TEMPLATE.replace("{guid}", generate_guid()))
                    fixed_count += 1
                    print(f"Fixed sprite meta: {meta_path}")
                elif fname.lower().endswith('.cs'):
                    guid = KNOWN_SCRIPT_GUIDS.get(fname, generate_guid())
                    with open(meta_path, "w", encoding="utf-8") as f:
                        f.write(SCRIPT_META_TEMPLATE.replace("{guid}", guid))
                    fixed_count += 1
                    print(f"Fixed script meta: {meta_path} (guid={guid})")

    print(f"Total fixed: {fixed_count}")

if __name__ == '__main__':
    fix_all_meta()
