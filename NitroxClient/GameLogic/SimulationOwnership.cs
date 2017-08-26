using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures.Util;
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

            return ownedGuidsToPlayer.TryGetValue(guid, out owningPlayerId) && owningPlayerId == muliplayerSession.Reservation.PlayerId;
        }

        public void TryToRequestOwnership(string guid)
        {
            if (!ownedGuidsToPlayer.ContainsKey(guid) && !requestedGuids.Contains(guid))
            {
                // TODO: Add a timeout to requestedguids, because the server will not send anything back when a request is denied (or maybe we should). This means the current player can never even try to change ownership of the item until someone else changed it.
                SimulationOwnershipRequest ownershipRequest = new SimulationOwnershipRequest(muliplayerSession.Reservation.PlayerId, guid);
                packetSender.Send(ownershipRequest);
                requestedGuids.Add(guid);
            }
        }

        public void ReleaseOwnership(string guid)
        {
            string playerId;
            if (ownedGuidsToPlayer.TryGetValue(guid, out playerId) && playerId == muliplayerSession.Reservation.PlayerId)
            {
                SimulationOwnershipRelease ownershipRelease = new SimulationOwnershipRelease(playerId, guid);
                packetSender.Send(ownershipRelease);

                // This is not strictly necessary, as the server will respond to all players with a relase command (that is, assuming the player has ownership, or more generally: ownership is in sync).
                AddOwnedGuid(guid, Optional<string>.Empty());
                ownedGuidsToPlayer.Remove(guid);
            }
        }

        public void AddOwnedGuid(string guid, Optional<string> playerId)
        {
            // If a requested guid exists, remove it. Doesn't matter if we received ownership or not.
            requestedGuids.Remove(guid);

            if (playerId.IsPresent())
            {
                ownedGuidsToPlayer[guid] = playerId.Get();
            }
            else
            {
                ownedGuidsToPlayer.Remove(guid);
            }
        }
    }
}
