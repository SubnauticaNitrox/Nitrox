using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PDALogEntryAddProcessor(PdaManager pdaManager, StoryScheduler storyScheduler) : IAuthPacketProcessor<PDALogEntryAdd>
{
    private readonly PdaManager pdaManager = pdaManager;
    private readonly StoryScheduler storyScheduler = storyScheduler;

    public async Task Process(AuthProcessorContext context, PDALogEntryAdd packet)
    {
        pdaManager.AddPDALogEntry(new PDALogEntry(packet.Key, packet.Timestamp));
        if (storyScheduler.ContainsScheduledStory(packet.Key))
        {
            storyScheduler.UnscheduleStory(packet.Key);
        }
        await context.SendToOthersAsync(packet);
    }
}
