using System.Collections;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.InitialSync;

class SimulationOwnershipInitialSyncProcessor : InitialSyncProcessor
{
    private readonly SimulationOwnership simulationOwnership;

    public SimulationOwnershipInitialSyncProcessor(SimulationOwnership simulationOwnership)
    {
        this.simulationOwnership = simulationOwnership;

        AddDependency<GlobalRootInitialSyncProcessor>();
    }

    public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
    {
        int idsSynced = 0;
        foreach (NitroxId entityId in packet.InitialSimulationOwnerships)
        {
            // Initial locks are transient
            simulationOwnership.SimulateEntity(entityId, SimulationLockType.TRANSIENT);
            Log.Debug($"Transient simulation ownership for {entityId} from initial sync");

            if (idsSynced++ % 5 == 0)
            {
                waitScreenItem.SetProgress(idsSynced, packet.InitialSimulationOwnerships.Count);
                yield return null;
            }
        }

        yield return null;
    }
}
