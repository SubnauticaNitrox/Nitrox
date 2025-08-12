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
#if SUBNAUTICA
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
#elif BELOWZERO
    // TODO: Check if needs for Sea Truck part is needed like in the original method
    public static bool Prefix(VehicleDockingBay __instance, Collider other, ref Dockable __state)
    {
        Dockable vehicle = other.GetComponentInParent<Dockable>();
        __state = __instance.interpolatingDockable;
        Optional<NitroxId> opVehicleId = vehicle.GetId();
        return !vehicle || (opVehicleId.HasValue && Resolve<SimulationOwnership>().HasAnyLockType(opVehicleId.Value));
    }

    public static void Postfix(VehicleDockingBay __instance, ref Dockable __state)
    {
        Dockable interpolatingVehicle = __instance.interpolatingDockable;
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
#endif
}
