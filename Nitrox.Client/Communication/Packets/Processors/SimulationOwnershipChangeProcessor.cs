using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets;
using UnityEngine;

namespace Nitrox.Client.Communication.Packets.Processors
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
                if (multiplayerSession.Reservation.PlayerId == simulatedEntity.PlayerId)
                {
                    if (simulatedEntity.ChangesPosition)
                    {
                        StartBroadcastingEntityPosition(simulatedEntity.Id);
                    }

                    simulationOwnershipManager.SimulateEntity(simulatedEntity.Id, SimulationLockType.TRANSIENT);
                }
                else if (simulationOwnershipManager.HasAnyLockType(simulatedEntity.Id))
                {
                    // The server has forcibly removed this lock from the client.  This is generally fine for
                    // transient locks because it is only broadcasting position.  However, exclusive locks may
                    // need additional cleanup (such as a person piloting a vehicle - they need to be kicked out)
                    // We can later add a forcibly removed callback but as of right now we have no use-cases for
                    // forcibly removing an exclusive lock.  Just log it if it happens....

                    if (simulationOwnershipManager.HasExclusiveLock(simulatedEntity.Id))
                    {
                        Log.Warn("The server has forcibly revoked an exlusive lock - this may cause undefined behaviour.  GUID: " + simulatedEntity.Id);
                    }

                    simulationOwnershipManager.StopSimulatingEntity(simulatedEntity.Id);
                    EntityPositionBroadcaster.StopWatchingEntity(simulatedEntity.Id);
                }
            }
        }

        private void StartBroadcastingEntityPosition(NitroxId id)
        {
            Optional<GameObject> gameObject = NitroxEntity.GetObjectFrom(id);

            if (gameObject.HasValue)
            {
                EntityPositionBroadcaster.WatchEntity(id, gameObject.Value);
            }
#if DEBUG
            else
            {
                Log.Error($"Expected to simulate an unknown entity: {id}");
            }
#endif
        }
    }
}
