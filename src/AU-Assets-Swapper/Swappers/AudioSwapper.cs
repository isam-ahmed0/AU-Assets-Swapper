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

// Not tested yet. but i think works. if it works, it works.
using UnityEngine;

namespace AU_Assets_Swapper.Swappers;

internal static class AudioSwapper
{
    // dead simple swap - if we have a replacement, use it
    public static AudioClip ReplaceClipData(AudioClip original, AudioClip replacement)
    {
        if (original == null || replacement == null) return original;
        return replacement;
    }
}
