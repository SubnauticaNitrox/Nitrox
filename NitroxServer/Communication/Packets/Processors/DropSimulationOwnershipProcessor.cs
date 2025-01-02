using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class DropSimulationOwnershipProcessor : AuthenticatedPacketProcessor<DropSimulationOwnership>
{
    private readonly SimulationOwnershipData simulationOwnershipData;
    private readonly PlayerManager playerManager;
    private readonly EntitySimulation entitySimulation;
    private readonly EntityRegistry entityRegistry;

    public DropSimulationOwnershipProcessor(SimulationOwnershipData simulationOwnershipData, PlayerManager playerManager, EntitySimulation entitySimulation, EntityRegistry entityRegistry)
    {
        this.simulationOwnershipData = simulationOwnershipData;
        this.playerManager = playerManager;
        this.entitySimulation = entitySimulation;
        this.entityRegistry = entityRegistry;
    }

    public override void Process(DropSimulationOwnership packet, Player player)
    {
        if (!entityRegistry.TryGetEntityById(packet.EntityId, out Entity entity) ||
            !simulationOwnershipData.RevokeIfOwner(packet.EntityId, player))
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
            playerManager.SendPacketToAllPlayers(packet);
        }
    }
}
