using Nitrox.Model.Logger;
using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;

namespace Nitrox.Server.Communication.Packets.Processors
{
    public class ChatMessageProcessor : AuthenticatedPacketProcessor<ChatMessage>
    {
        private readonly PlayerManager playerManager;

        public ChatMessageProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(ChatMessage packet, Player player)
        {
            Log.Info(string.Format("<{0}>: {1}", player.Name, packet.Text));
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
