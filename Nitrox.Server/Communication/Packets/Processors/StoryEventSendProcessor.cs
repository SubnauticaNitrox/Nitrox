using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Unlockables;

namespace Nitrox.Server.Communication.Packets.Processors
{
    public class StoryEventSendProcessor : AuthenticatedPacketProcessor<StoryEventSend>
    {
        private readonly PlayerManager playerManager;
        private readonly StoryGoalData storyGoalData;

        public StoryEventSendProcessor(PlayerManager playerManager, StoryGoalData storyGoalData)
        {
            this.playerManager = playerManager;
            this.storyGoalData = storyGoalData;
        }

        public override void Process(StoryEventSend packet, Player player)
        {
            switch (packet.StoryEventType)
            {
                case StoryEventType.RADIO:
                    if (!storyGoalData.RadioQueue.Contains(packet.Key))
                    {
                        storyGoalData.RadioQueue.Add(packet.Key);
                    }
                    break;
                case StoryEventType.GOAL_UNLOCK:
                    if (!storyGoalData.GoalUnlocks.Contains(packet.Key))
                    {
                        storyGoalData.GoalUnlocks.Add(packet.Key);
                        packet = new StoryEventSend(StoryEventType.STORY, packet.Key);
                    }
                    break;
                default:
                    if (!storyGoalData.CompletedGoals.Contains(packet.Key))
                    {
                        storyGoalData.CompletedGoals.Add(packet.Key);
                    }
                    break;
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
