using System.Reflection;
using NitroxClient.MonoBehaviours.Vehicles;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Gives the real velocity data for <see cref="FootstepSounds"/> to play the right step sounds.
/// </summary>
public sealed partial class Exosuit_GetVelocity_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = typeof(Exosuit).GetMethod("IGroundMoveable.GetVelocity", BindingFlags.NonPublic | BindingFlags.Instance);

    public static bool Prefix(Exosuit __instance, ref Vector3 __result)
    {
        if (__instance.TryGetComponent(out ExosuitMovementReplicator exosuitMovementReplicator))
        {
            __result = exosuitMovementReplicator.velocity;
            return false;
        }

        return true;
    }
}
