using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class RadioPlayPendingMessageProcessor : IAuthPacketProcessor<RadioPlayPendingMessage>
{
    // TODO: USE DATABASE
    // private readonly StoryGoalData storyGoalData = storyGoalData;

    public async Task Process(AuthProcessorContext context, RadioPlayPendingMessage packet)
    {
        // TODO: USE DATABASE
        // if (!storyGoalData.RemovedLatestRadioMessage())
        // {
        //     Log.Warn($"Tried to remove the latest radio message but the radio queue is empty: {packet}");
        // }
        await context.ReplyToOthers(packet);
    }
}
