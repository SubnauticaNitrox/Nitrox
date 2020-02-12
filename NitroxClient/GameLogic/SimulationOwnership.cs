using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using NitroxModel.DataStructures;
using NitroxModel.Logger;

namespace NitroxClient.GameLogic
{
    public class SimulationOwnership
    {
        public delegate void LockRequestCompleted(NitroxId id, bool lockAquired);

        private readonly IMultiplayerSession muliplayerSession;
        private readonly IPacketSender packetSender;
        private readonly Dictionary<NitroxId, SimulationLockType> simulatedIdsByLockType = new Dictionary<NitroxId, SimulationLockType>();
        private readonly Dictionary<NitroxId, LockRequestCompleted> completeFunctionsById = new Dictionary<NitroxId, LockRequestCompleted>();
        
        public SimulationOwnership(IMultiplayerSession muliplayerSession, IPacketSender packetSender)
        {
            this.muliplayerSession = muliplayerSession;
            this.packetSender = packetSender;
        }

        public bool HasAnyLockType(NitroxId id)
        {
            return simulatedIdsByLockType.ContainsKey(id);
        }

        public bool HasExclusiveLock(NitroxId id)
        {
            SimulationLockType activeLockType;

            if (simulatedIdsByLockType.TryGetValue(id, out activeLockType))
            {
                return (activeLockType == SimulationLockType.EXCLUSIVE);
            }

            return false;
        }

        public void RequestSimulationLock(NitroxId id, SimulationLockType lockType, LockRequestCompleted whenCompleted)
        {
            SimulationOwnershipRequest ownershipRequest = new SimulationOwnershipRequest(muliplayerSession.Reservation.PlayerId, id, lockType);
            packetSender.Send(ownershipRequest);
            completeFunctionsById[id] = whenCompleted;
        }

        public void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired, SimulationLockType lockType)
        {
            Log.Instance.LogMessage(LogCategory.Info, "Received lock response, id: " + id + " " + lockAquired + " " + lockType);

            if (lockAquired)
            {
                SimulateEntity(id, lockType);
            }

            LockRequestCompleted requestCompleted = null;

            if (completeFunctionsById.TryGetValue(id, out requestCompleted) && requestCompleted != null)
            {
                completeFunctionsById.Remove(id);
                requestCompleted(id, lockAquired);
            }
            else
            {
                Log.Instance.LogMessage(LogCategory.Warn, "Did not have an outstanding simulation request for " + id + " maybe there were multiple outstanding requests?");
            }
        }

        public void SimulateEntity(NitroxId id, SimulationLockType lockType)
        {
            simulatedIdsByLockType[id] = lockType;
        }

        public void StopSimulatingEntity(NitroxId id)
        {
            simulatedIdsByLockType.Remove(id);
        }
    }
}
