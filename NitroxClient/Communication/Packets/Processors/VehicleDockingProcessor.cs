using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using System.Collections;
using UnityEngine;
using NitroxModel.DataStructures;
using NitroxClient.MonoBehaviours;
using NitroxModel.Logger;
using NitroxModel.Helper;
using NitroxModel.DataStructures.Util;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleDockingProcessor : ClientPacketProcessor<VehicleDocking>
    {
        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;
        private readonly SimulationOwnership simulationOwnership;
        private readonly PlayerManager remotePlayerManager;

        public VehicleDockingProcessor(IPacketSender packetSender, Vehicles vehicles, SimulationOwnership simulationOwnership, PlayerManager remotePlayerManager)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
            this.simulationOwnership = simulationOwnership;
            this.remotePlayerManager = remotePlayerManager;
        }

        public override void Process(VehicleDocking packet)
        {
            GameObject vehicleGo = NitroxEntity.RequireObjectFrom(packet.VehicleId);
            GameObject vehicleDockingBayGo = NitroxEntity.RequireObjectFrom(packet.DockId);

            Vehicle vehicle = vehicleGo.RequireComponent<Vehicle>();
            VehicleDockingBay vehicleDockingBay = vehicleDockingBayGo.RequireComponentInChildren<VehicleDockingBay>();

            using (packetSender.Suppress<VehicleDocking>())
            {
                Log.Debug($"Set vehicle docked for {vehicleDockingBay.gameObject.name}");
                vehicle.GetComponent<MultiplayerVehicleControl>().Exit();
                Log.Debug($"before dock isKinematic: {vehicle.useRigidbody.isKinematic}");
                vehicleDockingBay.DockVehicle(vehicle);
                vehicle.useRigidbody.isKinematic = false;
                Log.Debug($"after dock isKinematic: {vehicle.useRigidbody.isKinematic}");
            }

            vehicle.StartCoroutine(DisablePilotingAfterAnimation(packet.VehicleId, packet.PlayerId));
        }        

        IEnumerator DisablePilotingAfterAnimation(NitroxId vehicleId, ushort playerId)
        {
            yield return new WaitForSeconds(3.0f);
            vehicles.SetOnPilotMode(vehicleId, playerId, false);
            GameObject vehicleGo = NitroxEntity.RequireObjectFrom(vehicleId);
            Vehicle vehicle = vehicleGo.RequireComponent<Vehicle>();
            if (!vehicle.docked)
            {
                Log.Error($"Vehicle not docked after docking process");
            }
        }
    }
}
