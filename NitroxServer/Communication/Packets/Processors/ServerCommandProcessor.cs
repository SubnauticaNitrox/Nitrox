using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.ConsoleCommands.Processor;
using NitroxServer.GameLogic.Players;

namespace NitroxServer.Communication.Packets.Processors
{
    public class ServerCommandProcessor : AuthenticatedPacketProcessor<ServerCommand>
    {
        private readonly ConsoleCommandProcessor cmdProcessor;
        private readonly PlayerData playerData;

        public ServerCommandProcessor(ConsoleCommandProcessor cmdProcessor, PlayerData playerData)
        {
            this.cmdProcessor = cmdProcessor;
            this.playerData = playerData;
        }

        public override void Process(ServerCommand packet, Player player)
        {
            string msg = string.Join(" ", packet.CmdArgs);
            
            cmdProcessor.ProcessCommand(msg, Optional<Player>.Of(player), playerData.GetPermissions(player.Name));
        }
    }
}
