using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    internal sealed class RadioPlayPendingMessageProcessor : AuthenticatedPacketProcessor<RadioPlayPendingMessage>
    {
        private readonly StoryManager storyManager;
        private readonly PlayerManager playerManager;
        private readonly ILogger<RadioPlayPendingMessageProcessor> logger;

        public RadioPlayPendingMessageProcessor(StoryManager storyManager, PlayerManager playerManager, ILogger<RadioPlayPendingMessageProcessor> logger)
        {
            this.storyManager = storyManager;
            this.playerManager = playerManager;
            this.logger = logger;
        }

        public override void Process(RadioPlayPendingMessage packet, Player player)
        {
            if (!storyManager.RemovedLatestRadioMessage())
            {
                logger.ZLogWarning($"Tried to remove the latest radio message but the radio queue is empty: {packet}");
                return;
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
