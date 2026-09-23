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
 * https://gamebanana.com/mods/719417
 */
using System;
using System.Reflection;
using System.Threading;
using HarmonyLib;
// its using UnityEngine. btw star this repo.
// unneccessary comment, make sure to star this repo. iy helpsssss meee.
using UnityEngine;

namespace AU_Assets_Swapper.Patches;

internal static class AssetBundlePatch
{
    private static int _syncLogged;
    private static int _asyncLogged;
    private static readonly HarmonyMethod _syncPrefix = new(AccessTools.Method(typeof(AssetBundlePatch), nameof(SyncPrefix)));
    private static readonly HarmonyMethod _asyncPrefix = new(AccessTools.Method(typeof(AssetBundlePatch), nameof(AsyncPrefix)));

    public static void Patch(Harmony harmony)
    {
        foreach (var m in typeof(AssetBundle).GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            if (m.IsGenericMethod || m.ContainsGenericParameters) continue;

            try
            {
                var p = m.GetParameters();
                if (m.Name == "LoadAsset" && p.Length == 1 && p[0].ParameterType == typeof(string))
                    harmony.Patch(m, _syncPrefix);
                else if (m.Name == "LoadAssetAsync" && p.Length == 1 && p[0].ParameterType == typeof(string))
                    harmony.Patch(m, _asyncPrefix);
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogWarning($"[AUAS] Failed to patch AssetBundle.{m.Name}: {ex.Message}");
            }
        }
    }

    internal static bool SyncPrefix(AssetBundle __instance, string name, ref UnityEngine.Object __result)
    {
        if (Interlocked.CompareExchange(ref _syncLogged, 1, 0) == 0)
            Plugin.LogSource.LogInfo("[AUAS] AssetBundle.LoadAsset(string) prefix CALLED");

        if (string.IsNullOrEmpty(name)) return true;
        var mgr = Plugin.SwapManager;
        if (mgr == null) return true;

        mgr.LogLoadedAsset($"[Bundle] {name}", typeof(UnityEngine.Object));

        var res = mgr.TryFindReplacement(name);
        if (res != null) { __result = res; return false; }

        return true;
    }

    internal static bool AsyncPrefix(AssetBundle __instance, string name, ref AssetBundleRequest __result)
    {
        if (Interlocked.CompareExchange(ref _asyncLogged, 1, 0) == 0)
            Plugin.LogSource.LogInfo("[AUAS] AssetBundle.LoadAssetAsync(string) prefix CALLED");

        if (string.IsNullOrEmpty(name)) return true;
        var mgr = Plugin.SwapManager;
        if (mgr == null) return true;

        mgr.LogLoadedAsset($"[Bundle/Async] {name}", typeof(UnityEngine.Object));

        // can't intercept async on IL2CPP - can't construct a dummy AssetBundleRequest
        return true;
    }
}
