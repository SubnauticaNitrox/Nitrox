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
    public class VehicleOnPilotModeProcessor : ClientPacketProcessor<VehicleOnPilotModeChanged>
    {
        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;

        public VehicleOnPilotModeProcessor(IPacketSender packetSender, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
        }

        public override void Process(VehicleOnPilotModeChanged packet)
        {
            vehicles.OnPilotModeSet(packet.VehicleGuid, packet.PlayerGuid, packet.Type);
        }
    }
}
