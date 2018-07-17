using System;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class VehicleAddProcessor : ClientPacketProcessor<VehicleCreated>
    {
        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;

        public VehicleAddProcessor(IPacketSender packetSender, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
        }

        public override void Process(VehicleCreated packet)
        {
            using (packetSender.Suppress<VehicleCreated>())
            {
                vehicles.AddVehiclePosition(packet.Vehicle);
            }
        }
    }
}
