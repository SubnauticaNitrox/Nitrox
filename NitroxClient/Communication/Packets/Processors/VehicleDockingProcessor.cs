using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using System.Collections;
using UnityEngine;

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
            GameObject vehicleGo = GuidHelper.RequireObjectFrom(packet.VehicleGuid);
            GameObject vehicleDockingBayGo = GuidHelper.RequireObjectFrom(packet.DockGuid);

            Vehicle vehicle = vehicleGo.RequireComponent<Vehicle>();
            VehicleDockingBay vehicleDockingBay = vehicleDockingBayGo.RequireComponentInChildren<VehicleDockingBay>();
            
            using (packetSender.Suppress<VehicleDocking>())
            {
                vehicleDockingBay.DockVehicle(vehicle);
            }

            vehicle.StartCoroutine(DisablePilotingAfterAnimation(packet.VehicleGuid, packet.PlayerId));
        }

        IEnumerator DisablePilotingAfterAnimation(string vehicleGuid, ushort playerId)
        {
            yield return new WaitForSeconds(3.0f);
            vehicles.SetOnPilotMode(vehicleGuid, playerId, false);
        }
    }
}
