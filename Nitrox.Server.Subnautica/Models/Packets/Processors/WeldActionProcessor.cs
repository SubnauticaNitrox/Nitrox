using Nitrox.Model.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class WeldActionProcessor(SimulationOwnershipData simulationOwnershipData, PlayerService playerService, ILogger<WeldActionProcessor> logger) : IAuthPacketProcessor<WeldAction>
{
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly PlayerService playerService = playerService;
    private readonly ILogger<WeldActionProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, WeldAction packet)
    {
        SessionId? simulatingPlayer = simulationOwnershipData.GetSessionIdForLock(packet.Id);
        if (simulatingPlayer.HasValue)
        {
            logger.ZLogDebug($"Send {nameof(WeldAction)} to simulating player {playerService.GetProperty<NameProperty>(simulatingPlayer.Value).Value} for entity {packet.Id}");
            await context.SendAsync(packet, simulatingPlayer.Value);
        }
    }
}
