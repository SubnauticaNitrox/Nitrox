using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
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
            if (player.PlayerContext.IsMuted)
            {
                player.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, "You're currently muted"));
                return;
            }
            Log.Info($"<{player.Name}>: {packet.Text}");
            playerManager.SendPacketToAllPlayers(packet);
        }
    }
}
