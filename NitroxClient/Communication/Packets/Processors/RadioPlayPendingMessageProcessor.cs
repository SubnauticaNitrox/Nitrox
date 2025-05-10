using NitroxModel.Networking.Packets;
using Story;

namespace NitroxClient.Communication.Packets.Processors
{
    public class RadioPlayPendingMessageProcessor : IClientPacketProcessor<RadioPlayPendingMessage>
    {
        public Task Process(IPacketProcessContext context, RadioPlayPendingMessage packet)
        {
            StoryGoalManager.main.ExecutePendingRadioMessage();

            return Task.CompletedTask;
        }
    }
}
