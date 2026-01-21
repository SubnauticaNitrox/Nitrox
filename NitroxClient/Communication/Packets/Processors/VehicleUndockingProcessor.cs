using System.Collections;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class VehicleUndockingProcessor(Vehicles vehicles, PlayerManager remotePlayerManager) : IClientPacketProcessor<VehicleUndocking>
{
    private readonly PlayerManager remotePlayerManager = remotePlayerManager;
    private readonly Vehicles vehicles = vehicles;

    public Task Process(ClientProcessorContext context, VehicleUndocking packet)
    {
        GameObject vehicleGo = NitroxEntity.RequireObjectFrom(packet.VehicleId);
        GameObject vehicleDockingBayGo = NitroxEntity.RequireObjectFrom(packet.DockId);

        Vehicle vehicle = vehicleGo.RequireComponent<Vehicle>();
        VehicleDockingBay vehicleDockingBay = vehicleDockingBayGo.RequireComponent<VehicleDockingBay>();

        using (PacketSuppressor<VehicleUndocking>.Suppress())
        {
            if (packet.UndockingStart)
            {
                StartVehicleUndocking(packet, vehicleGo, vehicle, vehicleDockingBay);
            }
            else
            {
                FinishVehicleUndocking(packet, vehicle, vehicleDockingBay);
            }
        }
        return Task.CompletedTask;
    }

    private static IEnumerator StartUndockingAnimation(VehicleDockingBay vehicleDockingBay)
    {
        yield return Yielders.WaitFor2Seconds;
        vehicleDockingBay.vehicle_docked_param = false;
    }

    private void StartVehicleUndocking(VehicleUndocking packet, GameObject vehicleGo, Vehicle vehicle, VehicleDockingBay vehicleDockingBay)
    {
        vehicleDockingBay.subRoot.BroadcastMessage("OnLaunchBayOpening", SendMessageOptions.DontRequireReceiver);
        SkyEnvironmentChanged.Broadcast(vehicleGo, (GameObject)null);

        if (remotePlayerManager.TryFind(packet.PlayerId, out RemotePlayer player))
        {
            // It can happen that the player turns in circles around himself in the vehicle. This stops it.
            player.RigidBody.angularVelocity = Vector3.zero;
            vehicles.SetOnPilotMode(packet.VehicleId, packet.PlayerId, true);
        }
        vehicleDockingBay.StartCoroutine(StartUndockingAnimation(vehicleDockingBay));

        if (vehicle.TryGetComponent(out MovementReplicator vehicleMovementReplicator))
        {
            vehicleMovementReplicator.ClearBuffer();
            Log.Debug($"[{nameof(VehicleDockingProcessor)}] Clear MovementReplicator on {packet.VehicleId}");
        }
    }

    private void FinishVehicleUndocking(VehicleUndocking packet, Vehicle vehicle, VehicleDockingBay vehicleDockingBay)
    {
        if (vehicleDockingBay.GetSubRoot().isCyclops)
        {
            vehicleDockingBay.SetVehicleUndocked();
        }
        vehicleDockingBay.dockedVehicle = null;
        vehicleDockingBay.CancelInvoke(nameof(VehicleDockingBay.RepairVehicle));
        vehicle.docked = false;
        if (remotePlayerManager.TryFind(packet.PlayerId, out RemotePlayer player))
        {
            // Sometimes the player is not set accordingly which stretches the player's model instead of putting them in place
            // after undocking. This fixes it (the player rigid body seems to not be set right sometimes)
            player.SetSubRoot(null);
            player.SetVehicle(null);
            player.SetVehicle(vehicle);
        }
        vehicles.SetOnPilotMode(packet.VehicleId, packet.PlayerId, true);

        if (vehicle.TryGetComponent(out MovementReplicator vehicleMovementReplicator))
        {
            vehicleMovementReplicator.enabled = true;
            Log.Debug($"[{nameof(VehicleDockingProcessor)}] Enabled MovementReplicator on {packet.VehicleId}");
        }

        Log.Debug("Set vehicle undocking complete");
    }
}
