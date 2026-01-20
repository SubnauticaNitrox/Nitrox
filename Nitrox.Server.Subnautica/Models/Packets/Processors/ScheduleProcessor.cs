using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ScheduleProcessor(IPacketSender packetSender, StoryScheduler storyScheduler) : AuthenticatedPacketProcessor<Schedule>
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly StoryScheduler storyScheduler = storyScheduler;

    public override void Process(Schedule packet, Player player)
    {
        storyScheduler.ScheduleStory(NitroxScheduledGoal.From(packet.TimeExecute, packet.Key, packet.Type));
        packetSender.SendPacketToOthersAsync(packet, player.SessionId);
    }
}
