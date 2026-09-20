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
