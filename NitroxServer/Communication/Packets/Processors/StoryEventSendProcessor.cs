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
            if (packet.StoryEventType == StoryEventType.Radio)
            {
                storyGoalData.AddRadioMessage(packet.Key);
            }
            if (packet.StoryEventType == StoryEventType.GoalUnlock)
            {
                storyGoalData.AddGoalUnlock(packet.Key);
            }
            else
            {
                storyGoalData.AddStoryGoal(packet.Key);
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
