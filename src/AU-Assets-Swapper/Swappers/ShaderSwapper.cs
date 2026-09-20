// Not tested yet. but i think works. if it works, it works. I dont know test it.
using UnityEngine;

namespace AU_Assets_Swapper.Swappers;

internal static class ShaderSwapper
{
    public static Shader FindShaderInBundle(AssetBundle bundle, string shaderName)
    {
        if (bundle == null) return null;

        var assets = bundle.LoadAllAssets();

        // try exact match first
        foreach (var asset in assets)
        {
            var s = asset as Shader;
            if (s == null) continue;
            if (s.name == shaderName || s.name.EndsWith("/" + shaderName))
                return s;
        }

        // fallback: just grab whatever shader we can find so the scene doesn't explode
        foreach (var asset in assets)
        {
            var s = asset as Shader;
            if (s != null)
            {
                Plugin.LogSource.LogWarning($"[AUAS] shader '{shaderName}' not found, using: {s.name}");
                return s;
            }
        }

        return null;
    }
}
