using System.Collections;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class VehicleUndockingProcessor : ClientPacketProcessor<VehicleUndocking>
{
    private readonly Vehicles vehicles;
    private readonly PlayerManager remotePlayerManager;

    public VehicleUndockingProcessor(Vehicles vehicles, PlayerManager remotePlayerManager)
    {
        this.vehicles = vehicles;
        this.remotePlayerManager = remotePlayerManager;
    }

    public override void Process(VehicleUndocking packet)
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
    }

    private void StartVehicleUndocking(VehicleUndocking packet, GameObject vehicleGo, Vehicle vehicle, VehicleDockingBay vehicleDockingBay)
    {
        vehicleDockingBay.subRoot.BroadcastMessage("OnLaunchBayOpening", SendMessageOptions.DontRequireReceiver);
        SkyEnvironmentChanged.Broadcast(vehicleGo, (GameObject)null);

        if (remotePlayerManager.TryFind(packet.PlayerId, out RemotePlayer player))
        {
            // It can happen that the player turns in circles around himself in the vehicle. This stops it.
            player.RigidBody.angularVelocity = Vector3.zero;
            // Set InCinematic to prevent movement packets from clearing vehicle state during undocking
            player.InCinematic = true;
            // Set enterAnimation to false to prevent the enter animation from playing when player_in is set
            // Then set player_in to true so that when FinishVehicleUndocking calls SetVehicle,
            // it won't trigger the enter animation (since player_in is already true)
            vehicle.mainAnimator.SetBool("enterAnimation", false);
            vehicle.mainAnimator.SetBool("player_in", true);
        }
        vehicleDockingBay.StartCoroutine(StartUndockingAnimation(vehicleDockingBay));

        if (vehicle.TryGetComponent(out MovementReplicator vehicleMovementReplicator))
        {
            vehicleMovementReplicator.ClearBuffer();
            Log.Debug($"[{nameof(VehicleUndockingProcessor)}] Clear MovementReplicator on {packet.VehicleId}");
        }
    }

    private static IEnumerator StartUndockingAnimation(VehicleDockingBay vehicleDockingBay)
    {
        yield return Yielders.WaitFor2Seconds;
        vehicleDockingBay.vehicle_docked_param = false;
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
        // We look up the player again (instead of passing from StartVehicleUndocking) because
        // the player could have disconnected between the start and finish packets
        if (remotePlayerManager.TryFind(packet.PlayerId, out RemotePlayer player))
        {
            // Clear InCinematic flag now that undocking is complete
            player.InCinematic = false;
            // Sometimes the player is not set accordingly which stretches the player's model instead of putting them in place
            // after undocking. This fixes it (the player rigid body seems to not be set right sometimes)
            player.SetSubRoot(null);
            // Only call SetVehicle if not already in this vehicle to avoid replaying the enter animation
            if (player.Vehicle != vehicle)
            {
                player.SetVehicle(vehicle);
            }
        }
        vehicles.SetOnPilotMode(packet.VehicleId, packet.PlayerId, true);

        if (vehicle.TryGetComponent(out MovementReplicator vehicleMovementReplicator))
        {
            vehicleMovementReplicator.enabled = true;
            Log.Debug($"[{nameof(VehicleUndockingProcessor)}] Enabled MovementReplicator on {packet.VehicleId}");
        }

        Log.Debug("Set vehicle undocking complete");
    }
}
