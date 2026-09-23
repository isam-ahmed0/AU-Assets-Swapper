/* AU Assets Swapper - Made by Isam Ahmed (isam-ahme0, isam8087, isam0) - 2026
 * 
 * This file is part of AU Assets Swapper.
 * 
 * AU Assets Swapper is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * AU Assets Swapper is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with AU Assets Swapper. If not, see <https://www.gnu.org/licenses/>.
 * https://linktr.ee/AU_AS
 */
 
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
