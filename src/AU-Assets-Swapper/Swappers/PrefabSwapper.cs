// Not tested yet. but i think works. if it works, it works.
using UnityEngine;

namespace AU_Assets_Swapper.Swappers;

internal static class PrefabSwapper
{
    public static GameObject InstantiateReplacement(GameObject orig, GameObject repl)
    {
        if (orig == null || repl == null) return orig;

        var inst = Object.Instantiate(repl);
        if (inst == null) return orig;

        // copy transform from original
        inst.name = orig.name;
        inst.transform.position = orig.transform.position;
        inst.transform.rotation = orig.transform.rotation;
        inst.transform.localScale = orig.transform.localScale;
        return inst;
    }
}
