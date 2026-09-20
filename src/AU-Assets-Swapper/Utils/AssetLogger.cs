using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
// this is normal logger. i will rewrite later....
namespace AU_Assets_Swapper.Utils;

internal static class AssetLogger
{
    private static readonly List<string> logged = new();

    public static void LogAsset(string path, Type t)
    {
        if (!Plugin.DumpAllAssets.Value) return;

        var entry = $"[{t.Name}] {path}";
        if (!logged.Contains(entry))
        {
            logged.Add(entry);
            Plugin.LogSource.LogInfo($"[AUAS-DUMP] {entry}");
        }
    }

    public static void SaveDumpToFile()
    {
        if (logged.Count == 0) return;

        try
        {
            var p = Path.Combine(Plugin.SwapRootPath, "AssetDump.txt");
            File.WriteAllLines(p, logged);
            Plugin.LogSource.LogInfo($"[AUAS] dump saved: {p} ({logged.Count} entries)");
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError($"[AUAS] dump save failed: {ex.Message}");
        }
    }

    public static void Clear() => logged.Clear();
    public static int Count => logged.Count;
}
