using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerUnseeOutOfCellEntityProcessor(SimulationOwnershipData simulationOwnershipData, PlayerService playerService, EntitySimulation entitySimulation, EntityRegistry entityRegistry)
    : IAuthPacketProcessor<PlayerUnseeOutOfCellEntity>
{
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly PlayerService playerService = playerService;
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public async Task Process(AuthProcessorContext context, PlayerUnseeOutOfCellEntity packet)
    {
        // Most of this packet's utility is in the below Remove
        if (!playerService.GetProperty<OutOfCellVisibleEntitiesProperty>(context.Sender).Value.Remove(packet.EntityId) ||
            !entityRegistry.TryGetEntityById(packet.EntityId, out Entity entity))
        {
            return;
        }
        // If player can still see the entity even after removing it from the OutOfCellVisibleEntities, then we don't need to change anything
        if (playerService.GetProperty<VisibleCellsProperty>(context.Sender).CanSee(entity))
        {
            return;
        }
        // If the player doesn't own the entity's simulation then we don't need to do anything
        if (!simulationOwnershipData.RevokeIfOwner(packet.EntityId, context.Sender))
        {
            return;
        }

        if (entitySimulation.TryAssignEntityToPlayers(playerService.GetSessionsExcept(context.Sender), entity, out SimulatedEntity simulatedEntity))
        {
            entitySimulation.BroadcastSimulationChanges([simulatedEntity]);
        }
        else
        {
            // No player has taken simulation on the entity
            await context.SendToAllAsync(new DropSimulationOwnership(packet.EntityId));
        }
    }
}
