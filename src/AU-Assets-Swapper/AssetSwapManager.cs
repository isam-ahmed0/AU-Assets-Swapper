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

namespace AU_Assets_Swapper;

internal class AssetSwapManager
{
    private string _root;

    // case-insensitive so unity names match regardless of OS casing
    private readonly Dictionary<string, string> sprites = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> textures = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> audio = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> fonts = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> shaders = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> materials = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> prefabs = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, Sprite> spriteCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Texture2D> texCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AudioClip> audioCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Font> fontCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AssetBundle> bundles = new(StringComparer.OrdinalIgnoreCase);

    public AssetSwapManager(string rootPath)
    {
        _root = rootPath;
    }

    public void SetRoot(string rootPath) => _root = rootPath;
// It is made by Isam
    public void ScanAndLoadAssets()
    {
        sprites.Clear(); textures.Clear(); audio.Clear();
        fonts.Clear(); shaders.Clear(); materials.Clear(); prefabs.Clear();

        Scan("Sprites", sprites);
        Scan("Textures", textures);
        Scan("Audio", audio);
        Scan("Fonts", fonts);
        Scan("Shaders", shaders);
        Scan("Materials", materials);
        Scan("Prefabs", prefabs);

        int total = sprites.Count + textures.Count + audio.Count +
                    fonts.Count + shaders.Count + materials.Count + prefabs.Count;

        Plugin.LogSource.LogInfo(
            $"[AUAS] Scanned: {sprites.Count} sprites, {textures.Count} textures, " +
            $"{audio.Count} audio, {fonts.Count} fonts, {shaders.Count} shaders, " +
            $"{materials.Count} materials, {prefabs.Count} prefabs - total {total}");
    }

