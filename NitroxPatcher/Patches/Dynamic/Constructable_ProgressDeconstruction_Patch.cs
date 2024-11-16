using System.Reflection;
using NitroxClient.MonoBehaviours.Cyclops;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Unregisters constructables from virtual cyclops when they're fully deconstructed.
/// </summary>
public sealed partial class Constructable_ProgressDeconstruction_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Constructable t) => t.ProgressDeconstruction());

    public static void Prefix(Constructable __instance)
    {
        if (__instance.constructedAmount <= 0f && __instance.transform.parent &&
            __instance.transform.parent.TryGetComponent(out NitroxCyclops nitroxCyclops) && nitroxCyclops.Virtual)
        {
            nitroxCyclops.Virtual.UnregisterConstructable(__instance.gameObject);
        }
    }
}
