using System.Reflection;
using NitroxClient.MonoBehaviours.Cyclops;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Registers the local player in the cyclops it enters.
/// </summary>
public sealed partial class SubRoot_OnPlayerEntered_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubRoot t) => t.OnPlayerEntered(default));

    public static void Postfix(SubRoot __instance)
    {
        if (__instance.TryGetComponent(out NitroxCyclops nitroxCyclops))
        {
            nitroxCyclops.OnLocalPlayerEnter();
        }
    }
}
