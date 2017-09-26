using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    // TODO: Merge this with SimulationOwnershipChange or SimulationOwnershipRequest (when OwnedGuid contains info about the current state)
    class SimulationOwnershipReleaseProcessor : AuthenticatedPacketProcessor<SimulationOwnershipRelease>
    {
        private readonly PlayerManager playerManager;
        private SimulationOwnership simulationOwnership;

        public SimulationOwnershipReleaseProcessor(PlayerManager playerManager, SimulationOwnership simulationOwnership)
        {
            this.playerManager = playerManager;
            this.simulationOwnership = simulationOwnership;
        }

        public override void Process(SimulationOwnershipRelease ownershipRelease, Player player)
        {
            if (simulationOwnership.RevokeIfOwner(ownershipRelease.Guid, player))
            {
                SimulationOwnershipChange simulationOwnershipChange = new SimulationOwnershipChange(ownershipRelease.Guid, null);
                playerManager.SendPacketToAllPlayers(simulationOwnershipChange);
            }
        }
    }
}
