using System.Collections;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
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
            vehicle.mainAnimator.SetBool("player_in", true);
            player.Attach(vehicle.playerPosition.transform);
            // It can happen that the player turns in circles around himself in the vehicle. This stops it.
            player.RigidBody.angularVelocity = Vector3.zero;
            player.ArmsController.SetWorldIKTarget(vehicle.leftHandPlug, vehicle.rightHandPlug);
            player.AnimationController["in_seamoth"] = vehicle is SeaMoth;
            player.AnimationController["in_exosuit"] = player.AnimationController["using_mechsuit"] = vehicle is Exosuit;
            vehicles.SetOnPilotMode(packet.VehicleId, packet.PlayerId, true);
            player.AnimationController.UpdatePlayerAnimations = false;
        }
        vehicleDockingBay.StartCoroutine(StartUndockingAnimation(vehicleDockingBay));
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
        if (remotePlayerManager.TryFind(packet.PlayerId, out RemotePlayer player))
        {
            // Sometimes the player is not set accordingly which stretches the player's model instead of putting them in place
            // after undocking. This fixes it (the player rigid body seems to not be set right sometimes)
            player.SetSubRoot(null);
            player.SetVehicle(null);
            player.SetVehicle(vehicle);
        }
        vehicles.SetOnPilotMode(packet.VehicleId, packet.PlayerId, true);
        Log.Debug("Set vehicle undocking complete");
    }
}
