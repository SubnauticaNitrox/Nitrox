using System.Collections;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class VehicleDockingProcessor : ClientPacketProcessor<VehicleDocking>
{
    private readonly Vehicles vehicles;

    public VehicleDockingProcessor(Vehicles vehicles)
    {
        this.vehicles = vehicles;
    }

    public override void Process(VehicleDocking packet)
    {
        if (!NitroxEntity.TryGetComponentFrom(packet.VehicleId, out Vehicle vehicle))
        {
            Log.Error($"[{nameof(VehicleDockingProcessor)}] could not find Vehicle component on {packet.VehicleId}");
            return;
        }

        if (!NitroxEntity.TryGetComponentFrom(packet.DockId, out VehicleDockingBay dockingBay))
        {
            Log.Error($"[{nameof(VehicleDockingProcessor)}] could not find VehicleDockingBay component on {packet.DockId}");
            return;
        }

        if (vehicle.TryGetComponent(out MovementReplicator vehicleMovementReplicator))
        {
            vehicleMovementReplicator.enabled = false;
            Log.Debug($"[{nameof(VehicleDockingProcessor)}] Disabled MovementReplicator on {packet.VehicleId}");
        }

        vehicle.StartCoroutine(DelayAnimationAndDisablePiloting(vehicle, dockingBay, packet.VehicleId, packet.PlayerId));
    }

    private IEnumerator DelayAnimationAndDisablePiloting(Vehicle vehicle, VehicleDockingBay vehicleDockingBay, NitroxId vehicleId, ushort playerId)
    {
        yield return Yielders.WaitFor1Second;
        // DockVehicle sets the rigid body kinematic of the vehicle to true, we don't want that behaviour
        // Therefore disable kinematic (again) to remove the bouncing behavior
        DockRemoteVehicle(vehicleDockingBay, vehicle);
        vehicle.useRigidbody.isKinematic = false;
        yield return Yielders.WaitFor2Seconds;
        vehicles.SetOnPilotMode(vehicleId, playerId, false);
    }

    /// Copy of <see cref="VehicleDockingBay.DockVehicle"/> without the player centric bits
    private static void DockRemoteVehicle(VehicleDockingBay bay, Vehicle vehicle)
    {
        bay.dockedVehicle = vehicle;
        LargeWorldStreamer.main.cellManager.UnregisterEntity(bay.dockedVehicle.gameObject);
        bay.dockedVehicle.transform.parent = bay.GetSubRoot().transform;
        vehicle.docked = true;
        bay.vehicle_docked_param = true;
        bay.GetSubRoot().BroadcastMessage("UnlockDoors", SendMessageOptions.DontRequireReceiver);

        if (false) // TODO: Should be executed when sym lock on vehicle or cyclops or both, idk
        {
            bay.CancelInvoke("RepairVehicle");
            bay.InvokeRepeating("RepairVehicle", 0.0f, 5f);
        }
    }
}
