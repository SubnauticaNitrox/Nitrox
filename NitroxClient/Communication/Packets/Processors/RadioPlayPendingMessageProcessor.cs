using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using Story;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class RadioPlayPendingMessageProcessor : IClientPacketProcessor<RadioPlayPendingMessage>
{
    public Task Process(ClientProcessorContext context, RadioPlayPendingMessage packet)
    {
        StoryGoalManager.main.ExecutePendingRadioMessage();
        return Task.CompletedTask;
    }
}