using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class GoalCompletedProcessor(PlayerService playerService) : IAuthPacketProcessor<GoalCompleted>
{
    private readonly PlayerService playerService = playerService;

    public async Task Process(AuthProcessorContext context, GoalCompleted packet)
    {
        playerService.GetProperty<CompletedGoalsWithTimestampProperty>(context.Sender).Value.Add(packet.CompletedGoal, packet.CompletionTime);
    }
}
