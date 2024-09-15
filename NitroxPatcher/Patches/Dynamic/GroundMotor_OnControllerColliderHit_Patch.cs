using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Disables a useless callback for <see cref="CyclopsMotor"/>.
/// </summary>
public sealed partial class GroundMotor_OnControllerColliderHit_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((GroundMotor t) => t.OnControllerColliderHit(default));

    public static bool Prefix(GroundMotor __instance, ControllerColliderHit hit)
    {
        if (__instance is CyclopsMotor)
        {
            // Just cancel this as we don't need this on CyclopsMotor
            return false;
        }
        return true;
    }
}
