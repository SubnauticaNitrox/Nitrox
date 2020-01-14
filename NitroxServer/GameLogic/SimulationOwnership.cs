﻿using NitroxModel.DataStructures;
using System.Collections.Generic;

namespace NitroxServer.GameLogic
{
    public class SimulationOwnershipData
    {
        struct PlayerLock
        {
            public Player Player { get; }
            public SimulationLockType LockType { get; set; }

            public PlayerLock(Player player, SimulationLockType lockType)
            {
                Player = player;
                LockType = lockType;
            }
        }

        Dictionary<NitroxId, PlayerLock> playerLocksById = new Dictionary<NitroxId, PlayerLock>();
        
        public bool TryToAcquire(NitroxId id, Player player, SimulationLockType requestedLock)
        {
            lock (playerLocksById)
            {
                PlayerLock playerLock;

                // If no one is simulating then aquire a lock for this player
                if (!playerLocksById.TryGetValue(id, out playerLock))
                {
                    playerLocksById[id] = new PlayerLock(player, requestedLock);
                    return true;
                }

                // If this player owns the lock then they are already simulating
                if (playerLock.Player == player)
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
        
        public bool RevokeIfOwner(NitroxId id, Player player)
        {
            lock (playerLocksById)
            {
                PlayerLock playerLock;

                if (playerLocksById.TryGetValue(id, out playerLock) && playerLock.Player == player)
                {
                    playerLocksById.Remove(id);
                    return true;
                }

                return false;
            }
        }

        public List<NitroxId> RevokeAllForOwner(Player player)
        {
            lock (playerLocksById)
            {
                List<NitroxId> revokedIds = new List<NitroxId>();

                foreach(KeyValuePair<NitroxId, PlayerLock> idWithPlayerLock in playerLocksById)
                {
                    if(idWithPlayerLock.Value.Player == player)
                    {
                        revokedIds.Add(idWithPlayerLock.Key);
                    }
                }

                foreach(NitroxId id in revokedIds)
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
                if(playerLocksById.ContainsKey(id))
                {
                    playerLocksById.Remove(id);
                    return true;
                }
            }

            return false;
        }
    }
}
