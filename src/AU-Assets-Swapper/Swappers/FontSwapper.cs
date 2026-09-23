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

// Not tested yet. but i think works. if it works, it works.
using System.IO;
using UnityEngine;

namespace AU_Assets_Swapper.Swappers;

internal static class FontSwapper
{
    public static Font LoadFromTtf(string path, string name)
    {
        if (!File.Exists(path)) return null;
        try
        {
            var f = new Font(path);
            f.name = name;
            return f;
        }
        catch (System.Exception ex)
        {
            Plugin.LogSource.LogError($"[AUAS] failed to load TTF '{name}': {ex.Message}");
            return null;
        }
    }
}
