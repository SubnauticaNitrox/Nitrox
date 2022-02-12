using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.Communication.Packets.Processors
{
    public class RadioPlayPendingMessageProcessor : AuthenticatedPacketProcessor<RadioPlayPendingMessage>
    {
        private readonly StoryGoalData storyGoalData;
        private readonly PlayerManager playerManager;

        public RadioPlayPendingMessageProcessor(StoryGoalData storyGoalData, PlayerManager playerManager)
        {
            this.storyGoalData = storyGoalData;
            this.playerManager = playerManager;
        }

        public override void Process(RadioPlayPendingMessage packet, Player player)
        {
            if (!storyGoalData.RemovedLatestRadioMessage())
            {
                Log.Warn($"Tried to remove the latest radio message but the radio queue is empty: {packet}");
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
