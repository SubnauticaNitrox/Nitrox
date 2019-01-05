using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.ConsoleCommands.Processor;
using NitroxServer.GameLogic.Players;
using NitroxModel.DataStructures.GameLogic;

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

            if (playerData.Permissions(player.Name) >= Perms.Admin)
            {
                cmdProcessor.ProcessCommand(msg);
            }
            else
            {
                player.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, "You do not have permission for this command!"));
            }
        }
    }
}
