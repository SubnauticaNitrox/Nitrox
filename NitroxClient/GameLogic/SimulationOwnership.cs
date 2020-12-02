using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using NitroxModel.DataStructures;
using NitroxModel.Logger;
using System;

namespace NitroxClient.GameLogic
{
    public class SimulationOwnership
    {
        private class PlayerLock
        {
            public ushort? PlayerId { get; }
            public SimulationLockType LockType { get; set; }
        }
        public class SimulationOverride : IDisposable
        {
            SimulationOwnership simulationOwnership;
            NitroxId id;
            public SimulationOverride(SimulationOwnership simulationOwnership, NitroxId id)
            {
                this.simulationOwnership = simulationOwnership;
                this.id = id;
                this.simulationOwnership.AddSimulationOverride(id);
            }
            public void Dispose()
            {
                simulationOwnership.RemoveSimulationOverride(id);
            }
        }

        public delegate void LockRequestCompleted(NitroxId id, bool lockAquired);

        private readonly IMultiplayerSession muliplayerSession;
        private readonly IPacketSender packetSender;
        private readonly Dictionary<NitroxId, SimulationLockType> simulatedIdsByLockType = new Dictionary<NitroxId, SimulationLockType>();
        private readonly Dictionary<NitroxId, LockRequestCompleted> completeFunctionsById = new Dictionary<NitroxId, LockRequestCompleted>();
        private readonly HashSet<NitroxId> simulationOverride = new HashSet<NitroxId>();
        
        public SimulationOwnership(IMultiplayerSession muliplayerSession, IPacketSender packetSender)
        {
            this.muliplayerSession = muliplayerSession;
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

        public void RequestSimulationLock(NitroxId id, SimulationLockType lockType, LockRequestCompleted whenCompleted)
        {
            SimulationOwnershipRequest ownershipRequest = new SimulationOwnershipRequest(muliplayerSession.Reservation.PlayerId, id, lockType);
            packetSender.Send(ownershipRequest);
            completeFunctionsById[id] = whenCompleted;
        }

        public void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired, SimulationLockType lockType)
        {
            Log.Info("Received lock response, id: " + id + " " + lockAquired + " " + lockType);

            if (lockAquired)
            {
                SimulateEntity(id, lockType);
            }

            LockRequestCompleted requestCompleted = null;

            if (completeFunctionsById.TryGetValue(id, out requestCompleted) && requestCompleted != null)
            {
                completeFunctionsById.Remove(id);
                requestCompleted(id, lockAquired);
            }
            else
            {
                Log.Warn("Did not have an outstanding simulation request for " + id + " maybe there were multiple outstanding requests?");
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

        public void AddSimulationOverride(NitroxId id)
        {
            if (simulatedIdsByLockType.ContainsKey(id))
            {
                Log.Warn($"Tried to add simulation override for entity {id} the player already simulates.");
            }
            else
            {
                simulationOverride.Add(id);
            }
        }

        public void RemoveSimulationOverride(NitroxId id)
        {
            if (!simulationOverride.Remove(id))
            {
                Log.Warn($"Tried to remove non existing simulation override for NitroxId: {id}");
            }
        }

        public SimulationOverride GetSimulationOverride(NitroxId id)
        {
            return new SimulationOverride(this, id);
        }

        public bool SimulationLockOverrideActive(NitroxId id)
        {
            return simulationOverride.Contains(id);
        }
    }
}
