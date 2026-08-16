using System;
using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using UnityEngine;

namespace NitroxClient.Patching.Patches.Dynamic;

internal sealed partial class VehicleDockingBay_OnTriggerEnter : NitroxPatch, IDynamicPatch
{
    private static SimulationOwnership simulationOwnership;
    private static IPacketSender packetSender;
    private static IMultiplayerSession multiplayerSession;
    private static readonly MethodInfo targetMethod = Reflect.Method((VehicleDockingBay t) => t.OnTriggerEnter(default(Collider)));

    public VehicleDockingBay_OnTriggerEnter(SimulationOwnership so, IPacketSender ps, IMultiplayerSession ms)
    {
        simulationOwnership = so ?? throw new ArgumentNullException(nameof(so));
        packetSender = ps ?? throw new ArgumentNullException(nameof(ps));
        multiplayerSession = ms ?? throw new ArgumentNullException(nameof(ms));
    }

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
            simulationOwnership.HasAnyLockType(vehicleId))
        {
            Vehicles.EngagePlayerMovementSuppressor(interpolatingVehicle);
            packetSender.Send(new VehicleDocking(vehicleId, dockId, multiplayerSession.Reservation.SessionId));
        }
    }
}
