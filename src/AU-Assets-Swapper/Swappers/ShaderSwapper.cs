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
