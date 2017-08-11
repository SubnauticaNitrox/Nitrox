using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NitroxServer.GameLogic
{
    public class SimulationOwnership
    {
        Dictionary<String, Player> guidsByPlayer = new Dictionary<String, Player>();
        
        //TODO: redistribute upon disconnect

        public bool TryToAquireOwnership(String guid, Player player)
        {
            lock(guidsByPlayer)
            {
                Player owningPlayer;

                if (guidsByPlayer.TryGetValue(guid, out owningPlayer))
                {
                    if(owningPlayer != player)
                    {
                        return false;
                    }

                    return true;
                }

                guidsByPlayer[guid] = player;
                return true;
            }
        }

    }
}
