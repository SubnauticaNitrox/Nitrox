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
    public class VehicleOnPilotModeProcessor : ClientPacketProcessor<VehicleOnPilotMode>
    {
        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;

        public VehicleOnPilotModeProcessor(IPacketSender packetSender, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
        }

        public override void Process(VehicleOnPilotMode packet)
        {
            using (packetSender.Suppress<VehicleRemoveEntry>())
            {
                vehicles.OnPilotModeSet(packet.VehicleGuid, packet.PlayerGuid,  packet.Type);
            }
        }
    }
}
