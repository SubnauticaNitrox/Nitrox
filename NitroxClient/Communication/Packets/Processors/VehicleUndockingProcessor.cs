using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleUndockingProcessor : ClientPacketProcessor<VehicleUndocking>
    {
        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;
        private readonly PlayerManager remotePlayerManager;

        public VehicleUndockingProcessor(IPacketSender packetSender, Vehicles vehicles, PlayerManager playerManager)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
            remotePlayerManager = playerManager;
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
            Optional<RemotePlayer> player = remotePlayerManager.Find(packet.PlayerId);
            vehicleDockingBay.subRoot.BroadcastMessage("OnLaunchBayOpening", SendMessageOptions.DontRequireReceiver);
            SkyEnvironmentChanged.Broadcast(vehicleGo, (GameObject)null);

            if (player.HasValue)
            {
                RemotePlayer playerInstance = player.Value;
                vehicle.mainAnimator.SetBool("player_in", true);
                playerInstance.Attach(vehicle.playerPosition.transform);
                // It can happen that the player turns in circles around himself in the vehicle. This stops it.
                playerInstance.RigidBody.angularVelocity = Vector3.zero;
                playerInstance.ArmsController.SetWorldIKTarget(vehicle.leftHandPlug, vehicle.rightHandPlug);
                playerInstance.AnimationController["in_seamoth"] = vehicle is SeaMoth;
                playerInstance.AnimationController["in_exosuit"] = playerInstance.AnimationController["using_mechsuit"] = vehicle is Exosuit;
                vehicles.SetOnPilotMode(packet.VehicleId, packet.PlayerId, true);
                playerInstance.AnimationController.UpdatePlayerAnimations = false;
            }
            vehicleDockingBay.StartCoroutine(StartUndockingAnimation(vehicleDockingBay));
        }

        public IEnumerator StartUndockingAnimation(VehicleDockingBay vehicleDockingBay)
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
            vehicleDockingBay.CancelInvoke("RepairVehicle");
            vehicle.docked = false;
            Optional<RemotePlayer> player = remotePlayerManager.Find(packet.PlayerId);
            if (player.HasValue)
            {
                // Sometimes the player is not set accordingly which stretches the player's model instead of putting them in place
                // after undocking. This fixes it (the player rigid body seems to not be set right sometimes)
                player.Value.SetSubRoot(null);
                player.Value.SetVehicle(null);
                player.Value.SetVehicle(vehicle);
            }
            vehicles.SetOnPilotMode(packet.VehicleId, packet.PlayerId, true);
            Log.Debug($"Set vehicle undocking complete");
        }
    }
}
