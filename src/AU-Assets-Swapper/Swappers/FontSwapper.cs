// Not tested yet. but i think works. if it works, it works.
using System.IO;
using UnityEngine;

namespace AU_Assets_Swapper.Swappers;

internal static class FontSwapper
{
    public static Font LoadFromTtf(string path, string name)
    {
        if (!File.Exists(path)) return null;
        try
        {
            var f = new Font(path);
            f.name = name;
            return f;
        }
        catch (System.Exception ex)
        {
            Plugin.LogSource.LogError($"[AUAS] failed to load TTF '{name}': {ex.Message}");
            return null;
        }
    }
}
