using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    internal sealed class ScheduleProcessor : AuthenticatedPacketProcessor<Schedule>
    {
        private readonly PlayerManager playerManager;
        private readonly StoryScheduler storyScheduler;

        public ScheduleProcessor(PlayerManager playerManager, StoryScheduler storyScheduler)
        {
            this.playerManager = playerManager;
            this.storyScheduler = storyScheduler;
        }

        public override void Process(Schedule packet, Player player)
        {
            storyScheduler.ScheduleStory(NitroxScheduledGoal.From(packet.TimeExecute, packet.Key, packet.Type));
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
