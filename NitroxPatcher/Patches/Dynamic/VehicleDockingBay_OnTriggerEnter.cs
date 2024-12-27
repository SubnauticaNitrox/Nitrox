using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class VehicleDockingBay_OnTriggerEnter : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((VehicleDockingBay t) => t.OnTriggerEnter(default(Collider)));

    public static bool Prefix(VehicleDockingBay __instance, Collider other, ref Vehicle __state)
    {
        Vehicle vehicle = other.GetComponentInParent<Vehicle>();
        __state = __instance.interpolatingVehicle;
        Optional<NitroxId> opVehicleId = vehicle.GetId();
        return !vehicle || (opVehicleId.HasValue && Resolve<SimulationOwnership>().HasAnyLockType(opVehicleId.Value));
    }

    public static void Postfix(VehicleDockingBay __instance, ref Vehicle __state)
    {
        Vehicle interpolatingVehicle = __instance.interpolatingVehicle;
        // Only send data, when interpolatingVehicle changes to avoid multiple packages send
        if (!interpolatingVehicle || interpolatingVehicle == __state)
        {
            return;
        }

        if (__instance.gameObject.TryGetIdOrWarn(out NitroxId dockId) &&
            interpolatingVehicle.TryGetIdOrWarn(out NitroxId vehicleId) &&
            Resolve<SimulationOwnership>().HasAnyLockType(vehicleId))
        {
            Vehicles.EngagePlayerMovementSuppressor(interpolatingVehicle);
            Resolve<IPacketSender>().Send(new VehicleDocking(vehicleId, dockId, Resolve<IMultiplayerSession>().Reservation.PlayerId));
        }
    }
}
