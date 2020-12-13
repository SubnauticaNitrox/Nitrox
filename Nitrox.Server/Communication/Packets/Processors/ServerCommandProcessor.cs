using Nitrox.Model.DataStructures;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.ConsoleCommands.Processor;

namespace Nitrox.Server.Communication.Packets.Processors
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
