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
        private readonly Dictionary<NitroxId, SimulationLockType> simulatedIdsByLockType = [];
        private readonly Dictionary<NitroxId, LockRequestBase> lockRequestsById = [];

        private readonly Dictionary<NitroxId, SimulatedEntity> newerSimulationById = [];

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
                TreatVehicleEntity(id, true, lockType);
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

            if (TreatVehicleEntity(simulatedEntity.Id, isLocalPlayerNewOwner, simulatedEntity.LockType) ||
                newerSimulationById.ContainsKey(simulatedEntity.Id))
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
                Object.Destroy(remotelyControlled);
            }
        }

        public void DropSimulationFrom(NitroxId entityId)
        {
            StopSimulatingEntity(entityId);
            EntityPositionBroadcaster.StopWatchingEntity(entityId);
            if (!NitroxEntity.TryGetObjectFrom(entityId, out GameObject gameObject))
            {
                return;
            }

            if (gameObject.TryGetComponent(out RemotelyControlled remotelyControlled))
            {
                Object.Destroy(remotelyControlled);
            }
        }

        public bool TryGetLockType(NitroxId nitroxId, out SimulationLockType simulationLockType)
        {
            return simulatedIdsByLockType.TryGetValue(nitroxId, out simulationLockType);
        }

        public bool TreatVehicleEntity(NitroxId entityId, bool isLocalPlayerNewOwner, SimulationLockType simulationLockType)
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
                    Object.Destroy(movementReplicator);
                }
                MovementBroadcaster.RegisterWatched(gameObject, entityId);
                SimulateEntity(entityId, simulationLockType);
            }
            else
            {
                if (!movementReplicator)
                {
                    MovementReplicator.AddReplicatorToObject(gameObject);
                }
                MovementBroadcaster.UnregisterWatched(entityId);
                StopSimulatingEntity(entityId);
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

        public void RegisterNewerSimulation(NitroxId entityId, SimulatedEntity simulatedEntity)
        {
            newerSimulationById[entityId] = simulatedEntity;
        }

        public void ApplyNewerSimulation(NitroxId nitroxId)
        {
            if (newerSimulationById.TryGetValue(nitroxId, out SimulatedEntity simulatedEntity))
            {
                newerSimulationById.Remove(nitroxId);
                TreatSimulatedEntity(simulatedEntity);
            }
        }

        public void ClearNewerSimulations()
        {
            newerSimulationById.Clear();
        }

        public bool TryGetLockType(NitroxId nitroxId, out SimulationLockType simulationLockType)
        {
            return simulatedIdsByLockType.TryGetValue(nitroxId, out simulationLockType);
        }
    }
}
