using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class WeldActionProcessor(SimulationOwnershipData simulationOwnershipData, ILogger<WeldActionProcessor> logger) : IAuthPacketProcessor<WeldAction>
{
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly ILogger<WeldActionProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, WeldAction packet)
    {
        Player? simulatingPlayer = simulationOwnershipData.GetPlayerForLock(packet.Id);
        if (simulatingPlayer != null)
        {
            logger.ZLogDebug($"Send WeldAction to simulating player {simulatingPlayer.Name} for entity {packet.Id}");
            await context.SendAsync(packet, simulatingPlayer.SessionId);
        }
    }
}
