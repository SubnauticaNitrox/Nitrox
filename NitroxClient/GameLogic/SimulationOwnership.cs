using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class SimulationOwnership
    {
        private readonly IMultiplayerSession muliplayerSession;
        private readonly IPacketSender packetSender;
        private readonly Dictionary<string, string> ownedGuidsToPlayer = new Dictionary<string, string>();
        private readonly HashSet<string> requestedGuids = new HashSet<string>();

        public SimulationOwnership(IMultiplayerSession muliplayerSession, IPacketSender packetSender)
        {
            this.muliplayerSession = muliplayerSession;
            this.packetSender = packetSender;
        }

        public bool HasOwnership(string guid)
        {
            string owningPlayerId;

            if (ownedGuidsToPlayer.TryGetValue(guid, out owningPlayerId))
            {
                return owningPlayerId == muliplayerSession.Reservation.PlayerId;
            }

            return false;
        }

        public void TryToRequestOwnership(string guid)
        {
            if (!ownedGuidsToPlayer.ContainsKey(guid) && !requestedGuids.Contains(guid))
            {
                SimulationOwnershipRequest ownershipRequest = new SimulationOwnershipRequest(muliplayerSession.Reservation.PlayerId, guid);
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
