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
        private readonly INitroxLogger log;
        private readonly Dictionary<string, SimulationLockType> simulatedGuidsByLockType = new Dictionary<string, SimulationLockType>();
        private readonly Dictionary<string, LockRequestCompleted> completeFunctionsByGuid = new Dictionary<string, LockRequestCompleted>();
        
        public SimulationOwnership(IMultiplayerSession muliplayerSession, IPacketSender packetSender, INitroxLogger logger)
        {
            this.muliplayerSession = muliplayerSession;
            this.packetSender = packetSender;
            log = logger;
        }

        public bool HasAnyLockType(string guid)
        {
            return simulatedGuidsByLockType.ContainsKey(guid);
        }

        public bool HasExclusiveLock(string guid)
        {
            SimulationLockType activeLockType;

            if (simulatedGuidsByLockType.TryGetValue(guid, out activeLockType))
            {
                return (activeLockType == SimulationLockType.EXCLUSIVE);
            }

            return false;
        }

        public void RequestSimulationLock(string guid, SimulationLockType lockType, LockRequestCompleted whenCompleted)
        {
            SimulationOwnershipRequest ownershipRequest = new SimulationOwnershipRequest(muliplayerSession.Reservation.PlayerId, guid, lockType);
            packetSender.Send(ownershipRequest);
            completeFunctionsByGuid[guid] = whenCompleted;
        }

        public void ReceivedSimulationLockResponse(string guid, bool lockAquired, SimulationLockType lockType)
        {
            log.Info($"Received lock response, guid: {guid} {lockAquired} {lockType}");

            if (lockAquired)
            {
                SimulateGuid(guid, lockType);
            }

            LockRequestCompleted requestCompleted = null;

            if (completeFunctionsByGuid.TryGetValue(guid, out requestCompleted) && requestCompleted != null)
            {
                completeFunctionsByGuid.Remove(guid);
                requestCompleted(guid, lockAquired);
            }
            else
            {
                log.Warn($"Did not have an outstanding simulation request for {guid} maybe there were multiple outstanding requests?");
            }
        }

        public void SimulateGuid(string guid, SimulationLockType lockType)
        {
            simulatedGuidsByLockType[guid] = lockType;
        }
    }
}
