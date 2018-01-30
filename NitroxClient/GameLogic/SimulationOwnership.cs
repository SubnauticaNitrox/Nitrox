﻿using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class SimulationOwnership
    {
        private readonly IPacketSender packetSender;
        private readonly Dictionary<string, string> ownedGuidsToPlayer = new Dictionary<string, string>();
        private readonly HashSet<string> requestedGuids = new HashSet<string>();

        public SimulationOwnership(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public bool HasOwnership(string guid)
        {
            string owningPlayerId;

            if (ownedGuidsToPlayer.TryGetValue(guid, out owningPlayerId))
            {
                return owningPlayerId == packetSender.PlayerId;
            }

            return false;
        }

        public void TryToRequestOwnership(string guid)
        {
            if (!ownedGuidsToPlayer.ContainsKey(guid) && !requestedGuids.Contains(guid))
            {
                SimulationOwnershipRequest ownershipRequest = new SimulationOwnershipRequest(packetSender.PlayerId, guid);
                packetSender.Send(ownershipRequest);
                requestedGuids.Add(guid);
            }
        }

        public void AddOwnedGuid(string guid, string playerId)
        {
            ownedGuidsToPlayer[guid] = playerId;
        }
    }
}
