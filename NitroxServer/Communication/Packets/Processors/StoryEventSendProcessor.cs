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
                    storyGoalData.RadioQueue.Add(packet.Key);
                    break;
                case StoryEventType.GOAL_UNLOCK:
                    storyGoalData.GoalUnlocks.Add(packet.Key);
                    break;
                default:
                    storyGoalData.CompletedGoals.Add(packet.Key);
                    break;
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
