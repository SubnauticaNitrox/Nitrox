using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    sealed class ChatMessageProcessor : AuthenticatedPacketProcessor<ChatMessage>
    {
        private readonly PlayerManager playerManager;
        private readonly ILogger<ChatMessageProcessor> logger;

        public ChatMessageProcessor(PlayerManager playerManager, ILogger<ChatMessageProcessor> logger)
        {
            this.playerManager = playerManager;
            this.logger = logger;
        }

        public override void Process(ChatMessage packet, Player player)
        {
            if (player.PlayerContext.IsMuted)
            {
                player.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, "You're currently muted"));
                return;
            }
            logger.ZLogInformation($"<{player.Name}>: {packet.Text}");
            playerManager.SendPacketToAllPlayers(packet);
        }
    }
}
