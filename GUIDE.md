# Assets Pack making guide

Use the **F7** pick mode and hover over game assets to learn the type and asset name.

Use [AssetStudioMod](https://github.com/aelurum/AssetStudioMod) to view or export vanilla textures and sizes.

## Packs

Every direct subfolder of `AUAS_Data` is a pack. Put category folders inside the pack:

```text
AUAS_Data/MyPack/
├── Sprites/    - PNG/JPG images replace Sprite or Texture2D assets
├── Textures/   - PNG/JPG images replace raw Texture2D assets
├── Audio/      - WAV/OGG files replace AudioClip assets
├── Fonts/      - TTF/OTF files replace Font assets
├── Shaders/    - AssetBundle files (.ab) replace Shader assets
├── Materials/  - AssetBundle files (.ab) replace Material assets
└── Prefabs/    - AssetBundle files (.ab) replace Prefab/GameObject assets
```

File names should match the asset name (without extension).

## In-game keys

- **F5** - hot-reload replacements
- **F6** - pack menu (switch packs, or pick `None` for vanilla)
- **F7** - pick mode, hover assets to inspect

Old flat assets (category folders directly under `AUAS_Data`) are moved to `AUAS_Data/legacy/` automatically on first launch.
