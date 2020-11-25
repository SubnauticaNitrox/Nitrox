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
                vehicleDockingBay.ReflectionSet("vehicle_docked_param", false);
                RemotePlayer playerInstance = player.Value;
                playerInstance.Attach(vehicle.transform);
                vehicle.mainAnimator.SetBool("player_in", true);
                vehicles.SetOnPilotMode(packet.VehicleId, packet.PlayerId, false);
                playerInstance.AnimationController.UpdatePlayerAnimations = false;
            }
        }
        private void FinishVehicleUndocking(VehicleUndocking packet, Vehicle vehicle, VehicleDockingBay vehicleDockingBay)
        {
            if (vehicleDockingBay.GetSubRoot().isCyclops)
            {
                vehicleDockingBay.SetVehicleUndocked();
            }
            vehicleDockingBay.ReflectionSet("_dockedVehicle", null);
            vehicleDockingBay.CancelInvoke("RepairVehicle");
            vehicle.docked = false;
            vehicles.SetOnPilotMode(packet.VehicleId, packet.PlayerId, true);
            Log.Debug($"Set vehicle undocking complete");
        }
    }
}
