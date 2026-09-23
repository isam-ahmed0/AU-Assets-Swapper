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
using System.Reflection;
using System.Threading;
using HarmonyLib;
using UnityEngine;

namespace AU_Assets_Swapper.Patches;

internal static class ResourcesLoadPatch
{
    private static int _strLogged;
    private static int _typedLogged;
    private static int _allLogged;
    private static readonly HarmonyMethod _loadPrefix = new(AccessTools.Method(typeof(ResourcesLoadPatch), nameof(LoadPrefix)));
    private static readonly HarmonyMethod _typedPrefix = new(AccessTools.Method(typeof(ResourcesLoadPatch), nameof(TypedPrefix)));
    private static readonly HarmonyMethod _allPrefix = new(AccessTools.Method(typeof(ResourcesLoadPatch), nameof(AllPrefix)));
/* https://linktr.ee/AU_AS
Please star my repo plz */
    public static void Patch(Harmony harmony)
    {
        foreach (var m in typeof(Resources).GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            if (m.IsGenericMethod || m.ContainsGenericParameters) continue;

            try
            {
                var p = m.GetParameters();
                if (m.Name == "Load" && p.Length == 1 && p[0].ParameterType == typeof(string))
                    harmony.Patch(m, _loadPrefix);
                else if (m.Name == "Load" && p.Length == 2 && p[0].ParameterType == typeof(string) && p[1].ParameterType == typeof(Type))
                    harmony.Patch(m, _typedPrefix);
                else if (m.Name == "LoadAll" && p.Length == 1 && p[0].ParameterType == typeof(string))
                    harmony.Patch(m, _allPrefix);
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogWarning($"[AUAS] Failed to patch Resources.{m.Name}: {ex.Message}");
            }
        }
    }

    internal static bool LoadPrefix(string path, ref UnityEngine.Object __result)
    {
        if (Interlocked.CompareExchange(ref _strLogged, 1, 0) == 0)
            Plugin.LogSource.LogInfo("[AUAS] Resources.Load(string) prefix CALLED");

        if (string.IsNullOrEmpty(path)) return true;
        var mgr = Plugin.SwapManager;
        if (mgr == null) return true;

        var name = ExtractName(path);
        mgr.LogLoadedAsset(path, typeof(UnityEngine.Object));

        var res = mgr.TryFindReplacement(name);
        if (res != null) { __result = res; return false; }

        return true;
    }
// Gahh. It works.
    internal static bool TypedPrefix(string path, Type t, ref UnityEngine.Object __result)
    {
        if (Interlocked.CompareExchange(ref _typedLogged, 1, 0) == 0)
            Plugin.LogSource.LogInfo("[AUAS] Resources.Load(string, Type) prefix CALLED");

        if (string.IsNullOrEmpty(path)) return true;
        var mgr = Plugin.SwapManager;
        if (mgr == null) return true;

        var name = ExtractName(path);
        mgr.LogLoadedAsset(path, t ?? typeof(UnityEngine.Object));

        var res = mgr.TryFindReplacement(name, t);
        if (res != null) { __result = res; return false; }

        return true;
    }

    internal static bool AllPrefix(string path, ref UnityEngine.Object[] __result)
    {
        if (Interlocked.CompareExchange(ref _allLogged, 1, 0) == 0)
            Plugin.LogSource.LogInfo("[AUAS] Resources.LoadAll(string) prefix CALLED");

        if (string.IsNullOrEmpty(path)) return true;
        var mgr = Plugin.SwapManager;
        if (mgr == null) return true;

        mgr.LogLoadedAsset(path + " (LoadAll)", typeof(UnityEngine.Object));
        return true;
    }

    // grab filename from a resource path like "Sprites/mySprite"
    private static string ExtractName(string path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;
        var i = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
        return i >= 0 ? path.Substring(i + 1) : path;
    }
}
