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
