using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
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
