using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Model.Packets;
using Story;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class RadioPlayPendingMessageProcessor : ClientPacketProcessor<RadioPlayPendingMessage>
    {
        public override void Process(RadioPlayPendingMessage packet)
        {
            StoryGoalManager.main.ExecutePendingRadioMessage();
        }
    }
}
