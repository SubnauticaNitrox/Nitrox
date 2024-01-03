using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

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
                    EntityPositionBroadcaster.WatchEntity(simulatedEntity.Id);
                }

                simulationOwnershipManager.SimulateEntity(simulatedEntity.Id, simulatedEntity.LockType);
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
                    Log.Warn($"The server has forcibly revoked an exlusive lock - this may cause undefined behaviour.  GUID: {simulatedEntity.Id}");
                }

                simulationOwnershipManager.StopSimulatingEntity(simulatedEntity.Id);
                EntityPositionBroadcaster.StopWatchingEntity(simulatedEntity.Id);
            }

            // Avoid keeping artifacts of the entity's previous ChangesPosition state
            if (!simulatedEntity.ChangesPosition && NitroxEntity.TryGetComponentFrom(simulatedEntity.Id, out RemotelyControlled remotelyControlled))
            {
                Object.Destroy(remotelyControlled);
            }
        }
    }
}

