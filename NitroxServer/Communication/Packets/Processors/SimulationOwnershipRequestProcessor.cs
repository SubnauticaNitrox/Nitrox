using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    class SimulationOwnershipRequestProcessor : AuthenticatedPacketProcessor<SimulationOwnershipRequest>
    {
        private readonly PlayerManager playerManager;
        private readonly SimulationOwnership simulationOwnership;

        public SimulationOwnershipRequestProcessor(PlayerManager playerManager, SimulationOwnership simulationOwnership)
        {
            this.playerManager = playerManager;
            this.simulationOwnership = simulationOwnership;
        }

        public override void Process(SimulationOwnershipRequest ownershipRequest, Player player)
        {
            if (simulationOwnership.TryToAcquire(ownershipRequest.Guid, player))
            {
                // TODO: Distribute ownership in the `SimulationOwnership` class.
                SimulationOwnershipChange simulationOwnershipChange = new SimulationOwnershipChange(ownershipRequest.Guid, player.Id);
                playerManager.SendPacketToAllPlayers(simulationOwnershipChange);
            }
        }
    }
}
