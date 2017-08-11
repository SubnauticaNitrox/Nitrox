using NitroxClient.Communication;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;

namespace NitroxClient.GameLogic
{
    public class SimulationOwnership
    {
        private PacketSender packetSender;
        private Dictionary<String, String> ownedGuidsToPlayer;
        private HashSet<String> requestedGuids;

        public SimulationOwnership(PacketSender packetSender)
        {
            this.packetSender = packetSender;
            this.ownedGuidsToPlayer = new Dictionary<String, String>();
            this.requestedGuids = new HashSet<String>();
        }
         
        public bool HasOwnership(String guid)
        {
            String owningPlayerId;

            if(ownedGuidsToPlayer.TryGetValue(guid, out owningPlayerId))
            {
                return owningPlayerId == packetSender.PlayerId;
            }

            return false;
        }

        public void TryToRequestOwnership(String guid)
        {
            if(!ownedGuidsToPlayer.ContainsKey(guid) && !requestedGuids.Contains(guid))
            {
                SimulationOwnershipRequest ownershipRequest = new SimulationOwnershipRequest(packetSender.PlayerId, guid);
                packetSender.Send(ownershipRequest);
                requestedGuids.Add(guid);
            }            
        }

        public void AddOwnedGuid(String guid, String playerId)
        {
            ownedGuidsToPlayer[guid] = playerId;
        }
    }
}
