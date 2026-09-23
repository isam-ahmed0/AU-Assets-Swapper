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
using System.Collections.Generic;
using UnityEngine;

namespace AU_Assets_Swapper;

public class SwapManagerComponent : MonoBehaviour
{
    private string lastScene = "";

    private bool pickMode;
    private int lastPickedID;
    private float nextPickTime;
    private const float PickInterval = 0.3f;
    private readonly List<string> pickLines = new();
    private GUIStyle boxStyle, labelStyle, headerStyle;

    private readonly List<SpriteRenderer> sprRends = new();
    private readonly List<Renderer> rends = new();

    private bool packMenu;
    private List<string> packs = new();
    private GUIStyle btnStyle, btnActiveStyle;
    private readonly Dictionary<SpriteRenderer, Sprite> origSpr = new();
    private readonly Dictionary<Renderer, Texture> origTex = new();

    private void Update()
    {
        // hot reload
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Plugin.SwapManager?.Rescan();
            ScanAndReplace();
            Plugin.LogSource.LogInfo("[AUAS] rescanned (F5)");
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            packMenu = !packMenu;
            if (packMenu) packs = PackManager.GetPacks();
        }

        if (Input.GetKeyDown(KeyCode.F7))
        {
            pickMode = !pickMode;
            lastPickedID = 0;
            pickLines.Clear();
            Plugin.LogSource.LogInfo(pickMode
                ? "[AUAS] pick mode ON - hover over stuff"
                : "[AUAS] pick mode OFF");
        }

        if (pickMode && Time.time >= nextPickTime)
        {
            nextPickTime = Time.time + PickInterval;
            InspectUnderMouse();
        }

