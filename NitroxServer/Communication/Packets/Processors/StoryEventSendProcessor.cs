using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.Communication.Packets.Processors
{
    public class StoryEventSendProcessor : AuthenticatedPacketProcessor<StoryEventSend>
    {
        private readonly PlayerManager playerManager;
        private readonly StoryGoalData storyGoalData;
        private readonly ScheduleKeeper scheduleKeeper;

        public StoryEventSendProcessor(PlayerManager playerManager, StoryGoalData storyGoalData, ScheduleKeeper scheduleKeeper)
        {
            this.playerManager = playerManager;
            this.storyGoalData = storyGoalData;
            this.scheduleKeeper = scheduleKeeper;
        }

        public override void Process(StoryEventSend packet, Player player)
        {
            switch (packet.Type)
            {
                case StoryEventSend.EventType.RADIO:
                    if (!storyGoalData.RadioQueue.Contains(packet.Key))
                    {
                        storyGoalData.RadioQueue.Add(packet.Key);
                    }
                    break;
                case StoryEventSend.EventType.GOAL_UNLOCK:
                    if (!storyGoalData.GoalUnlocks.Contains(packet.Key))
                    {
                        storyGoalData.GoalUnlocks.Add(packet.Key);
                        packet = new StoryEventSend(StoryEventSend.EventType.STORY, packet.Key);
                    }
                    break;
                default:
                    if (!storyGoalData.CompletedGoals.Contains(packet.Key))
                    {
                        storyGoalData.CompletedGoals.Add(packet.Key);
                    }
                    break;
            }

            if (scheduleKeeper.ContainsScheduledGoal(packet.Key))
            {
                scheduleKeeper.UnScheduleGoal(packet.Key);
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
