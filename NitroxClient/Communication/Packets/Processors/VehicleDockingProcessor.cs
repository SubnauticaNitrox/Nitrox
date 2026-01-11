using System.Collections;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Vehicles;
using NitroxClient.Unity.Helper;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class VehicleDockingProcessor : ClientPacketProcessor<VehicleDocking>
{
    private readonly Vehicles vehicles;
    private readonly PlayerManager playerManager;

    public VehicleDockingProcessor(Vehicles vehicles, PlayerManager playerManager)
    {
        this.vehicles = vehicles;
        this.playerManager = playerManager;
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

        // Set InCinematic on the remote player to prevent movement packets from interfering during docking
        if (playerManager.TryFind(packet.PlayerId, out RemotePlayer player))
        {
            player.InCinematic = true;
        }

        vehicle.StartCoroutine(InterpolateAndDockVehicle(vehicle, vehicleMovementReplicator, dockingBay, packet.VehicleId, packet.PlayerId));
    }

    private IEnumerator InterpolateAndDockVehicle(Vehicle vehicle, VehicleMovementReplicator vehicleMovementReplicator, VehicleDockingBay dockingBay, NitroxId vehicleId, ushort playerId)
    {
        // Get the target docking position based on vehicle type (same logic as VehicleDockingBay.UpdateDockedPosition)
        Transform dockingEndPos = vehicle is Exosuit ? dockingBay.dockingEndPosExo : dockingBay.dockingEndPos;

        // Store starting position for interpolation
        Vector3 startPosition = vehicle.transform.position;
        Quaternion startRotation = vehicle.transform.rotation;

        // Use the same interpolation time as the game (default is 1 second)
        float interpolationTime = dockingBay.interpolationTime;
        float elapsedTime = 0f;

        // Interpolate vehicle position to docking bay (replicates VehicleDockingBay.LateUpdate behavior)
        while (elapsedTime < interpolationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / interpolationTime);

            vehicle.transform.position = Vector3.Lerp(startPosition, dockingEndPos.position, t);
            vehicle.transform.rotation = Quaternion.Lerp(startRotation, dockingEndPos.rotation, t);

            yield return null;
        }

        // Ensure final position is exact
        vehicle.transform.position = dockingEndPos.position;
        vehicle.transform.rotation = dockingEndPos.rotation;

        // Now dock the vehicle
        DockRemoteVehicle(dockingBay, vehicle);
        vehicle.useRigidbody.isKinematic = false;

        // Wait for the docking cinematic to complete before disabling pilot mode
        yield return Yielders.WaitFor2Seconds;

        // Clear the remote player's vehicle state since they're now exiting
        if (playerManager.TryFind(playerId, out RemotePlayer player))
        {
            player.InCinematic = false;
            player.SetVehicle(null);
        }

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
        SkyEnvironmentChanged.Broadcast(vehicle.gameObject, bay.subRoot);
        bay.GetSubRoot().BroadcastMessage("UnlockDoors", SendMessageOptions.DontRequireReceiver);

        // We are only actually adding the health if we have a lock on the vehicle so we're fine to keep this routine going on.
        // If vehicle ownership changes then it'll still be fine because the verification will still be on the vehicle ownership.
        bay.CancelInvoke(nameof(VehicleDockingBay.RepairVehicle));
        bay.InvokeRepeating(nameof(VehicleDockingBay.RepairVehicle), 0.0f, 5f);
    }
}
