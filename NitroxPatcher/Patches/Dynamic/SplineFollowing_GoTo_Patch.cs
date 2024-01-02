using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// SplineFollowing is the internal behavior inside SwimBehavior and WalkBehavior that causes an entity to move.
/// Whenever a NitroxEntity receives a MoveTo action, we'll let the EntityPositionBroadcaster know (internally,
/// it will check if it is an entity we are watching an broadcast when applicable).
/// </summary>
public sealed partial class SplineFollowing_GoTo_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((SplineFollowing t) => t.GoTo(default(Vector3), default(Vector3), default(float)));

    public static void Prefix(SplineFollowing __instance, Vector3 targetPos, Vector3 targetDir, float velocity)
    {
        NitroxEntity nitroxEntity = __instance.GetComponent<NitroxEntity>();

        if (nitroxEntity)
        {
            EntityPositionBroadcaster.RegisterSplineMovementChange(nitroxEntity.Id, __instance.gameObject, targetPos, targetDir, velocity);
        }
    }
}
