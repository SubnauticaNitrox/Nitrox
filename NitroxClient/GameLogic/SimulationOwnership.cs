using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Simulation;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class SimulationOwnership
    {
        private readonly IMultiplayerSession muliplayerSession;
        private readonly IPacketSender packetSender;
        private readonly Dictionary<NitroxId, SimulationLockType> simulatedIdsByLockType = new Dictionary<NitroxId, SimulationLockType>();
        private readonly Dictionary<NitroxId, LockRequestBase> lockRequestsById = new Dictionary<NitroxId, LockRequestBase>();

        public SimulationOwnership(IMultiplayerSession muliplayerSession, IPacketSender packetSender)
        {
            this.muliplayerSession = muliplayerSession;
            this.packetSender = packetSender;
        }
        public bool PlayerHasMinLockType(NitroxId id, SimulationLockType lockType)
        {
            if (simulatedIdsByLockType.TryGetValue(id, out SimulationLockType playerLock))
            {
                return playerLock <= lockType;
            }
            return false;
        }

        public bool HasAnyLockType(NitroxId id)
        {
            return PlayerHasMinLockType(id, SimulationLockType.TRANSIENT);
        }

        public bool HasExclusiveLock(NitroxId id)
        {
            return PlayerHasMinLockType(id, SimulationLockType.EXCLUSIVE);
        }

        public void RequestSimulationLock(NitroxId id, SimulationLockType lockType)
        {
            SimulationOwnershipRequest ownershipRequest = new SimulationOwnershipRequest(muliplayerSession.Reservation.PlayerId, id, lockType);
            packetSender.Send(ownershipRequest);
        }

        public void RequestSimulationLock(LockRequestBase lockRequest)
        {
            lockRequestsById[lockRequest.Id] = lockRequest;
            RequestSimulationLock(lockRequest.Id, lockRequest.LockType);
        }

        public void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired, SimulationLockType lockType)
        {
            Log.Info("Received lock response, id: " + id + " " + lockAquired + " " + lockType);

            if (lockAquired)
            {
                SimulateEntity(id, lockType);
            }

            if (lockRequestsById.TryGetValue(id, out LockRequestBase lockRequest))
            {
                lockRequest.LockRequestComplete(id, lockAquired);
                lockRequestsById.Remove(id);
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
