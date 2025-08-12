using System.Collections;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Vehicles;
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

        if (vehicle.TryGetComponent(out VehicleMovementReplicator vehicleMovementReplicator))
        {
            vehicleMovementReplicator.enabled = false;
            Log.Debug($"[{nameof(VehicleDockingProcessor)}] Disabled VehicleMovementReplicator on {packet.VehicleId}");
        }

        vehicle.StartCoroutine(DelayAnimationAndDisablePiloting(vehicle, vehicleMovementReplicator, dockingBay, packet.VehicleId, packet.PlayerId));
    }

    private IEnumerator DelayAnimationAndDisablePiloting(Vehicle vehicle, VehicleMovementReplicator vehicleMovementReplicator, VehicleDockingBay vehicleDockingBay, NitroxId vehicleId, ushort playerId)
    {
        // Consider the vehicle movement latency (we don't teleport the vehicle to the docking position)
        if (vehicleMovementReplicator)
        {
            // NB: We don't have a lifetime ahead of us
            float waitTime = Mathf.Clamp(vehicleMovementReplicator.maxAllowedLatency, 0f, 2f);
            yield return new WaitForSeconds(waitTime);
        }
        else
        {
            yield return Yielders.WaitFor1Second;
        }

        // DockVehicle sets the rigid body kinematic of the vehicle to true, we don't want that behaviour
        // Therefore disable kinematic (again) to remove the bouncing behavior
#if SUBNAUTICA
        DockRemoteVehicle(vehicleDockingBay, vehicle);
#elif BELOWZERO
        DockRemoteDockable(vehicleDockingBay, vehicle.dockable);
#endif
        vehicle.useRigidbody.isKinematic = false;

        yield return Yielders.WaitFor2Seconds;
        vehicles.SetOnPilotMode(vehicleId, playerId, false);
    }

#if SUBNAUTICA
    /// Copy of <see cref="VehicleDockingBay.DockVehicle"/> without the player centric bits
    private void DockRemoteVehicle(VehicleDockingBay bay, Vehicle vehicle)
    {
        bay.dockedVehicle = vehicle;
        LargeWorldStreamer.main.cellManager.UnregisterEntity(bay.dockedVehicle.gameObject);
        bay.dockedVehicle.transform.parent = bay.GetSubRoot().transform;
        vehicle.docked = true;
        bay.vehicle_docked_param = true;
        SkyEnvironmentChanged.Broadcast(vehicle.gameObject, bay.subRoot);
        bay.GetSubRoot().BroadcastMessage("UnlockDoors", SendMessageOptions.DontRequireReceiver);
        
        // We are only actually adding the health if we have a lock on the vehicle so we're fine to keep this routine going on.
        // If vehicle ownership changes then it'll still be fine because the verification will still be on the vehicle ownership.
        bay.CancelInvoke(nameof(VehicleDockingBay.RepairVehicle));
        bay.InvokeRepeating(nameof(VehicleDockingBay.RepairVehicle), 0.0f, 5f);
    }
#elif BELOWZERO
    /// Copy of <see cref="VehicleDockingBay.Dock"/> without the player centric bits
    private void DockRemoteDockable(VehicleDockingBay bay, Dockable dockable)
    {
        bay.dockedObject = dockable;
        LargeWorldStreamer.main.cellManager.UnregisterEntity(bay.dockedObject.gameObject);
        bay.dockedObject.transform.parent = bay.GetSubRoot().transform;
        dockable.SetDocked(bay, bay.dockType);
        if (bay.MoonpoolExpansionEnabled())
        {
            bay.expansionManager.StartDocking();
        }
        bay.docked_param = true;
        SkyEnvironmentChanged.Broadcast(dockable.gameObject, bay.subRoot);
        bay.GetSubRoot().BroadcastMessage("UnlockDoors", SendMessageOptions.DontRequireReceiver);
    }
#endif
}
