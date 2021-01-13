using System;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Logger;
using NitroxModel.Packets;
using Story;

namespace NitroxClient.Communication.Packets.Processors
{
    public class RadioPlayPendingMessageProcessor : ClientPacketProcessor<RadioPlayPendingMessage>
    {
        public override void Process(RadioPlayPendingMessage packet)
        {
            if (StoryGoalManager.main.pendingRadioMessages.Count != 0) // Check if the pendingRadioMessages array isn't empty to prevent errors.
            {
                StoryGoalManager.main.ExecutePendingRadioMessage();
                return;
            }
            Log.InGame("The radio message didn't play as there was a syncing issue.");
        }
    }
}
