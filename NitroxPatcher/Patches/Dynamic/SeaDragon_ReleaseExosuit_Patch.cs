using System.Reflection;
using NitroxClient.MonoBehaviours.Vehicles;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Enables exosuits's position sync when they're released from sea dragons
/// </summary>
public sealed partial class SeaDragon_ReleaseExosuit_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeaDragon t) => t.ReleaseExosuit());

    public static void Prefix(SeaDragon __instance)
    {
        if (__instance.holdingExosuit && __instance.holdingExosuit.TryGetComponent(out VehicleMovementReplicator vehicleMovementReplicator))
        {
            vehicleMovementReplicator.enabled = true;
        }
    }
}
