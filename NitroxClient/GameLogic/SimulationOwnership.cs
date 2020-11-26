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

            public PlayerLock(ushort? playerId, SimulationLockType lockType)
            {
                PlayerId = playerId;
                LockType = lockType;
            }
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
        private readonly Dictionary<NitroxId, PlayerLock> simulatedIdsByLockType = new Dictionary<NitroxId, PlayerLock>();
        private readonly Dictionary<NitroxId, LockRequestCompleted> completeFunctionsById = new Dictionary<NitroxId, LockRequestCompleted>();
        private readonly HashSet<NitroxId> simulationOverride = new HashSet<NitroxId>();
        
        public SimulationOwnership(IMultiplayerSession muliplayerSession, IPacketSender packetSender)
        {
            this.muliplayerSession = muliplayerSession;
            this.packetSender = packetSender;
        }
        public bool PlayerHasMinLockType(NitroxId id, bool isOtherPlayer, SimulationLockType lockType)
        {
            if (simulatedIdsByLockType.TryGetValue(id, out PlayerLock playerLock))
            {
                bool ownPlayer = playerLock.PlayerId == muliplayerSession.Reservation.PlayerId;
                bool accepted = isOtherPlayer ? !ownPlayer : ownPlayer;
                return accepted && playerLock.LockType <= lockType;
            }

            return false;
        }

        public bool HasAnyLockType(NitroxId id)
        {
            return PlayerHasMinLockType(id, false, SimulationLockType.TRANSIENT);
        }

        public bool HasExclusiveLock(NitroxId id)
        {
            return PlayerHasMinLockType(id, false, SimulationLockType.EXCLUSIVE);
        }

        public bool OtherPlayerHasAnyLock(NitroxId id)
        {
            return PlayerHasMinLockType(id, true, SimulationLockType.TRANSIENT);
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
            else if (!OtherPlayerHasAnyLock(id))
            {
                AddSimulationLockForUnknown(id, lockType);
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

        public void RemovePlayerSimulations(ushort playerId)
        {
            // When a player disconnects, we need to remove his locks
            // We also remove locks of unknown player, as they could come from the disconnected player
            HashSet<NitroxId> toRemove = new HashSet<NitroxId>();
            foreach (KeyValuePair<NitroxId, PlayerLock> playerLock in simulatedIdsByLockType)
            {
                if (!playerLock.Value.PlayerId.HasValue ||playerLock.Value.PlayerId == playerId)
                {
                    toRemove.Add(playerLock.Key);
                }
            }
            foreach (NitroxId id in toRemove)
            {
                simulatedIdsByLockType.Remove(id);
            }
        }

        public void SimulateEntity(NitroxId id, SimulationLockType lockType)
        {
            simulatedIdsByLockType[id] = new PlayerLock(muliplayerSession.Reservation.PlayerId, lockType);
        }

        public void StopSimulatingEntity(NitroxId id)
        {
            simulatedIdsByLockType.Remove(id);
        }
        
        public void AddSimulationLock(NitroxId id, ushort playerId, SimulationLockType lockType)
        {
            simulatedIdsByLockType[id] = new PlayerLock(playerId, lockType);
        }

        public void AddSimulationLockForUnknown(NitroxId id, SimulationLockType lockType)
        {
            simulatedIdsByLockType[id] = new PlayerLock(null, lockType);
        }

        public void AddSimulationOverride(NitroxId id)
        {
            simulatedIdsByLockType.TryGetValue(id, out PlayerLock @lock);
            if (@lock.PlayerId == muliplayerSession.Reservation.PlayerId)
            {
                Log.Warn($"Tried to add simulation override for entity {id} the player already simulates.");
            }
            else
            {
                simulationOverride.Add(id);
            }
        }

        public SimulationOverride GetSimulationOverride(NitroxId id)
        {
            return new SimulationOverride(this, id);
        }

        public void RemoveSimulationOverride(NitroxId id)
        {
            if (!simulationOverride.Remove(id))
            {
                Log.Warn($"Tried to remove non existing simulation override for NitroxId: {id}");
            }
        }

        public bool SimulationLockOverrideActive(NitroxId id)
        {
            return simulationOverride.Contains(id);
        }
    }
}
