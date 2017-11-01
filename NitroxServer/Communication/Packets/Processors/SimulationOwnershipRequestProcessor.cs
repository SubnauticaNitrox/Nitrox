using NitroxModel.Logger;
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
            Log.Debug(ownershipRequest);

            if (simulationOwnership.TryToAquireOwnership(ownershipRequest.Guid, player))
            {
                SimulationOwnershipChange simulationOwnershipChange = new SimulationOwnershipChange(ownershipRequest.Guid, player.Id);
                playerManager.SendPacketToAllPlayers(simulationOwnershipChange);
                Log.Debug("Sending: " + simulationOwnershipChange);
            }
        }
    }
}
