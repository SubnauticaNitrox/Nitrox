using System;
using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class SimulationOwnership
    {
        private readonly PacketSender packetSender;
        private readonly Dictionary<String, String> ownedGuidsToPlayer = new Dictionary<String, String>();
        private readonly HashSet<String> requestedGuids = new HashSet<String>();

        public SimulationOwnership(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public bool HasOwnership(String guid)
        {
            String owningPlayerId;

            if (ownedGuidsToPlayer.TryGetValue(guid, out owningPlayerId))
            {
                return owningPlayerId == packetSender.PlayerId;
            }

            return false;
        }

        public void TryToRequestOwnership(String guid)
        {
            if (!ownedGuidsToPlayer.ContainsKey(guid) && !requestedGuids.Contains(guid))
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
