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
            Log.Info("Got SimulationOwnershipChangePacket: " + simulationOwnershipChange.Count);
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
                else if(simulationOwnershipManager.HasAnyLockType(simulatedEntity.Guid))
                {
                    // The server has forcibly removed this lock from the client.  This is generally fine for
                    // transient locks because it is only broadcasting position.  However, exclusive locks may
                    // need additional cleanup (such as a person piloting a vehicle - they need to be kicked out)
                    // We can later add a forcibly removed callback but as of right now we have no use-cases for
                    // forcibly removing an exclusive lock.  Just log it if it happens....

                    if(simulationOwnershipManager.HasExclusiveLock(simulatedEntity.Guid))
                    {
                        Log.Warn("The server has forcibly revoked an exlusive lock - this may cause undefined behaviour.  GUID: " + simulatedEntity.Guid);
                    }

                    simulationOwnershipManager.StopSimulatingGuid(simulatedEntity.Guid);
                    EntityPositionBroadcaster.StopWatchingEntity(simulatedEntity.Guid);
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
                Log.Error("Expected to simulate an unknown entity: " + guid);
            }
        }
    }
}
