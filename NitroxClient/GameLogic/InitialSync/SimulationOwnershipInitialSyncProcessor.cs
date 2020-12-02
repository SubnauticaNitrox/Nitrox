using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    class SimulationOwnershipInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly IPacketSender packetSender;
        private readonly SimulationOwnership simulationOwnership;

        public SimulationOwnershipInitialSyncProcessor(IPacketSender packetSender, SimulationOwnership simulationOwnership)
        {
            this.packetSender = packetSender;
            this.simulationOwnership = simulationOwnership;

            DependentProcessors.Add(typeof(BuildingInitialSyncProcessor));
            DependentProcessors.Add(typeof(CyclopsInitialAsyncProcessor));
            DependentProcessors.Add(typeof(VehicleInitialSyncProcessor));
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            int idsSynced = 0;
            foreach (NitroxId ownerId in packet.InitialSimulationOwnerships)
            {
                waitScreenItem.SetProgress(idsSynced++, packet.InitialSimulationOwnerships.Count);
                // Initial locks are transient
                simulationOwnership.SimulateEntity(ownerId, SimulationLockType.TRANSIENT);
                Log.Debug($"Transient simulation ownership for {ownerId} from initial sync");
                yield return null;
            }
        }
    }
}
