using System.Collections.Generic;

namespace NitroxServer.GameLogic
{
    public class SimulationOwnershipData
    {
        Dictionary<string, Player> guidsByPlayer = new Dictionary<string, Player>();
        
        public bool TryToAcquire(string guid, Player player)
        {
            lock (guidsByPlayer)
            {
                Player owningPlayer;

                if (guidsByPlayer.TryGetValue(guid, out owningPlayer))
                {
                    return (owningPlayer == player);
                }

                guidsByPlayer[guid] = player;
                return true;
            }
        }

        public bool RevokeIfOwner(string guid, Player player)
        {
            lock (guidsByPlayer)
            {
                Player owningPlayer;

                if (guidsByPlayer.TryGetValue(guid, out owningPlayer) && owningPlayer == player)
                {
                    guidsByPlayer.Remove(guid);
                    return true;
                }

                return false;
            }
        }

        public List<string> RevokeAllForOwner(Player player)
        {
            lock (guidsByPlayer)
            {
                List<string> revokedGuids = new List<string>();

                foreach(KeyValuePair<string, Player> guidWithPlayer in guidsByPlayer)
                {
                    if(guidWithPlayer.Value == player)
                    {
                        revokedGuids.Add(guidWithPlayer.Key);
                    }
                }

                foreach(string guid in revokedGuids)
                {
                    guidsByPlayer.Remove(guid);
                }

                return revokedGuids;
            }
        }
    }
}
