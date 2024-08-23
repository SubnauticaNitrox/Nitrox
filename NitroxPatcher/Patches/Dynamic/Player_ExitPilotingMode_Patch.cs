using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Setups the local player when it stops piloting a cyclops
/// </summary>
public sealed partial class Player_ExitPilotingMode_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Player t) => t.ExitPilotingMode(default));

    public static void Postfix(Player __instance)
    {
        if (__instance.currentSub && __instance.currentSub.isCyclops && __instance.TryGetComponent(out CyclopsMotor cyclopsMotor))
        {
            __instance.transform.parent = __instance.currentSub.transform;
            cyclopsMotor.ToggleCyclopsMotor(true);
        }
    }
}
