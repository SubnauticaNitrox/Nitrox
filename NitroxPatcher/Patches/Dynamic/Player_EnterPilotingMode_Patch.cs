using System.Reflection;
using NitroxClient.MonoBehaviours.Cyclops;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Setups the cyclops and the local player when it starts piloting it.
/// </summary>
public sealed partial class Player_EnterPilotingMode_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Player t) => t.EnterPilotingMode(default, default));

    public static void Postfix(Player __instance)
    {
        if (__instance.currentSub && __instance.currentSub.TryGetComponent(out NitroxCyclops nitroxCyclops))
        {
            __instance.rigidBody.isKinematic = true;
            nitroxCyclops.SetBroadcasting();
        }
    }
}
