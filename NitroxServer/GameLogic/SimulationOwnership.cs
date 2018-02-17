using System.Collections.Generic;

namespace NitroxServer.GameLogic
{
    public class SimulationOwnership
    {
        Dictionary<string, Player> guidsByPlayer = new Dictionary<string, Player>();

        // TODO: redistribute upon disconnect

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
    }
}
