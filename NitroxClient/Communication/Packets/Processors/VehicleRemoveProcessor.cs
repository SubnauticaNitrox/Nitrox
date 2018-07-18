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
    public class VehicleRemoveProcessor : ClientPacketProcessor<VehicleDestroyed>
    {
        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;
        private readonly PlayerManager remotePlayerManager;

        public VehicleRemoveProcessor(IPacketSender packetSender, Vehicles vehicles, PlayerManager remotePlayerManager)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
            this.remotePlayerManager = remotePlayerManager;
        }

        public override void Process(VehicleDestroyed packet)
        {
            using (packetSender.Suppress<VehicleDestroyed>())
            {
                vehicles.DestroyVehicle(packet, remotePlayerManager);
            }
        }
    }
}
