using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class PlayerUnseeOutOfCellEntityProcessor : AuthenticatedPacketProcessor<PlayerUnseeOutOfCellEntity>
{
    private readonly SimulationOwnershipData simulationOwnershipData;
    private readonly PlayerManager playerManager;
    private readonly EntitySimulation entitySimulation;
    private readonly EntityRegistry entityRegistry;

    public PlayerUnseeOutOfCellEntityProcessor(SimulationOwnershipData simulationOwnershipData, PlayerManager playerManager, EntitySimulation entitySimulation, EntityRegistry entityRegistry)
    {
        this.simulationOwnershipData = simulationOwnershipData;
        this.playerManager = playerManager;
        this.entitySimulation = entitySimulation;
        this.entityRegistry = entityRegistry;
    }

    public override void Process(PlayerUnseeOutOfCellEntity packet, Player player)
    {
        // Most of this packet's utility is in the below Remove
        if (!player.OutOfCellVisibleEntities.Remove(packet.EntityId) ||
            !entityRegistry.TryGetEntityById(packet.EntityId, out Entity entity))
        {
            return;
        }

        // If player can still see the entity even after removing it from the OutOfCellVisibleEntities, then we don't need to change anything
        if (player.CanSee(entity))
        {
            return;
        }

        // If the player doesn't own the entity's simulation then we don't need to do anything
        if (!simulationOwnershipData.RevokeIfOwner(packet.EntityId, player))
        {
            return;
        }

        List<Player> otherPlayers = playerManager.GetConnectedPlayersExcept(player);
        if (entitySimulation.TryAssignEntityToPlayers(otherPlayers, entity, out SimulatedEntity simulatedEntity))
        {
            entitySimulation.BroadcastSimulationChanges([simulatedEntity]);
        }
        else
        {
            // No player has taken simulation on the entity
            playerManager.SendPacketToAllPlayers(new DropSimulationOwnership(packet.EntityId));
        }
    }
}
