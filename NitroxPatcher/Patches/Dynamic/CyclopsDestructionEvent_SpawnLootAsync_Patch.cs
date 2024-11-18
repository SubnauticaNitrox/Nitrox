using System.Collections;
using System.Linq;
using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CyclopsDestructionEvent_SpawnLootAsync_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsDestructionEvent t) => t.SpawnLootAsync());

    public static bool Prefix(ref IEnumerator __result)
    {
        // TODO: sync loot spawned by Cyclops destruction
        __result = EmptyEnumerator();
        return false;
    }

    private static IEnumerator EmptyEnumerator()
    {
        yield break;
    }
}
