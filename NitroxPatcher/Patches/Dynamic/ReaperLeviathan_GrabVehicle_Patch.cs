using System.Reflection;
using NitroxClient.MonoBehaviours.Vehicles;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Temporarily disables vehicle's position sync while they're grabbed by reapers
/// </summary>
public sealed partial class ReaperLeviathan_GrabVehicle_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ReaperLeviathan t) => t.GrabVehicle(default, default));

    public static void Prefix(Vehicle vehicle)
    {
        if (vehicle.TryGetComponent(out VehicleMovementReplicator vehicleMovementReplicator))
        {
            vehicleMovementReplicator.enabled = false;
        }
    }
}
