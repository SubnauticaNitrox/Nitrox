using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class RadioPlayPendingMessageProcessor(StoryManager storyManager, IPacketSender packetSender, ILogger<RadioPlayPendingMessageProcessor> logger) : AuthenticatedPacketProcessor<RadioPlayPendingMessage>
{
    private readonly StoryManager storyManager = storyManager;
    private readonly IPacketSender packetSender = packetSender;
    private readonly ILogger<RadioPlayPendingMessageProcessor> logger = logger;

    public override void Process(RadioPlayPendingMessage packet, Player player)
    {
        if (!storyManager.RemovedLatestRadioMessage())
        {
            logger.ZLogWarning($"Tried to remove the latest radio message but the radio queue is empty: {packet}");
            return;
        }
        packetSender.SendPacketToOthersAsync(packet, player.SessionId);
    }
}
