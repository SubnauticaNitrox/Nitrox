using System.Reflection;
using NitroxClient.MonoBehaviours.Vehicles;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Provide exosuits driven by remote players with the right velocity for the animations to use
/// </summary>
public sealed partial class Exosuit_UpdateAnimations_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Exosuit t) => t.UpdateAnimations());

    public static void Prefix(Exosuit __instance, out Vector3? __state)
    {
        if (__instance.TryGetComponent(out ExosuitMovementReplicator exosuitMovementReplicator))
        {
            __state = __instance.useRigidbody.velocity;
            __instance.useRigidbody.velocity = exosuitMovementReplicator.velocity;
        }
        else
        {
            __state = null;
        }
    }

    public static void Postfix(Exosuit __instance, Vector3? __state)
    {
        if (__state.HasValue)
        {
            __instance.useRigidbody.velocity = __state.Value;
        }
    }
}
