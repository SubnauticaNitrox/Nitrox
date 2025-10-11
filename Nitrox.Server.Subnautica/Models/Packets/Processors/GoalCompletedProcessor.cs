using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class GoalCompletedProcessor : AuthenticatedPacketProcessor<GoalCompleted>
{
    public override void Process(GoalCompleted packet, Player player)
    {
        player.PersonalCompletedGoalsWithTimestamp.Add(packet.CompletedGoal, packet.CompletionTime);
    }
}
