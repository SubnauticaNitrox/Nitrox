using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal abstract class BuildingProcessor<T>(BuildingManager buildingManager, EntitySimulation? entitySimulation = null) : IAuthPacketProcessor<T>
    where T : Packet
{
    protected readonly BuildingManager BuildingManager = buildingManager;
    private readonly EntitySimulation? entitySimulation = entitySimulation;

    public abstract Task Process(AuthProcessorContext context, T packet);

    protected async Task SendToOtherPlayersWithOperationIdAsync(AuthProcessorContext context, T packet, int operationId)
    {
        if (packet is OrderedBuildPacket buildPacket)
        {
            buildPacket.OperationId = operationId;
        }
        await context.SendToOthersAsync(packet);
    }

    /// <summary>
    ///     Attempts to acquire simulation ownership on <paramref name="entity" /> for <paramref name="player" />.
    ///     If the attempt is successful, it will be notified to all players.
    ///     Otherwise, nothing will happen.
    /// </summary>
    protected async Task TryClaimBuildPieceAsync(AuthProcessorContext context, Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entitySimulation);
        if (entitySimulation.TryAssignEntityToPlayer(entity, context.Sender, false, out SimulatedEntity simulatedEntity))
        {
            SimulationOwnershipChange ownershipChangePacket = new(simulatedEntity);
            await context.SendToAllAsync(ownershipChangePacket);
        }
    }
}
