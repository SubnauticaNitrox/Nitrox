using NitroxModel.DataStructures;
using System.Collections.Generic;

namespace NitroxServer.GameLogic
{
    public class SimulationOwnershipData
    {
        struct PlayerLock
        {
            public Player Player { get; }
            public SimulationLockType LockType { get; }

            public PlayerLock(Player player, SimulationLockType lockType)
            {
                Player = player;
                LockType = lockType;
            }
        }

        Dictionary<string, PlayerLock> playerLocksByGuid = new Dictionary<string, PlayerLock>();
        
        public bool TryToAcquire(string guid, Player player, SimulationLockType requestedLock)
        {
            lock (playerLocksByGuid)
            {
                PlayerLock playerLock;

                // If no one is simulating then aquire a lock for this player
                if (!playerLocksByGuid.TryGetValue(guid, out playerLock))
                {
                    playerLocksByGuid[guid] = new PlayerLock(player, requestedLock);
                    return true;
                }

                // If this player owns the lock then they are already simulating
                if (playerLock.Player == player)
                {
                    return true;
                }

                // If the current lock owner has a transient lock then only override if we are requesting exclusive access
                if (playerLock.LockType == SimulationLockType.TRANSIENT && requestedLock == SimulationLockType.EXCLUSIVE)
                {
                    playerLocksByGuid[guid] = new PlayerLock(player, requestedLock);
                    return true;
                }
                
                // We must be requesting a transient lock and the owner already has a lock (either transient or exclusive).
                // there is no way to break it so we will return false.
                return false;
            }
        }
        
        public bool RevokeIfOwner(string guid, Player player)
        {
            lock (playerLocksByGuid)
            {
                PlayerLock playerLock;

                if (playerLocksByGuid.TryGetValue(guid, out playerLock) && playerLock.Player == player)
                {
                    playerLocksByGuid.Remove(guid);
                    return true;
                }

                return false;
            }
        }

        public List<string> RevokeAllForOwner(Player player)
        {
            lock (playerLocksByGuid)
            {
                List<string> revokedGuids = new List<string>();

                foreach(KeyValuePair<string, PlayerLock> guidWithPlayerLock in playerLocksByGuid)
                {
                    if(guidWithPlayerLock.Value.Player == player)
                    {
                        revokedGuids.Add(guidWithPlayerLock.Key);
                    }
                }

                foreach(string guid in revokedGuids)
                {
                    playerLocksByGuid.Remove(guid);
                }

                return revokedGuids;
            }
        }
    }
}
