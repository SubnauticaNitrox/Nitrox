using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SimulationOwnershipRequestProcessor : AuthenticatedPacketProcessor<SimulationOwnershipRequest>
{
    private readonly PlayerManager playerManager;
    private readonly SimulationOwnershipData simulationOwnershipData;
    private readonly EntitySimulation entitySimulation;

    public SimulationOwnershipRequestProcessor(PlayerManager playerManager, SimulationOwnershipData simulationOwnershipData, EntitySimulation entitySimulation)
    {
        this.playerManager = playerManager;
        this.simulationOwnershipData = simulationOwnershipData;
        this.entitySimulation = entitySimulation;
    }

    public override void Process(SimulationOwnershipRequest ownershipRequest, Player player)
    {
        bool aquiredLock = simulationOwnershipData.TryToAcquire(ownershipRequest.Id, player, ownershipRequest.LockType);

        if (aquiredLock)
        {
            bool shouldEntityMove = entitySimulation.ShouldSimulateEntityMovement(ownershipRequest.Id);
            SimulationOwnershipChange simulationOwnershipChange = new(ownershipRequest.Id, player.Id, ownershipRequest.LockType, shouldEntityMove);
            playerManager.SendPacketToOtherPlayers(simulationOwnershipChange, player);
        }

        SimulationOwnershipResponse responseToPlayer = new(ownershipRequest.Id, aquiredLock, ownershipRequest.LockType);
        player.SendPacket(responseToPlayer);
    }
}
