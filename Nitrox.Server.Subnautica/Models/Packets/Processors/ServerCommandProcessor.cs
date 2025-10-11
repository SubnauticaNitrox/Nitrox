using Nitrox.Model.DataStructures.Util;
using Nitrox.Server.Subnautica.Models.Commands.Processor;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
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
            Log.Info($"{player.Name} issued command: /{packet.Cmd}");
            cmdProcessor.ProcessCommand(packet.Cmd, Optional.Of(player), player.Permissions);
        }
    }
}
