using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class ScheduleProcessor(StoryScheduleService storyScheduleService) : IAuthPacketProcessor<Schedule>
{
    private readonly StoryScheduleService storyScheduleService = storyScheduleService;

    public async Task Process(AuthProcessorContext context, Schedule packet)
    {
        storyScheduleService.ScheduleGoal(NitroxScheduledGoal.From(packet.TimeExecute, packet.Key, packet.Category));
        await context.ReplyToOthers(packet);
    }
}
