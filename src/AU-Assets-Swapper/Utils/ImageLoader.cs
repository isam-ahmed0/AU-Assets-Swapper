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
using UnityEngine;
// loadddddddddddddddd.......
namespace AU_Assets_Swapper;

internal static class ImageLoader
{
    public static Texture2D LoadTexture2D(string path)
    {
        if (!File.Exists(path)) return null;

        var bytes = File.ReadAllBytes(path);
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        try
        {
            if (tex.LoadImage(bytes))
                return tex;

            UnityEngine.Object.Destroy(tex);
            return null;
        }
        catch
        {
            UnityEngine.Object.Destroy(tex);
            return null;
        }
    }
}
