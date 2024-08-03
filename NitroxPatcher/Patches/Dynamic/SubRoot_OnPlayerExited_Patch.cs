using System.Reflection;
using NitroxClient.MonoBehaviours.Cyclops;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Unregisters the local player from the cyclops it leaves.
/// </summary>
public sealed partial class SubRoot_OnPlayerExited_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubRoot t) => t.OnPlayerExited(default));

    public static void Postfix(SubRoot __instance)
    {
        if (__instance.TryGetComponent(out NitroxCyclops nitroxCyclops))
        {
            nitroxCyclops.OnLocalPlayerExit();
        }
    }
}
