using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class GoalCompletedProcessor : AuthenticatedPacketProcessor<GoalCompleted>
{
    public override void Process(GoalCompleted packet, Player player)
    {
        player.PersonalCompletedGoalsWithTimestamp.Add(packet.CompletedGoal, packet.CompletionTime);
    }
}
