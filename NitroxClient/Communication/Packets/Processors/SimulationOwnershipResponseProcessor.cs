using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class SimulationOwnershipResponseProcessor : ClientPacketProcessor<SimulationOwnershipResponse>
    {
        private readonly IMultiplayerSession multiplayerSession;
        private readonly SimulationOwnership simulationOwnershipManager;

        public SimulationOwnershipResponseProcessor(IMultiplayerSession multiplayerSession, SimulationOwnership simulationOwnershipManager)
        {
            this.multiplayerSession = multiplayerSession;
            this.simulationOwnershipManager = simulationOwnershipManager;
        }

        public override void Process(SimulationOwnershipResponse response)
        {
            /*
             * For now, we expect the simulation lock callback to setup entity broadcasting as
             * most items that are requesting an exclusive lock have custom broadcast code, ex:
             * vehicles like the cyclops.  However, we may one day want to add a watcher here
             * to ensure broadcast one day, ex:
             * 
             * EntityPositionBroadcaster.WatchEntity(simulatedEntity.Id, gameObject.Value);
             * 
             */
            simulationOwnershipManager.ReceivedSimulationLockResponse(response.Id, response.LockAquired, response.LockType);
        }
    }
}
