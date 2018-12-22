using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.ConsoleCommands.Processor;

namespace NitroxServer.Communication.Packets.Processors
{
    public class ServerCommandProcessor : AuthenticatedPacketProcessor<ServerCommand>
    {
        public override void Process(ServerCommand packet, Player player)
        {
            string msg = string.Join(" ", packet.CmdArgs);

            ConsoleCommandProcessor.ProcessCommand(msg);
        }
    }
}
