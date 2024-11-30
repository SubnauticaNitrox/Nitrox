using System.Reflection;
using NitroxClient.MonoBehaviours.Vehicles;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Enables vehicle's position sync when they're released from reapers
/// </summary>
public sealed partial class ReaperLeviathan_ReleaseVehicle_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ReaperLeviathan t) => t.ReleaseVehicle());

    public static void Prefix(ReaperLeviathan __instance)
    {
        if (__instance.holdingVehicle && __instance.holdingVehicle.TryGetComponent(out VehicleMovementReplicator vehicleMovementReplicator))
        {
            vehicleMovementReplicator.enabled = true;
        }
    }
}
