using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using System.Collections;
using UnityEngine;
using NitroxModel.DataStructures;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleDockingProcessor : ClientPacketProcessor<VehicleDocking>
    {
        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;

        public VehicleDockingProcessor(IPacketSender packetSender, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
        }

        public override void Process(VehicleDocking packet)
        {
            GameObject vehicleGo = NitroxEntity.RequireObjectFrom(packet.VehicleId);
            GameObject vehicleDockingBayGo = NitroxEntity.RequireObjectFrom(packet.DockId);

            Vehicle vehicle = vehicleGo.RequireComponent<Vehicle>();
            VehicleDockingBay vehicleDockingBay = vehicleDockingBayGo.RequireComponentInChildren<VehicleDockingBay>();
            
            using (packetSender.Suppress<VehicleDocking>())
            {
                vehicleDockingBay.SetVehicleDocked(vehicle);
            }

            vehicle.StartCoroutine(DisablePilotingAfterAnimation(packet.VehicleId, packet.PlayerId));
        }        

        IEnumerator DisablePilotingAfterAnimation(NitroxId vehicleId, ushort playerId)
        {
            yield return new WaitForSeconds(3.0f);
            vehicles.SetOnPilotMode(vehicleId, playerId, false);
        }
    }
}
