using System.Collections.Generic;

namespace NitroxServer.GameLogic
{
    public class SimulationOwnership
    {
        // TODO: redistribute upon disconnect
        // TODO: Also upon connect.

        Dictionary<string, Player> guidsByPlayer = new Dictionary<string, Player>();

        // TODO: redistribute upon disconnect

        public bool TryToAcquire(string guid, Player player)
        {
            lock (guidsByPlayer)
            {
                Player owningPlayer;

                if (guidsByPlayer.TryGetValue(guid, out owningPlayer))
                {
                    // Ownership can only be taken if no-one else has it.
                    return owningPlayer == player;
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
