using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    internal sealed class PDALogEntryAddProcessor : AuthenticatedPacketProcessor<PDALogEntryAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly PdaManager pdaManager;
        private readonly StoryScheduler storyScheduler;

        public PDALogEntryAddProcessor(PlayerManager playerManager, PdaManager pdaManager, StoryScheduler storyScheduler)
        {
            this.playerManager = playerManager;
            this.pdaManager = pdaManager;
            this.storyScheduler = storyScheduler;
        }

        public override void Process(PDALogEntryAdd packet, Player player)
        {
            pdaManager.AddPDALogEntry(new PDALogEntry(packet.Key, packet.Timestamp));
            if (storyScheduler.ContainsScheduledStory(packet.Key))
            {
                storyScheduler.UnscheduleStory(packet.Key);
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
