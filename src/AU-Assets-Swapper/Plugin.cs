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
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using AU_Assets_Swapper.Patches;

namespace AU_Assets_Swapper;

[BepInPlugin("com.auassetsswapper.plugin", "AU Assets Swapper", "1.0.0")]
[BepInProcess("Among Us.exe")]
public class Plugin : BasePlugin
{
    internal static Plugin Instance { get; private set; }
    internal static ManualLogSource LogSource { get; private set; }
    internal static string PluginPath { get; private set; }
    internal static string SwapRootPath { get; private set; }
    internal static string ActivePackPath { get; private set; }
    internal static Harmony HarmonyInstance { get; private set; }
    internal static AssetSwapManager SwapManager { get; private set; }

    internal static ConfigEntry<bool> DumpAllAssets { get; private set; }
    internal static ConfigEntry<string> ActivePack { get; private set; }
    internal static ConfigEntry<bool> EnableSpriteSwap { get; private set; }
    internal static ConfigEntry<bool> EnableTextureSwap { get; private set; }
    internal static ConfigEntry<bool> EnableAudioSwap { get; private set; }
    internal static ConfigEntry<bool> EnableFontSwap { get; private set; }
    internal static ConfigEntry<bool> EnableShaderSwap { get; private set; }
    internal static ConfigEntry<bool> EnableMaterialSwap { get; private set; }
    internal static ConfigEntry<bool> EnablePrefabSwap { get; private set; }

    public override void Load()
    {
        Instance = this;
        LogSource = Log;

        PluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        // go up two levels from the plugin dll to reach game root, then into AUAS_Data
        var parent = Directory.GetParent(PluginPath);
        var grandparent = parent?.Parent;
        SwapRootPath = grandparent != null
            ? Path.Combine(grandparent.FullName, "AUAS_Data")
            : Path.Combine(PluginPath, "AUAS_Data");

        InitConfig();
        PackManager.Migrate();
        PackManager.WriteRootReadme();
        ActivePack.Value = PackManager.Resolve(ActivePack.Value);
        ActivePackPath = ActivePack.Value == PackManager.None
            ? null
            : Path.Combine(SwapRootPath, ActivePack.Value);

        LogSource.LogInfo($"[AUAS] plugin path: {PluginPath}");
        LogSource.LogInfo($"[AUAS] swap root: {SwapRootPath}");
        LogSource.LogInfo($"[AUAS] pack: {ActivePack.Value}");

        SwapManager = new AssetSwapManager(ActivePackPath);
        SwapManager.ScanAndLoadAssets();

        HarmonyInstance = new Harmony("com.auassetsswapper.plugin");

        // patch each thing, swallow errors so one failure doesn't kill the whole plugin
        try { ResourcesLoadPatch.Patch(HarmonyInstance); }
        catch (Exception ex) { LogSource.LogWarning($"[AUAS] Resources patch error: {ex.Message}"); }

        try { AssetBundlePatch.Patch(HarmonyInstance); }
        catch (Exception ex) { LogSource.LogWarning($"[AUAS] AssetBundle patch error: {ex.Message}"); }

        try { AddressablesPatch.Patch(HarmonyInstance); }
        catch (Exception ex) { LogSource.LogWarning($"[AUAS] Addressables patch error: {ex.Message}"); }

        AddComponent<SwapManagerComponent>();
        LogSource.LogInfo("[AUAS] plugin loaded. scanner active.");
    }

    public override bool Unload()
    {
        HarmonyInstance?.UnpatchSelf();
        LogSource.LogInfo("[AUAS] unloaded, harmony patches removed.");
        return true;
    }

    internal static void SetActivePack(string p)
    {
        ActivePack.Value = p;
        ActivePackPath = p == PackManager.None ? null : Path.Combine(SwapRootPath, p);
        SwapManager.SetRoot(ActivePackPath);
        SwapManager.Rescan();
        LogSource.LogInfo($"[AUAS] pack -> {p}");
    }

    private void InitConfig()
    {
        DumpAllAssets = Config.Bind("General", "DumpAllAssets", false,
            "Log all loaded asset names to the BepInEx console");

        ActivePack = Config.Bind("General", "ActivePack", "legacy",
            "Active pack folder name under AUAS_Data, or None");

        EnableSpriteSwap = Config.Bind("Swappers", "Sprites", true, "Enable sprite swapping");
        EnableTextureSwap = Config.Bind("Swappers", "Textures", true, "Enable Texture2D swapping");
        EnableAudioSwap = Config.Bind("Swappers", "Audio", true, "Enable audio clip swapping");
        EnableFontSwap = Config.Bind("Swappers", "Fonts", true, "Enable font swapping");
        EnableShaderSwap = Config.Bind("Swappers", "Shaders", true, "Enable shader swapping");
        EnableMaterialSwap = Config.Bind("Swappers", "Materials", true, "Enable material swapping");
        EnablePrefabSwap = Config.Bind("Swappers", "Prefabs", true, "Enable prefab/gameobject swapping");
    }
}
