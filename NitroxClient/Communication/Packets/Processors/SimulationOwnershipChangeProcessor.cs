using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class SimulationOwnershipChangeProcessor : ClientPacketProcessor<SimulationOwnershipChange>
    {
        private readonly IMultiplayerSession multiplayerSession;
        private readonly INitroxLogger log;
        private readonly SimulationOwnership simulationOwnershipManager;

        public SimulationOwnershipChangeProcessor(IMultiplayerSession multiplayerSession, SimulationOwnership simulationOwnershipManager, INitroxLogger logger)
        {
            this.multiplayerSession = multiplayerSession;
            this.simulationOwnershipManager = simulationOwnershipManager;
            log = logger;
        }

        public override void Process(SimulationOwnershipChange simulationOwnershipChange)
        {
            foreach (SimulatedEntity simulatedEntity in simulationOwnershipChange.Entities)
            {
                if (multiplayerSession.Reservation.PlayerId == simulatedEntity.PlayerId)
                {
                    if(simulatedEntity.ChangesPosition)
                    {
                        StartBroadcastingEntityPosition(simulatedEntity.Guid);
                    }

                    simulationOwnershipManager.SimulateGuid(simulatedEntity.Guid, SimulationLockType.TRANSIENT);
                }
            }
        }

        private void StartBroadcastingEntityPosition(string guid)
        {
            Optional<GameObject> gameObject = GuidHelper.GetObjectFrom(guid);

            if (gameObject.IsPresent())
            {
                EntityPositionBroadcaster.WatchEntity(guid, gameObject.Get());
            }
            else
            {
                log.Error("Expected to simulate an unknown entity: " + guid);
            }
        }
    }
}
