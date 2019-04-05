using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using NitroxModel.DataStructures;
using NitroxModel.Logger;
using System;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.GameLogic
{
    public class SimulationOwnership
    {
        public delegate void LockRequestCompleted(string guid, bool lockAquired);

        public delegate void LockStatusChangedRemote(string guid, bool locked);
        public event LockStatusChangedRemote OnLockStatusChanged;
        private readonly IMultiplayerSession multiplayerSession;

      

        private readonly IPacketSender packetSender;
        private readonly Dictionary<string, SimulationLock> simulatedGuidsByLockType = new Dictionary<string, SimulationLock>();
        private readonly Dictionary<string, LockRequestCompleted> completeFunctionsByGuid = new Dictionary<string, LockRequestCompleted>();
        
        public SimulationOwnership(IMultiplayerSession multiplayerSession, IPacketSender packetSender)
        {
            this.multiplayerSession = multiplayerSession;
            this.packetSender = packetSender;
        }

        public bool HasAnyLockType(string guid)
        {
            return simulatedGuidsByLockType.ContainsKey(guid);
        }
        
        public bool HasExclusiveLock(string guid)
        {

#if TRACE && OWNERSHIP
            NitroxModel.Logger.Log.Debug("HasExclusiveLock - guid: " + guid);
#endif

            SimulationLock activeLockType;

            if (simulatedGuidsByLockType.TryGetValue(guid, out activeLockType))
            {
#if TRACE && OWNERSHIP
                NitroxModel.Logger.Log.Debug("HasExclusiveLock - true");
#endif
                return (activeLockType.LockType == SimulationLockType.EXCLUSIVE);
            }

#if TRACE && OWNERSHIP
            NitroxModel.Logger.Log.Debug("HasExclusiveLock - false");
#endif

            return false;
        }

        internal void RemoteSimulationOwnershipChange(SimulationOwnershipChange simulationOwnershipChange)
        {

#if TRACE && OWNERSHIP
            NitroxModel.Logger.Log.Debug("RemoteSimulationOwnershipChange");
#endif

            foreach (SimulatedEntity simulatedEntity in simulationOwnershipChange.Entities)
            {

#if TRACE && OWNERSHIP
                NitroxModel.Logger.Log.Debug("RemoteSimulationOwnershipChange - guid: " + simulatedEntity.Guid + " playerID: " + simulatedEntity.PlayerId);
#endif

                if (multiplayerSession.Reservation.PlayerId == simulatedEntity.PlayerId)
                {
                    if (simulatedEntity.ChangesPosition)
                    {
                        StartBroadcastingEntityPosition(simulatedEntity.Guid);
                    }

                    SimulateGuid(simulatedEntity.Guid, SimulationLockType.TRANSIENT, simulatedEntity.PlayerId);
                }
                else if (HasAnyLockType(simulatedEntity.Guid)  && simulatedEntity.ChangesPosition)
                {
                    // The server has forcibly removed this lock from the client.  This is generally fine for
                    // transient locks because it is only broadcasting position.  However, exclusive locks may
                    // need additional cleanup (such as a person piloting a vehicle - they need to be kicked out)
                    // We can later add a forcibly removed callback but as of right now we have no use-cases for
                    // forcibly removing an exclusive lock.  Just log it if it happens....

                    if (HasExclusiveLock(simulatedEntity.Guid))
                    {
                        Log.Warn("The server has forcibly revoked an exlusive lock - this may cause undefined behaviour.  GUID: " + simulatedEntity.Guid);
                    }

                    StopSimulatingGuid(simulatedEntity.Guid);
                    EntityPositionBroadcaster.StopWatchingEntity(simulatedEntity.Guid);
                }
                else
                {

#if TRACE && OWNERSHIP
                    NitroxModel.Logger.Log.Debug("RemoteSimulationOwnershipChange - SimulatingGuid");
#endif

                    SimulateGuid(simulatedEntity.Guid, simulatedEntity.LockType, simulatedEntity.PlayerId);
                    if(OnLockStatusChanged != null)
                    {
                        OnLockStatusChanged(simulatedEntity.Guid, true);
                    }
                }
            }
        }

        internal void RemoteSimulationOwnershipRelease(SimulationOwnershipRelease simulationOwnershipRelease)
        {

#if TRACE && OWNERSHIP
            NitroxModel.Logger.Log.Debug("RemoteSimulationOwnershipRelease - guid: " + simulationOwnershipRelease.Guid);
#endif

            StopSimulatingGuid(simulationOwnershipRelease.Guid);
            if (OnLockStatusChanged != null)
            {
                OnLockStatusChanged(simulationOwnershipRelease.Guid, false);
            }

        }

        public bool HasExclusiveLockByRemotePlayer(string guid, out ushort playerID)
        {

#if TRACE && OWNERSHIP
            NitroxModel.Logger.Log.Debug("HasExclusiveLockByRemotePlayer - guid: " + guid);
#endif

            playerID = 0;
            SimulationLock activeLockType;

            if (simulatedGuidsByLockType.TryGetValue(guid, out activeLockType))
            {
                if(activeLockType.LockType == SimulationLockType.EXCLUSIVE)
                {
                    if (activeLockType.PlayerID != multiplayerSession.Reservation.PlayerId)
                    {
                        playerID = activeLockType.PlayerID;

#if TRACE && OWNERSHIP
                        NitroxModel.Logger.Log.Debug("HasExclusiveLockByRemotePlayer - playerID: " + playerID);
#endif

                        return true;
                    }
                }
            }

            return false;
        }

        public bool HasExclusiveLockByLocalPlayer(string guid)
        {

#if TRACE && OWNERSHIP
            NitroxModel.Logger.Log.Debug("HasExclusiveLockByMe - guid: " + guid);
#endif

            SimulationLock activeLockType;

            if (simulatedGuidsByLockType.TryGetValue(guid, out activeLockType))
            {
                if (activeLockType.LockType == SimulationLockType.EXCLUSIVE)
                {
                    if (activeLockType.PlayerID == multiplayerSession.Reservation.PlayerId)
                    {

#if TRACE && OWNERSHIP
                        NitroxModel.Logger.Log.Debug("HasExclusiveLockByMe - playerID: " + playerID);
#endif

                        return true;
                    }
                }
            }

            return false;
        }

        public void RequestSimulationLock(string guid, SimulationLockType lockType, LockRequestCompleted whenCompleted)
        {

#if TRACE && OWNERSHIP
            NitroxModel.Logger.Log.Debug("RequestSimulationLock - guid: " + guid + " locktype: " + lockType);
#endif

            SimulationOwnershipRequest ownershipRequest = new SimulationOwnershipRequest(multiplayerSession.Reservation.PlayerId, guid, lockType);
            packetSender.Send(ownershipRequest);
            completeFunctionsByGuid[guid] = whenCompleted;
        }

        internal void ReleaseSimulationLock(string guid)
        {

#if TRACE && OWNERSHIP
            NitroxModel.Logger.Log.Debug("ReleaseSimulationLock - guid: " + guid);
#endif

            StopSimulatingGuid(guid);
            packetSender.Send(new SimulationOwnershipRelease(multiplayerSession.Reservation.PlayerId, guid));
        }

        public void ReceivedSimulationLockResponse(string guid, bool lockAquired, SimulationLockType lockType)
        {

#if TRACE && OWNERSHIP
            NitroxModel.Logger.Log.Debug("ReceivedSimulationLockResponse - guid: " + guid + " lockAquired: " + lockAquired + " lockType: " + lockType);
#endif

            /*
            * For now, we expect the simulation lock callback to setup entity broadcasting as
            * most items that are requesting an exclusive lock have custom broadcast code, ex:
            * vehicles like the cyclops.  However, we may one day want to add a watcher here
            * to ensure broadcast one day, ex:
            * 
            * EntityPositionBroadcaster.WatchEntity(simulatedEntity.Guid, gameObject.Get());
            * 
            */

            Log.Info("Received lock response, guid: " + guid + " " + lockAquired + " " + lockType);

            if (lockAquired)
            {
                SimulateGuid(guid, lockType, multiplayerSession.Reservation.PlayerId);
            }

            LockRequestCompleted requestCompleted = null;

            if (completeFunctionsByGuid.TryGetValue(guid, out requestCompleted) && requestCompleted != null)
            {
                completeFunctionsByGuid.Remove(guid);
                requestCompleted(guid, lockAquired);
            }
            else
            {
                Log.Warn("Did not have an outstanding simulation request for " + guid + " maybe there were multiple outstanding requests?");
            }
        }

        public void SimulateGuid(string guid, SimulationLockType lockType, ushort playerID)
        {

#if TRACE && OWNERSHIP
            NitroxModel.Logger.Log.Debug("SimulateGuid - guid: " + guid + " lockType: " + lockType + " playerID: " + playerID);
#endif

            simulatedGuidsByLockType[guid] = new SimulationLock() { LockType = lockType, PlayerID = playerID };
        }

        public void StopSimulatingGuid(string guid)
        {

#if TRACE && OWNERSHIP
            NitroxModel.Logger.Log.Debug("StopSimulatingGuid - guid: " + guid );
#endif

            simulatedGuidsByLockType.Remove(guid);


        }

        private void StartBroadcastingEntityPosition(string guid)
        {
            Optional<GameObject> gameObject = GuidHelper.GetObjectFrom(guid);

            if (gameObject.IsPresent())
            {
                EntityPositionBroadcaster.WatchEntity(guid, gameObject.Get());
            }
            else
            {
                Log.Error("Expected to simulate an unknown entity: " + guid);
            }
        }



        internal class SimulationLock
        {
            internal SimulationLockType LockType = SimulationLockType.TRANSIENT;
            internal ushort PlayerID = 0;
        }

    }
}
