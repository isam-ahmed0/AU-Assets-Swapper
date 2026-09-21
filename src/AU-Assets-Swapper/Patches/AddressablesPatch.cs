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
 * https://github.com/isam-ahmed0/AU-Assets-Swapper
 */

using System;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using UnityEngine;

namespace AU_Assets_Swapper.Patches;

internal static class AddressablesPatch
{
    private static int _logged;

    public static void Patch(Harmony harmony)
    {
        try
        {
            var t = Type.GetType("UnityEngine.AddressableAssets.Addressables, Unity.Addressables");
            if (t == null)
            {
                Plugin.LogSource.LogWarning("[AUAS] Addressables type not found, skipping.");
                return; /* Star my repo plz. https://github.com/isam-ahmed0/AU-Assets-Swapper */
            }

            Plugin.LogSource.LogInfo("[AUAS] Found Addressables, patching LoadAsset...");

            int patched = 0;
            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (m.IsGenericMethod || m.ContainsGenericParameters) continue;

                try
                {
                    var p = m.GetParameters();
                    if (m.Name == "LoadAsset" && p.Length == 1 && p[0].ParameterType == typeof(string))
                    {
                        harmony.Patch(m, new HarmonyMethod(
                            AccessTools.Method(typeof(AddressablesPatch), nameof(Prefix))));
                        patched++;
                    }
                }
                catch { /* harmony already logs failures */ }
            }

            Plugin.LogSource.LogInfo($"[AUAS] Addressables: patched {patched} methods.");
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogWarning($"[AUAS] Addressables patch failed: {ex.Message}");
        }
    }

    internal static bool Prefix(string key, ref object __result)
    {
        if (Interlocked.CompareExchange(ref _logged, 1, 0) == 0)
            Plugin.LogSource.LogInfo("[AUAS] Addressables.LoadAsset(string) prefix CALLED");

        if (string.IsNullOrEmpty(key)) return true;
        var mgr = Plugin.SwapManager;
        if (mgr == null) return true;

        mgr.LogLoadedAsset($"[Addressable] {key}", typeof(object));

        var res = mgr.TryFindReplacement(key);
        if (res != null) { __result = res; return false; }

        return true;
    }
}
