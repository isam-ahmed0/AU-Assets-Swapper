// Not tested yet. but i think works. if it works, it works.
using UnityEngine;

namespace AU_Assets_Swapper.Swappers;

internal static class MaterialSwapper
{
    public static Material ReplaceMaterialData(Material orig, Material repl)
    {
        if (orig == null || repl == null) return orig;

        // clone so we don't mutate the original material
        var mat = new Material(repl);
        mat.name = orig.name;
        if (orig.renderQueue != repl.renderQueue)
            mat.renderQueue = orig.renderQueue;
        return mat;
    }
}
