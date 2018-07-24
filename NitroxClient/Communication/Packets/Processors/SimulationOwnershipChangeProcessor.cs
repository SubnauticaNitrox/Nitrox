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
        private readonly SimulationOwnership simulationOwnershipManager;

        public SimulationOwnershipChangeProcessor(IMultiplayerSession multiplayerSession, SimulationOwnership simulationOwnershipManager)
        {
            this.multiplayerSession = multiplayerSession;
            this.simulationOwnershipManager = simulationOwnershipManager;
        }

        public override void Process(SimulationOwnershipChange simulationOwnershipChange)
        {
            foreach (SimulatedEntity simulatedEntity in simulationOwnershipChange.Entities)
            {
                if (multiplayerSession.Reservation.PlayerId == simulatedEntity.PlayerId && simulatedEntity.ChangesPosition)
                {
                    SimulateEntity(simulatedEntity);
                }

                simulationOwnershipManager.AddOwnedGuid(simulatedEntity.Guid, simulatedEntity.PlayerId);
            }
        }

        private void SimulateEntity(SimulatedEntity simulatedEntity)
        {
            Optional<GameObject> gameObject = GuidHelper.GetObjectFrom(simulatedEntity.Guid);

            if (gameObject.IsPresent())
            {
                EntityPositionBroadcaster.WatchEntity(simulatedEntity.Guid, gameObject.Get());
            }
            else
            {
                Log.Error("Expected to simulate an unknown entity: " + simulatedEntity.Guid);
            }
        }
    }
}
