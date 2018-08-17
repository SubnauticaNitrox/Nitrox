using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;

namespace NitroxServer.Communication.Packets.Processors
{
    public class ServerCommandProcessor : AuthenticatedPacketProcessor<ServerCommand>
    {
        public override void Process(ServerCommand packet, Player player)
        {
            Server.Instance.Save();
        }
    }
}
