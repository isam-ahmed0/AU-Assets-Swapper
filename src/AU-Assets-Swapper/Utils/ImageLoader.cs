using System;
using System.IO;
using UnityEngine;
// loadddddddddddddddd.......
namespace AU_Assets_Swapper;

internal static class ImageLoader
{
    public static Texture2D LoadTexture2D(string path)
    {
        if (!File.Exists(path)) return null;

        var bytes = File.ReadAllBytes(path);
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        try
        {
            if (tex.LoadImage(bytes))
                return tex;

            UnityEngine.Object.Destroy(tex);
            return null;
        }
        catch
        {
            UnityEngine.Object.Destroy(tex);
            return null;
        }
    }
}
