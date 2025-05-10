using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerUnseeOutOfCellEntityProcessor(SimulationOwnershipData simulationOwnershipData, EntitySimulation entitySimulation, EntityRegistry entityRegistry)
    : IAuthPacketProcessor<PlayerUnseeOutOfCellEntity>
{
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public async Task Process(AuthProcessorContext context, PlayerUnseeOutOfCellEntity packet)
    {
        // TODO: USE DATABASE
        // // Most of this packet's utility is in the below Remove
        // if (!player.OutOfCellVisibleEntities.Remove(packet.EntityId) ||
        //     !entityRegistry.TryGetEntityById(packet.EntityId, out Entity entity))
        // {
        //     return;
        // }
        //
        // // If player can still see the entity even after removing it from the OutOfCellVisibleEntities, then we don't need to change anything
        // if (player.CanSee(entity))
        // {
        //     return;
        // }
        //
        // // If the player doesn't own the entity's simulation then we don't need to do anything
        // if (!simulationOwnershipData.RevokeIfOwner(packet.EntityId, player))
        // {
        //     return;
        // }
        //
        // List<NitroxServer.Player> otherPlayers = playerManager.GetConnectedPlayersExcept(player);
        // if (entitySimulation.TryAssignEntityToPlayers(otherPlayers, entity, out SimulatedEntity simulatedEntity))
        // {
        //     entitySimulation.BroadcastSimulationChanges([simulatedEntity]);
        // }
        // else
        // {
        //     // No player has taken simulation on the entity
        //     context.ReplyToAll(new DropSimulationOwnership(packet.EntityId));
        // }
    }
}
