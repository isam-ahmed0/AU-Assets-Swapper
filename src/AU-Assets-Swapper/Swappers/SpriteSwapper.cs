using UnityEngine;

namespace AU_Assets_Swapper.Swappers;

internal static class SpriteSwapper
{
    public static Sprite CreateFromTexture(Texture2D tex, string name, float ppu = 100f)
    {
        if (tex == null) return null;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), ppu);
    }

    // Fuck: It actually swaps. 👌
    public static Sprite CreateSliceSprite(Texture2D tex, string name, Vector4 border, float ppu = 100f)
    {
        if (tex == null) return null;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), ppu);
    }
}
