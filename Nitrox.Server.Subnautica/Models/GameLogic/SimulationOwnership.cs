using System.Collections.Generic;
using System.Threading;
using NitroxModel.DataStructures;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

public class SimulationOwnershipData
{
    private readonly Lock locker = new();
    private readonly Dictionary<NitroxId, PlayerLock> playerLocksById = [];

    public bool TryToAcquire(NitroxId id, PeerId player, SimulationLockType requestedLock)
    {
        lock (locker)
        {
            // If no one is simulating then aquire a lock for this player
            if (!playerLocksById.TryGetValue(id, out PlayerLock playerLock))
            {
                playerLocksById[id] = new PlayerLock(player, requestedLock);
                return true;
            }

            // If this player owns the lock then they are already simulating
            if (playerLock.PlayerId == player)
            {
                // update the lock type in case they are attempting to downgrade
                playerLocksById[id] = new PlayerLock(player, requestedLock);
                return true;
            }

            // If the current lock owner has a transient lock then only override if we are requesting exclusive access
            if (playerLock.LockType == SimulationLockType.TRANSIENT && requestedLock == SimulationLockType.EXCLUSIVE)
            {
                playerLocksById[id] = new PlayerLock(player, requestedLock);
                return true;
            }

            // We must be requesting a transient lock and the owner already has a lock (either transient or exclusive).
            // there is no way to break it so we will return false.
            return false;
        }
    }

    public bool RevokeIfOwner(NitroxId id, PeerId player)
    {
        lock (locker)
        {
            if (playerLocksById.TryGetValue(id, out PlayerLock playerLock) && playerLock.PlayerId == player)
            {
                playerLocksById.Remove(id);
                return true;
            }

            return false;
        }
    }

    public List<NitroxId> RevokeAllForOwner(PeerId player)
    {
        lock (locker)
        {
            List<NitroxId> revokedIds = new();

            foreach (KeyValuePair<NitroxId, PlayerLock> idWithPlayerLock in playerLocksById)
            {
                if (idWithPlayerLock.Value.PlayerId == player)
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
        lock (locker)
        {
            return playerLocksById.Remove(id);
        }
    }

    public PeerId? GetPlayerForLock(NitroxId id)
    {
        lock (locker)
        {
            if (playerLocksById.TryGetValue(id, out PlayerLock playerLock))
            {
                return playerLock.PlayerId;
            }
        }
        return null;
    }

    public bool TryGetLock(NitroxId id, out PlayerLock playerLock)
    {
        lock (locker)
        {
            return playerLocksById.TryGetValue(id, out playerLock);
        }
    }

    public struct PlayerLock(PeerId playerId, SimulationLockType lockType)
    {
        public PeerId PlayerId { get; } = playerId;
        public SimulationLockType LockType { get; set; } = lockType;
    }
}
