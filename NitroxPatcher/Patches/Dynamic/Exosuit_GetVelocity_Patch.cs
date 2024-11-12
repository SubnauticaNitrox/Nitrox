using System.Reflection;
using NitroxClient.MonoBehaviours.Vehicles;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Gives the real velocity data for <see cref="FootstepSounds"/> to play the right step sounds.
/// </summary>
public sealed partial class Exosuit_GetVelocity_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((Exosuit t) => ((IGroundMoveable)t).GetVelocity());

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
