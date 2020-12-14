using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
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
            Log.Info($"{player.Name} issued command: /{packet.Command}");
            cmdProcessor.ProcessCommand(packet.Command, Optional.Of(player), player.Permissions);
        }
    }
}
