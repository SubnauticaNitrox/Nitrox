using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class SimulationOwnership
    {
        private readonly IMultiplayerSession multiplayerSession;
        private readonly IPacketSender packetSender;
        private readonly Dictionary<NitroxId, SimulationLockType> simulatedIdsByLockType = new Dictionary<NitroxId, SimulationLockType>();
        private readonly Dictionary<NitroxId, LockRequestBase> lockRequestsById = new Dictionary<NitroxId, LockRequestBase>();

        public SimulationOwnership(IMultiplayerSession muliplayerSession, IPacketSender packetSender)
        {
            this.multiplayerSession = muliplayerSession;
            this.packetSender = packetSender;
        }
        public bool PlayerHasMinLockType(NitroxId id, SimulationLockType lockType)
        {
            if (simulatedIdsByLockType.TryGetValue(id, out SimulationLockType playerLock))
            {
                return playerLock <= lockType;
            }
            return false;
        }

        public bool HasAnyLockType(NitroxId id)
        {
            return PlayerHasMinLockType(id, SimulationLockType.TRANSIENT);
        }

        public bool HasExclusiveLock(NitroxId id)
        {
            return PlayerHasMinLockType(id, SimulationLockType.EXCLUSIVE);
        }

        public void RequestSimulationLock(NitroxId id, SimulationLockType lockType)
        {
            SimulationOwnershipRequest ownershipRequest = new SimulationOwnershipRequest(multiplayerSession.Reservation.PlayerId, id, lockType);
            packetSender.Send(ownershipRequest);
        }

        public void RequestSimulationLock(LockRequestBase lockRequest)
        {
            lockRequestsById[lockRequest.Id] = lockRequest;
            RequestSimulationLock(lockRequest.Id, lockRequest.LockType);
        }

        public void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired, SimulationLockType lockType)
        {
            Log.Info($"Received lock response, id: {id} {lockAquired} {lockType}");

            if (lockAquired)
            {
                SimulateEntity(id, lockType);
                TreatVehicleEntity(id, true);
            }

            if (lockRequestsById.TryGetValue(id, out LockRequestBase lockRequest))
            {
                lockRequest.LockRequestComplete(id, lockAquired);
                lockRequestsById.Remove(id);
            }
        }

        public void SimulateEntity(NitroxId id, SimulationLockType lockType)
        {
            simulatedIdsByLockType[id] = lockType;
        }

        public void StopSimulatingEntity(NitroxId id)
        {
            simulatedIdsByLockType.Remove(id);
        }

        public void TreatSimulatedEntity(SimulatedEntity simulatedEntity)
        {
            bool isLocalPlayerNewOwner = multiplayerSession.Reservation.PlayerId == simulatedEntity.PlayerId;

            if (TreatVehicleEntity(simulatedEntity.Id, isLocalPlayerNewOwner))
            {
                return;
            }

            if (isLocalPlayerNewOwner)
            {
                if (simulatedEntity.ChangesPosition)
                {
                    EntityPositionBroadcaster.WatchEntity(simulatedEntity.Id);
                }

                SimulateEntity(simulatedEntity.Id, simulatedEntity.LockType);
            }
            else if (HasAnyLockType(simulatedEntity.Id))
            {
                // The server has forcibly removed this lock from the client.  This is generally fine for
                // transient locks because it is only broadcasting position.  However, exclusive locks may
                // need additional cleanup (such as a person piloting a vehicle - they need to be kicked out)
                // We can later add a forcibly removed callback but as of right now we have no use-cases for
                // forcibly removing an exclusive lock.  Just log it if it happens....

                if (HasExclusiveLock(simulatedEntity.Id))
                {
                    Log.Warn($"The server has forcibly revoked an exclusive lock - this may cause undefined behaviour.  GUID: {simulatedEntity.Id}");
                }

                StopSimulatingEntity(simulatedEntity.Id);
                EntityPositionBroadcaster.StopWatchingEntity(simulatedEntity.Id);
            }

            // Avoid keeping artifacts of the entity's previous ChangesPosition state
            if (!simulatedEntity.ChangesPosition && NitroxEntity.TryGetComponentFrom(simulatedEntity.Id, out RemotelyControlled remotelyControlled))
            {
                GameObject.Destroy(remotelyControlled);
            }
        }

        public bool TryGetLockType(NitroxId nitroxId, out SimulationLockType simulationLockType)
        {
            return simulatedIdsByLockType.TryGetValue(nitroxId, out simulationLockType);
        }

        private bool TreatVehicleEntity(NitroxId entityId, bool isLocalPlayerNewOwner)
        {
            if (!NitroxEntity.TryGetObjectFrom(entityId, out GameObject gameObject) || !IsVehicle(gameObject))
            {
                return false;
            }
            
            MovementReplicator movementReplicator = gameObject.GetComponent<MovementReplicator>();
            if (isLocalPlayerNewOwner)
            {
                if (movementReplicator)
                {
                    GameObject.Destroy(movementReplicator);
                }
                MovementBroadcaster.RegisterWatched(gameObject, entityId);
            }
            else
            {
                if (!movementReplicator)
                {
                    gameObject.AddComponent<MovementReplicator>();
                }
                MovementBroadcaster.UnregisterWatched(entityId);
            }

            return true;
        }

        public bool IsVehicle(GameObject gameObject)
        {
            if (gameObject.GetComponent<Vehicle>())
            {
                return true;
            }
            if (gameObject.TryGetComponent(out SubRoot subRoot) && !subRoot.isBase)
            {
                return true;
            }

            return false;
        }
    }
}
