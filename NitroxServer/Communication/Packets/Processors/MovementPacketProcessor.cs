using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using System;

namespace NitroxServer.Communication.Packets.Processors
{
    class MovementPacketProcessor : AuthenticatedPacketProcessor<Movement>
    {
        public override void Process(Movement packet, Player player)
        {
             player.Position = packet.PlayerPosition;
        }
    }
}
