/* AU Assets Swapper - Made by Isam Ahmed (isam-ahme0, isam8087, isam0) - 2026
 * GPL-3.0 - https://gamebanana.com/mods/719417
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace AU_Assets_Swapper;

internal static class PackManager
{
    public const string None = "None";
    private static readonly string[] cats = { "Sprites", "Textures", "Audio", "Fonts", "Shaders", "Materials", "Prefabs" };

    public static List<string> GetPacks()
    {
        var l = new List<string>();
        var r = Plugin.SwapRootPath;
        if (!Directory.Exists(r)) return l;
        foreach (var d in Directory.GetDirectories(r))
            l.Add(Path.GetFileName(d));
        l.Sort(StringComparer.OrdinalIgnoreCase);
        return l;
    }

    public static string Resolve(string want)
    {
        var r = Plugin.SwapRootPath;
        if (!string.IsNullOrEmpty(want) && !want.Equals(None, StringComparison.OrdinalIgnoreCase)
            && Directory.Exists(Path.Combine(r, want)))
            return want;
        if (Directory.Exists(Path.Combine(r, "legacy"))) return "legacy";
        return None;
    }

    // old flat layout -> AUAS_Data/legacy/ - thanks for readingg.
    public static void Migrate()
    {
        var r = Plugin.SwapRootPath;
        if (!Directory.Exists(r)) return;

        bool flat = false;
        foreach (var c in cats)
            if (Directory.Exists(Path.Combine(r, c))) { flat = true; break; }
        if (!flat) return;

        var leg = Path.Combine(r, "legacy");
        Directory.CreateDirectory(leg);

        foreach (var c in cats)
        {
            var src = Path.Combine(r, c);
            if (!Directory.Exists(src)) continue;
            var dst = Path.Combine(leg, c);
            if (Directory.Exists(dst))
            {
                Plugin.LogSource.LogWarning($"[AUAS] migrate skip {c} (legacy/{c} exists)");
                continue;
            }
            Directory.Move(src, dst);
            Plugin.LogSource.LogInfo($"[AUAS] migrated {c} -> legacy/{c}");
        }

        var rm = Path.Combine(r, "README.txt");
        var rmDst = Path.Combine(leg, "README.txt");
        if (File.Exists(rm) && !File.Exists(rmDst))
            File.Move(rm, rmDst);
    }

    public static void WriteRootReadme()
    {
        Directory.CreateDirectory(Plugin.SwapRootPath);
        var p = Path.Combine(Plugin.SwapRootPath, "README.txt");
        if (File.Exists(p)) return;
        File.WriteAllText(p,
            "AU-Assets-Swapper packs\n" +
            "=======================\n\n" +
            "Each subfolder of AUAS_Data is a pack. Category folders go inside a pack:\n\n" +
            "  AUAS_Data/MyPack/Sprites/\n" +
            "  AUAS_Data/MyPack/Audio/\n" +
            "  ...\n\n" +
            "Categories: Sprites, Textures, Audio, Fonts, Shaders, Materials, Prefabs\n" +
            "File names must match the asset name (without extension).\n\n" +
            "In-game: F6 = pack menu, F5 = rescan, F7 = pick mode.\n" +
            "Old flat assets were moved to AUAS_Data/legacy/.\n");
    }
}
