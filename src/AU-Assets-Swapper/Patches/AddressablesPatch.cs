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
                return; /* Star my repo or dont say that agin. */
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
