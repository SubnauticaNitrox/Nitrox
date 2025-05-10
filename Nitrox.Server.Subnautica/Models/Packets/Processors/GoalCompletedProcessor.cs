using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class GoalCompletedProcessor : IAuthPacketProcessor<GoalCompleted>
{
    public async Task Process(AuthProcessorContext context, GoalCompleted packet)
    {
        // TODO: USE DATABASE
        // player.PersonalCompletedGoalsWithTimestamp.Add(packet.CompletedGoal, packet.CompletionTime);
    }
}
