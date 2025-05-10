using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PdaLogEntryAddProcessor(StoryScheduleService storyScheduleService) : IAuthPacketProcessor<PdaLogEntryAdd>
{
    // TODO: USE DATABASE
    // private readonly IStateManager<PdaStateData> pda = pda;
    private readonly StoryScheduleService storyScheduleService = storyScheduleService;

    public async Task Process(AuthProcessorContext context, PdaLogEntryAdd packet)
    {
        // TODO: USE DATABASE
        // pda.GetStateAsync().GetAwaiter().GetResult().AddPdaLogEntry(new PdaLogEntry(packet.Key, packet.Timestamp));
        if (storyScheduleService.ContainsScheduledGoal(packet.Key))
        {
            storyScheduleService.UnScheduleGoal(packet.Key);
        }
        await context.ReplyToOthers(packet);
    }
}
