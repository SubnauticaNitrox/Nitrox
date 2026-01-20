using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SimulationOwnershipRequestProcessor(IPacketSender packetSender, SimulationOwnershipData simulationOwnershipData, EntitySimulation entitySimulation) : AuthenticatedPacketProcessor<SimulationOwnershipRequest>
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly EntitySimulation entitySimulation = entitySimulation;

    public override void Process(SimulationOwnershipRequest ownershipRequest, Player player)
    {
        bool aquiredLock = simulationOwnershipData.TryToAcquire(ownershipRequest.Id, player, ownershipRequest.LockType);

        if (aquiredLock)
        {
            bool shouldEntityMove = entitySimulation.ShouldSimulateEntityMovement(ownershipRequest.Id);
            SimulationOwnershipChange simulationOwnershipChange = new(ownershipRequest.Id, player.SessionId, ownershipRequest.LockType, shouldEntityMove);
            packetSender.SendPacketToOthersAsync(simulationOwnershipChange, player.SessionId);
        }

        SimulationOwnershipResponse responseToPlayer = new(ownershipRequest.Id, aquiredLock, ownershipRequest.LockType);
        packetSender.SendPacketAsync(responseToPlayer, player.SessionId);
    }
}