    public void Rescan()
    {
        foreach (var b in bundles.Values)
            b?.Unload(false);
        bundles.Clear();
        spriteCache.Clear(); texCache.Clear(); audioCache.Clear(); fontCache.Clear();
        ScanAndLoadAssets();
    }
// Thanks for using my mod. If you are developer you can contribute.
    private void Scan(string cat, Dictionary<string, string> map)
    {
        if (_root == null) return;
        var dir = Path.Combine(_root, cat);
        if (!Directory.Exists(dir)) return;

        foreach (var f in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
        {
            if (f.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;
            map[Path.GetFileNameWithoutExtension(f)] = f;
        }
    }

    // --- has-replacement checks (gated by config toggles) ---

    public bool HasSprite(string n) => Plugin.EnableSpriteSwap.Value && sprites.ContainsKey(n);
    public bool HasTexture(string n) => Plugin.EnableTextureSwap.Value && textures.ContainsKey(n);
    public bool HasAudio(string n) => Plugin.EnableAudioSwap.Value && audio.ContainsKey(n);
    public bool HasFont(string n) => Plugin.EnableFontSwap.Value && fonts.ContainsKey(n);
    public bool HasShader(string n) => Plugin.EnableShaderSwap.Value && shaders.ContainsKey(n);
    public bool HasMaterial(string n) => Plugin.EnableMaterialSwap.Value && materials.ContainsKey(n);
    public bool HasPrefab(string n) => Plugin.EnablePrefabSwap.Value && prefabs.ContainsKey(n);
// Thanks for reading my code.
    public bool HasAnyReplacement()
    {
        return sprites.Count > 0 || textures.Count > 0 || audio.Count > 0 ||
               fonts.Count > 0 || shaders.Count > 0 || materials.Count > 0 || prefabs.Count > 0;
    }

    // try every asset type in priority order. used by all the harmony patches
    // so we don't have to copy-paste this shit 3 times
    public UnityEngine.Object TryFindReplacement(string name)
    {
        // textures first because sprites also show up as textures sometimes
        if (HasTexture(name))
        {
            var t = LoadReplacementTexture(name);
            if (t != null) return t;
        }

        if (HasSprite(name))
        {
            var s = LoadReplacementSprite(name);
            if (s != null) return s;
        }

        if (HasAudio(name))
        {
            var c = LoadReplacementAudio(name);
            if (c != null) return c;
        }

        if (HasFont(name))
        {
            var f = LoadReplacementFont(name);
            if (f != null) return f;
        }

        if (HasShader(name))
        {
            var sh = LoadReplacementShader(name);
            if (sh != null) return sh;
        }

        if (HasMaterial(name))
        {
            var m = LoadReplacementMaterial(name);
            if (m != null) return m;
        }

        if (HasPrefab(name))
        {
            var p = LoadReplacementPrefab(name);
            if (p != null) return p;
        }

        return null;
    }

    // typed variant for Resources.Load(path, type)
    public UnityEngine.Object TryFindReplacement(string name, Type t)
    {
        if (t == typeof(Texture2D) && HasTexture(name))
            return LoadReplacementTexture(name);
        if (t == typeof(Sprite) && HasSprite(name))
            return LoadReplacementSprite(name);
        if (t == typeof(AudioClip) && HasAudio(name))
            return LoadReplacementAudio(name);
        if (t == typeof(Font) && HasFont(name))
            return LoadReplacementFont(name);
        if (t == typeof(Shader) && HasShader(name))
            return LoadReplacementShader(name);
        if (t == typeof(Material) && HasMaterial(name))
            return LoadReplacementMaterial(name);
        if (t == typeof(GameObject) && HasPrefab(name))
            return LoadReplacementPrefab(name);
        return null;
    }

    // --- loaders (cached where it makes sense) ---

    public Sprite LoadReplacementSprite(string assetName)
    {
        if (spriteCache.TryGetValue(assetName, out var cached))
            return cached;

        if (!sprites.TryGetValue(assetName, out var path))
            return null;

        try
        {
            var tex = ImageLoader.LoadTexture2D(path);
            if (tex == null)
            {
                Plugin.LogSource.LogWarning($"[AUAS] sprite tex load fail: {assetName}");
                return null;
            }

            var spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 100f);
            spr.name = assetName;
            spriteCache[assetName] = spr;
            Plugin.LogSource.LogInfo($"[AUAS] sprite: {assetName} ({tex.width}x{tex.height})");
            return spr;
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError($"[AUAS] sprite err '{assetName}': {ex.Message}");
            return null;
        }
    }

    public Texture2D LoadReplacementTexture(string assetName)
    {
        if (texCache.TryGetValue(assetName, out var cached))
            return cached;

        if (!textures.TryGetValue(assetName, out var path))
            return null;

        try
        {
            var tex = ImageLoader.LoadTexture2D(path);
            if (tex == null)
            {
                Plugin.LogSource.LogWarning($"[AUAS] tex load fail: {assetName}");
                return null;
            }

            tex.name = assetName;
            texCache[assetName] = tex;
            Plugin.LogSource.LogInfo($"[AUAS] texture: {assetName} ({tex.width}x{tex.height})");
            return tex;
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError($"[AUAS] tex err '{assetName}': {ex.Message}");
            return null;
        }
    }

    public AudioClip LoadReplacementAudio(string assetName)
    {
        if (audioCache.TryGetValue(assetName, out var cached))
            return cached;

        if (!audio.TryGetValue(assetName, out var path))
            return null;

        try
        {
            var clip = AudioLoader.LoadAudioClip(path, assetName);
            if (clip == null)
            {
                Plugin.LogSource.LogWarning($"[AUAS] audio load fail: {assetName}");
                return null;
            }

            audioCache[assetName] = clip;
            Plugin.LogSource.LogInfo($"[AUAS] audio: {assetName}");
            return clip;
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError($"[AUAS] audio err '{assetName}': {ex.Message}");
            return null;
        }
    }

    public Font LoadReplacementFont(string assetName)
    {
        if (fontCache.TryGetValue(assetName, out var cached))
            return cached;

        if (!fonts.TryGetValue(assetName, out var path))
            return null;

        try
        {
            var f = new Font(path);
            f.name = assetName;
            fontCache[assetName] = f;
            Plugin.LogSource.LogInfo($"[AUAS] font: {assetName}");
            return f;
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError($"[AUAS] font err '{assetName}': {ex.Message}");
            return null;
        }
    }

    public Shader LoadReplacementShader(string name)
    {
        if (!shaders.TryGetValue(name, out var path))
            return null;
        return LoadFromBundle<Shader>(name, path);
    }

    public Material LoadReplacementMaterial(string name)
    {
        if (!materials.TryGetValue(name, out var path))
            return null;
        return LoadFromBundle<Material>(name, path);
    }

    public GameObject LoadReplacementPrefab(string name)
    {
        if (!prefabs.TryGetValue(name, out var path))
            return null;
        return LoadFromBundle<GameObject>(name, path);
    }

    private T LoadFromBundle<T>(string assetName, string bundlePath) where T : UnityEngine.Object
    {
        try
        {
            if (!bundles.TryGetValue(bundlePath, out var b) || b == null)
            {
                b = AssetBundle.LoadFromFile(bundlePath);
                if (b == null)
                {
                    Plugin.LogSource.LogWarning($"[AUAS] bundle load fail: {bundlePath}");
                    return null;
                }
                bundles[bundlePath] = b;
            }

            var obj = b.LoadAsset(assetName) as T;
            if (obj != null)
                Plugin.LogSource.LogInfo($"[AUAS] {typeof(T).Name}: {assetName} (from bundle)");
            return obj;
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError($"[AUAS] bundle err {typeof(T).Name} '{assetName}': {ex.Message}");
            return null;
        }
    }

    public void LogLoadedAsset(string assetName, Type t)
    {
        if (Plugin.DumpAllAssets.Value)
            Plugin.LogSource.LogInfo($"[AUAS-DUMP] {t.Name}: {assetName}");
    }
}
