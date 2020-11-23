using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using NitroxModel.Packets;
using System.Collections;
using UnityEngine;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;

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
            VehicleDockingBay vehicleDockingBay = vehicleDockingBayGo.RequireComponentInChildren<VehicleDockingBay>();
            
            using (packetSender.Suppress<VehicleUndocking>())
            {
                Optional<RemotePlayer> player = remotePlayerManager.Find(packet.PlayerId);
                if (packet.UndockingStart)
                {
                    vehicleDockingBay.subRoot.BroadcastMessage("OnLaunchBayOpening", SendMessageOptions.DontRequireReceiver);
                    SkyEnvironmentChanged.Broadcast(vehicleGo, (GameObject)null);
                    if (player.HasValue)
                    {
                        RemotePlayer playerInstance = player.Value;
                        playerInstance.Attach(vehicle.transform);
                        vehicle.mainAnimator.SetBool("player_in", true);
                        vehicles.SetOnPilotMode(packet.VehicleId, packet.PlayerId, false);
                        playerInstance.AnimationController.UpdatePlayerAnimations = false;
                    }
                }
                else
                {
                    vehicleDockingBay.SetVehicleUndocked();
                    vehicleDockingBay.ReflectionSet("vehicle_docked_param", false);
                    vehicleDockingBay.ReflectionSet("_dockedVehicle", null);
                    vehicle.docked = false;
                    //vehicle.useRigidbody.AddForce(Vector3.down * 5f, ForceMode.VelocityChange);
                    if (player.HasValue)
                    {
                        RemotePlayer playerInstance = player.Value;                     
                        vehicles.SetOnPilotMode(packet.VehicleId, packet.PlayerId, true);
                    }
                    Log.Debug($"Set vehicle undocking complete");
                }
            }
        }
    }
}