        // rescan on scene change
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (scene != lastScene)
        {
            lastScene = scene;
            origSpr.Clear();
            origTex.Clear();
            Plugin.SwapManager?.Rescan();
            ScanAndReplace();
            Plugin.LogSource.LogInfo($"[AUAS] scene changed to '{scene}', rescanning");
        }
    }

    private void InspectUnderMouse()
    {
        try
        {
            var cam = Camera.main;
            if (cam == null) return;

            var mp = Input.mousePosition;
            var wp = cam.ScreenToWorldPoint(new Vector3(mp.x, mp.y, 0f));

            // try 2d physics first
            var hit = Physics2D.OverlapPoint(wp);
            if (hit != null)
            {
                var id = hit.gameObject.GetInstanceID();
                if (id != lastPickedID)
                {
                    lastPickedID = id;
                    LogAssets(hit.gameObject, "2D");
                }
                return;
            }

            // then UI
            if (CheckUI(mp)) return;

            // then fallback to renderer bounds
            CheckRenderers(wp);
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogWarning($"[AUAS] pick error: {ex.Message}");
        }
    }

    private bool CheckUI(Vector3 screenPos)
    {
        foreach (var img in FindObjectsOfType<UnityEngine.UI.Image>())
        {
            if (img == null || !img.gameObject.activeInHierarchy) continue;
            var canvas = img.GetComponentInParent<Canvas>();
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    img.rectTransform, screenPos, canvas?.worldCamera))
                return MarkPicked(img.gameObject, "UI");
        }

        foreach (var txt in FindObjectsOfType<UnityEngine.UI.Text>())
        {
            if (txt == null || !txt.gameObject.activeInHierarchy) continue;
            var canvas = txt.GetComponentInParent<Canvas>();
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    txt.rectTransform, screenPos, canvas?.worldCamera))
                return MarkPicked(txt.gameObject, "UI Text");
        }

        return false;
    }

    private bool MarkPicked(GameObject go, string src)
    {
        var id = go.GetInstanceID();
        if (id == lastPickedID) return true;
        lastPickedID = id;
        LogAssets(go, src);
        return true;
    }

    private void CheckRenderers(Vector3 wp)
    {
        rends.Clear();
        rends.AddRange(FindObjectsOfType<Renderer>());
        Renderer best = null;
        float bestArea = float.MaxValue;

        foreach (var r in rends)
        {
            if (r == null || !r.gameObject.activeInHierarchy) continue;
            var b = r.bounds;
            if (wp.x < b.min.x || wp.x > b.max.x || wp.y < b.min.y || wp.y > b.max.y) continue;

            var area = b.size.x * b.size.y;
            if (area < bestArea) { bestArea = area; best = r; }
        }

        if (best != null)
        {
            var id = best.gameObject.GetInstanceID();
            if (id != lastPickedID)
            {
                lastPickedID = id;
                LogAssets(best.gameObject, "Renderer");
            }
        }
    }

    private void LogAssets(GameObject go, string src)
    {
        pickLines.Clear();

        var path = GetPath(go);
        pickLines.Add($"[{src}] {go.name}");
        pickLines.Add($"Path: {path}");

        var mgr = Plugin.SwapManager;
        bool found = false;

        // sprite renderers
        foreach (var sr in go.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sr == null || sr.sprite == null) continue;
            var sn = sr.sprite.name;
            var sz = $"{sr.sprite.texture.width}x{sr.sprite.texture.height}";
            pickLines.Add($"  Sprite [{sr.gameObject.name}]: {sn} ({sz})");
            if (mgr != null && mgr.HasSprite(sn))
                pickLines.Add("    >> HAS SPRITE REPLACEMENT");
            if (mgr != null && mgr.HasTexture(sn))
                pickLines.Add("    >> HAS TEXTURE REPLACEMENT");
            found = true;
        }

        // ui images
        foreach (var img in go.GetComponentsInChildren<UnityEngine.UI.Image>(true))
        {
            if (img == null || img.sprite == null) continue;
            var sn = img.sprite.name;
            var sz = $"{img.sprite.texture.width}x{img.sprite.texture.height}";
            pickLines.Add($"  UI Image [{img.gameObject.name}]: {sn} ({sz})");
            if (mgr != null && mgr.HasSprite(sn))
                pickLines.Add("    >> HAS SPRITE REPLACEMENT");
            if (mgr != null && mgr.HasTexture(sn))
                pickLines.Add("    >> HAS TEXTURE REPLACEMENT");
            found = true;
        }

        // renderer textures
        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            if (r == null || r is SpriteRenderer) continue;
            var mat = r.sharedMaterial;
            if (mat == null) continue;

            foreach (var prop in mat.GetTexturePropertyNames())
            {
                var t = mat.GetTexture(prop);
                if (t == null) continue;
                pickLines.Add($"  Texture [{r.gameObject.name}] {prop}: {t.name} ({t.width}x{t.height})");
                if (mgr != null && mgr.HasTexture(t.name))
                    pickLines.Add("    >> HAS TEXTURE REPLACEMENT");
                found = true;
            }

            if (mat.HasProperty("_Color"))
            {
                pickLines.Add($"  Color [{r.gameObject.name}]: {mat.color}");
                found = true;
            }
        }

        // ui text / fonts
        foreach (var txt in go.GetComponentsInChildren<UnityEngine.UI.Text>(true))
        {
            if (txt == null) continue;
            var fn = txt.font != null ? txt.font.name : "null";
            pickLines.Add($"  UI Text [{txt.gameObject.name}]: \"{txt.text}\" (Font: {fn})");
            found = true;
        }

        if (!found)
            pickLines.Add("  (no visual components)");

        foreach (var line in pickLines)
            Plugin.LogSource.LogInfo($"[AUAS-PICK] {line}");
    }

    private void OnGUI()
    {
        if (!pickMode && pickLines.Count == 0 && !packMenu) return;
        InitStyles();

        if (packMenu) DrawPackMenu();

        float x = 10f, y = 10f, w = 520f, lh = 20f;

        if (pickMode)
        {
            GUI.Box(new Rect(x, y, w, 28f), "", boxStyle);
            GUI.contentColor = Color.cyan;
            GUI.Label(new Rect(x + 8f, y + 4f, w - 16f, 22f),
                "AUAS Pick Mode: ON  |  F5=Rescan  F6=Packs  F7=Toggle", headerStyle);
            GUI.contentColor = Color.white;
            y += 34f;
        }

        if (pickLines.Count > 0)
        {
            float h = pickLines.Count * lh + 16f;
            GUI.Box(new Rect(x, y, w, h), "", boxStyle);

            float ly = y + 8f;
            foreach (var line in pickLines)
            {
                if (line.StartsWith("  >>")) GUI.contentColor = Color.green;
                else if (line.StartsWith("[")) GUI.contentColor = Color.yellow;
                else if (line.StartsWith("Path:")) GUI.contentColor = Color.gray;
                else GUI.contentColor = Color.white;

                GUI.Label(new Rect(x + 8f, ly, w - 16f, lh), line, labelStyle);
                ly += lh;
            }
            GUI.contentColor = Color.white;
        }
    }

    private void InitStyles()
    {
        if (boxStyle != null) return;

        boxStyle = new GUIStyle(GUI.skin.box)
        { normal = { background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.85f)) } };

        labelStyle = new GUIStyle(GUI.skin.label)
        { fontSize = 14, fontStyle = FontStyle.Normal, richText = true,
          normal = { textColor = Color.white } };

        headerStyle = new GUIStyle(GUI.skin.label)
        { fontSize = 15, fontStyle = FontStyle.Bold,
          normal = { textColor = Color.cyan } };

        btnStyle = new GUIStyle(GUI.skin.button)
        { fontSize = 14, alignment = TextAnchor.MiddleLeft };

        btnActiveStyle = new GUIStyle(btnStyle)
        { normal = { textColor = Color.green }, hover = { textColor = Color.green } };
    }

    private void DrawPackMenu()
    {
        float w = 300f, rh = 32f;
        float h = 40f + (packs.Count + 2) * rh;
        float x = (Screen.width - w) / 2f, y = (Screen.height - h) / 2f;

        GUI.Box(new Rect(x, y, w, h), "", boxStyle);
        GUI.contentColor = Color.cyan;
        GUI.Label(new Rect(x + 10f, y + 8f, w - 20f, 24f),
            $"AUAS Packs  [{Plugin.ActivePack.Value}]", headerStyle);
        GUI.contentColor = Color.white;

        float by = y + 36f;
        if (GUI.Button(new Rect(x + 10f, by, w - 20f, 26f),
                PackManager.None == Plugin.ActivePack.Value ? "None  *" : "None",
                PackManager.None == Plugin.ActivePack.Value ? btnActiveStyle : btnStyle))
            SwitchPack(PackManager.None);
        by += rh;

        foreach (var p in packs)
        {
            bool act = p == Plugin.ActivePack.Value;
            if (GUI.Button(new Rect(x + 10f, by, w - 20f, 26f), act ? p + "  *" : p,
                    act ? btnActiveStyle : btnStyle))
                SwitchPack(p);
            by += rh;
        }

        if (GUI.Button(new Rect(x + 10f, by, w - 20f, 26f), "Close", btnStyle))
            packMenu = false;
    }

    private void SwitchPack(string p)
    {
        Plugin.SetActivePack(p);
        ScanAndReplace();
        packs = PackManager.GetPacks();
    }

    // lazy solid color texture
    private static Texture2D MakeTex(int w, int h, Color c)
    {
        var px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = c;
        var t = new Texture2D(w, h);
        t.SetPixels(px);
        t.Apply();
        return t;
    }

    private static string GetPath(GameObject go)
    {
        var parts = new List<string>();
        var cur = go.transform;
        while (cur != null) { parts.Add(cur.name); cur = cur.parent; }
        parts.Reverse();
        return string.Join("/", parts);
    }

    // ---- scene scan & replace ----

    private void ScanAndReplace()
    {
        var mgr = Plugin.SwapManager;
        if (mgr == null) return;

        try
        {
            if (Plugin.ActivePackPath == null)
            {
                foreach (var kv in origSpr)
                    if (kv.Key != null) kv.Key.sprite = kv.Value;
                foreach (var kv in origTex)
                    if (kv.Key != null) kv.Key.material.mainTexture = kv.Value;
                origSpr.Clear();
                origTex.Clear();
                return;
            }

            ReplaceSprites(mgr);
            ReplaceRenders(mgr);
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogWarning($"[AUAS] scan error: {ex.Message}");
        }
    }

    private int ReplaceSprites(AssetSwapManager mgr)
    {
        sprRends.Clear();
        sprRends.AddRange(FindObjectsOfType<SpriteRenderer>());

        int count = 0;
        foreach (var sr in sprRends)
        {
            if (sr == null || sr.sprite == null) continue;
            if (!sr.gameObject.activeInHierarchy) continue;

            var name = sr.sprite.name;
            if (mgr.HasSprite(name))
            {
                var repl = mgr.LoadReplacementSprite(name);
                if (repl != null)
                {
                    if (!origSpr.ContainsKey(sr)) origSpr[sr] = sr.sprite;
                    sr.sprite = repl; count++;
                }
            }
            else if (mgr.HasTexture(name))
            {
                var tex = mgr.LoadReplacementTexture(name);
                if (tex != null)
                {
                    if (!origSpr.ContainsKey(sr)) origSpr[sr] = sr.sprite;
                    var newSpr = Sprite.Create(tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f), sr.sprite.pixelsPerUnit);
                    newSpr.name = name;
                    sr.sprite = newSpr;
                    count++;
                }
            }
        }
        return count;
    }

    private int ReplaceRenders(AssetSwapManager mgr)
    {
        rends.Clear();
        rends.AddRange(FindObjectsOfType<Renderer>());

        int count = 0;
        foreach (var r in rends)
        {
            if (r == null || !r.gameObject.activeInHierarchy) continue;
            if (r is SpriteRenderer) continue;

            var mat = r.sharedMaterial;
            if (mat == null || !mat.HasProperty("_MainTex") || mat.mainTexture == null) continue;

            var tn = mat.mainTexture.name;
            if (mgr.HasTexture(tn))
            {
                var repl = mgr.LoadReplacementTexture(tn);
                if (repl != null)
                {
                    if (!origTex.ContainsKey(r)) origTex[r] = mat.mainTexture;
                    r.material.mainTexture = repl; count++;
                }
            }
        }
        return count;
    }
}
