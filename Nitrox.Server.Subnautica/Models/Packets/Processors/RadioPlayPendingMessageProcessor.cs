using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class RadioPlayPendingMessageProcessor(StoryManager storyManager, IPacketSender packetSender, ILogger<RadioPlayPendingMessageProcessor> logger) : IAuthPacketProcessor<RadioPlayPendingMessage>
{
    private readonly StoryManager storyManager = storyManager;
    private readonly IPacketSender packetSender = packetSender;
    private readonly ILogger<RadioPlayPendingMessageProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, RadioPlayPendingMessage packet)
    {
        if (!storyManager.RemovedLatestRadioMessage())
        {
            logger.ZLogWarning($"Tried to remove the latest radio message but the radio queue is empty: {packet}");
            return;
        }
        await context.SendToOthersAsync(packet);
    }
}
