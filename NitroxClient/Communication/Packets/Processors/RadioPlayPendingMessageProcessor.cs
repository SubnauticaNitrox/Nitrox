using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using Story;

namespace NitroxClient.Communication.Packets.Processors
{
    public class RadioPlayPendingMessageProcessor : ClientPacketProcessor<RadioPlayPendingMessage>
    {
        public override void Process(RadioPlayPendingMessage packet)
        {
            StoryGoalManager.main.ExecutePendingRadioMessage();
        }
    }
}
