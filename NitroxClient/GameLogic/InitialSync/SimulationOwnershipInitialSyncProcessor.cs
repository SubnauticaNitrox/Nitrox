using System.Collections;
using NitroxClient.GameLogic.InitialSync.Abstract;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.GameLogic.InitialSync;

public sealed class SimulationOwnershipInitialSyncProcessor : InitialSyncProcessor
{
    private readonly SimulationOwnership simulationOwnership;

    public SimulationOwnershipInitialSyncProcessor(SimulationOwnership simulationOwnership)
    {
        this.simulationOwnership = simulationOwnership;

        AddDependency<GlobalRootInitialSyncProcessor>();
    }

    public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
    {
        int entitiesSynced = 0;
        foreach (SimulatedEntity simulatedEntity in packet.InitialSimulationOwnerships)
        {
            simulationOwnership.TreatSimulatedEntity(simulatedEntity);

            if (entitiesSynced++ % 5 == 0)
            {
                waitScreenItem.SetProgress(entitiesSynced, packet.InitialSimulationOwnerships.Count);
                yield return null;
            }
        }

        yield break;
    }
}
