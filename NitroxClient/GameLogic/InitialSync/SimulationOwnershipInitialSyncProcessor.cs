using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

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
            foreach (NitroxId entityId in packet.InitialSimulationOwnerships)
            {
                waitScreenItem.SetProgress(idsSynced++, packet.InitialSimulationOwnerships.Count);
                // Initial locks are transient
                simulationOwnership.SimulateEntity(entityId, SimulationLockType.TRANSIENT);
                Log.Debug($"Transient simulation ownership for {entityId} from initial sync");
            }
            yield return null;
        }
    }
}
