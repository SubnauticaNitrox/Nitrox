using System.Collections.Generic;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

internal sealed class SimulationOwnershipData
{
    private readonly Dictionary<NitroxId, PlayerLock> playerLocksById = [];

    public bool TryToAcquire(NitroxId id, SessionId sessionId, SimulationLockType requestedLock)
    {
        lock (playerLocksById)
        {
            // If no one is simulating then acquire a lock for this player
            if (!playerLocksById.TryGetValue(id, out PlayerLock playerLock))
            {
                playerLocksById[id] = new PlayerLock(sessionId, requestedLock);
                return true;
            }

            // If this player owns the lock then they are already simulating
            if (playerLock.SessionId == sessionId)
            {
                // update the lock type in case they are attempting to downgrade
                playerLocksById[id] = new PlayerLock(sessionId, requestedLock);
                return true;
            }

            // If the current lock owner has a transient lock then only override if we are requesting exclusive access
            if (playerLock.LockType == SimulationLockType.TRANSIENT && requestedLock == SimulationLockType.EXCLUSIVE)
            {
                playerLocksById[id] = new PlayerLock(sessionId, requestedLock);
                return true;
            }

            // We must be requesting a transient lock and the owner already has a lock (either transient or exclusive).
            // there is no way to break it so we will return false.
            return false;
        }
    }

    public bool RevokeIfOwner(NitroxId id, SessionId sessionId)
    {
        lock (playerLocksById)
        {
            if (playerLocksById.TryGetValue(id, out PlayerLock playerLock) && playerLock.SessionId == sessionId)
            {
                playerLocksById.Remove(id);
                return true;
            }

            return false;
        }
    }

    public List<NitroxId> RevokeAllForOwner(SessionId sessionId)
    {
        lock (playerLocksById)
        {
            List<NitroxId> revokedIds = [];

            foreach (KeyValuePair<NitroxId, PlayerLock> idWithPlayerLock in playerLocksById)
            {
                if (idWithPlayerLock.Value.SessionId == sessionId)
                {
                    revokedIds.Add(idWithPlayerLock.Key);
                }
            }

            foreach (NitroxId id in revokedIds)
            {
                playerLocksById.Remove(id);
            }

            return revokedIds;
        }
    }

    public bool RevokeOwnerOfId(NitroxId id)
    {
        lock (playerLocksById)
        {
            return playerLocksById.Remove(id);
        }
    }

    public SessionId? GetSessionIdForLock(NitroxId id)
    {
        lock (playerLocksById)
        {
            if (playerLocksById.TryGetValue(id, out PlayerLock playerLock))
            {
                return playerLock.SessionId;
            }
        }
        return null;
    }

    public bool TryGetLock(NitroxId id, out PlayerLock playerLock)
    {
        lock (playerLocksById)
        {
            return playerLocksById.TryGetValue(id, out playerLock);
        }
    }

    public struct PlayerLock(SessionId sessionId, SimulationLockType lockType)
    {
        public SessionId SessionId { get; } = sessionId;
        public SimulationLockType LockType { get; set; } = lockType;
    }
}
