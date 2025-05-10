using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class StoryGoalExecutedProcessor(StoryScheduleService storyScheduleService, ILogger<StoryGoalExecutedProcessor> logger) : IAuthPacketProcessor<StoryGoalExecuted>
{
    // TODO: USE DATABASE
    // private readonly StoryGoalData storyGoalData = storyGoalData;
    // private readonly PdaStateData pdaStateData = pdaStateData;
    private readonly StoryScheduleService storyScheduleService = storyScheduleService;
    private readonly ILogger<StoryGoalExecutedProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, StoryGoalExecuted packet)
    {
        logger.ZLogDebug($"Processing {packet}");
        // TODO: USE DATABASE
        // The switch is structure is similar to StoryGoal.Execute()
        // bool added = storyGoalData.CompletedGoals.Add(packet.Key);
        // switch (packet.Type)
        // {
        //     case StoryGoalExecuted.EventType.RADIO:
        //         if (added)
        //         {
        //             storyGoalData.RadioQueue.Enqueue(packet.Key);
        //         }
        //         break;
        //     case StoryGoalExecuted.EventType.PDA:
        //         if (packet.Timestamp.HasValue)
        //         {
        //             pdaStateData.AddPdaLogEntry(new(packet.Key, packet.Timestamp.Value));
        //         }
        //         break;
        // }

        storyScheduleService.UnScheduleGoal(packet.Key);
        await context.ReplyToOthers(packet);
    }
}
