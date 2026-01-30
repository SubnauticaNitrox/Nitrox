using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ScheduleProcessor(IPacketSender packetSender, StoryScheduler storyScheduler) : IAuthPacketProcessor<Schedule>
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly StoryScheduler storyScheduler = storyScheduler;

    public async Task Process(AuthProcessorContext context, Schedule packet)
    {
        storyScheduler.ScheduleStory(NitroxScheduledGoal.From(packet.TimeExecute, packet.Key, packet.Type));
        await context.SendToOthersAsync(packet);
    }
}
