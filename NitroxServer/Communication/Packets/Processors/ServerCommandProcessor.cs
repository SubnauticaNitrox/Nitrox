using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.ConsoleCommands.Processor;

namespace NitroxServer.Communication.Packets.Processors
{
    public class ServerCommandProcessor : AuthenticatedPacketProcessor<ServerCommand>
    {
        private readonly ConsoleCommandProcessor cmdProcessor;

        public ServerCommandProcessor(ConsoleCommandProcessor cmdProcessor)
        {
            this.cmdProcessor = cmdProcessor;
        }

        public override void Process(ServerCommand packet, Player player)
        {
            string msg = string.Join(" ", packet.CmdArgs);

            cmdProcessor.ProcessCommand(msg);
        }
    }
}
