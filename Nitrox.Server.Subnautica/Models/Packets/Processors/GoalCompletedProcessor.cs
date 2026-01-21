using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class GoalCompletedProcessor : IAuthPacketProcessor<GoalCompleted>
{
    public async Task Process(AuthProcessorContext context, GoalCompleted packet)
    {
        context.Sender.PersonalCompletedGoalsWithTimestamp.Add(packet.CompletedGoal, packet.CompletionTime);
    }
}
