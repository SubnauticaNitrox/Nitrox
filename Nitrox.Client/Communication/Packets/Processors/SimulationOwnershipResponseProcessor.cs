using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using UnityEngine;

namespace Nitrox.Client.Communication.Packets.Processors
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

            if (response.LockAquired)
            {
                RemoveRemoteController(response.Id);
            }
        }

        private void RemoveRemoteController(NitroxId id)
        {
            Optional<GameObject> gameObject = NitroxEntity.GetObjectFrom(id);

            if (gameObject.HasValue)
            {
                RemotelyControlled remotelyControlled = gameObject.Value.GetComponent<RemotelyControlled>();
                Object.Destroy(remotelyControlled);
            }
        }
    }
}
