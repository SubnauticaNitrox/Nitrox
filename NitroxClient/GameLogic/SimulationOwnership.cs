using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using NitroxModel.DataStructures;
using NitroxModel.Logger;

namespace NitroxClient.GameLogic
{
    public class SimulationOwnership
    {
        public delegate void LockRequestCompleted(string guid, bool lockAquired);

        private readonly IMultiplayerSession muliplayerSession;
        private readonly IPacketSender packetSender;
        private readonly Dictionary<string, string> ownedGuidsToPlayer = new Dictionary<string, string>();
        private readonly Dictionary<string, LockRequestCompleted> completeFunctionsByGuid = new Dictionary<string, LockRequestCompleted>();
        
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

        public void RequestSimulationLock(string guid, SimulationLockType lockType, LockRequestCompleted whenCompleted)
        {
            if (!ownedGuidsToPlayer.ContainsKey(guid))
            {
                SimulationOwnershipRequest ownershipRequest = new SimulationOwnershipRequest(muliplayerSession.Reservation.PlayerId, guid, lockType);
                packetSender.Send(ownershipRequest);
                completeFunctionsByGuid.Add(guid, whenCompleted);
            }
        }

        public void ReceivedSimulationLockResponse(string guid, bool lockAquired)
        {
            if(lockAquired)
            {
                AddOwnedGuid(guid, muliplayerSession.Reservation.PlayerId);
            }

            LockRequestCompleted requestCompleted = null;

            if (completeFunctionsByGuid.TryGetValue(guid, out requestCompleted))
            {
                completeFunctionsByGuid.Remove(guid);
                requestCompleted(guid, lockAquired);
            }
            else
            {
                Log.Warn("Did not have an outstanding simulation request for " + guid + " maybe there were multiple outstanding requests?");
            }
        }

        public void AddOwnedGuid(string guid, string playerId)
        {
            ownedGuidsToPlayer[guid] = playerId;
        }
    }
}
