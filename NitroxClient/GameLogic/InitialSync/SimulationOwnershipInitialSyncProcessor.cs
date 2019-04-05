using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.InitialSync
{
    public class SimulationOwnershipInitialSyncProcessor : InitialSyncProcessor
    {
        public override void Process(InitialPlayerSync packet)
        {
            foreach (SimulationOwnershipChange ownership in packet.SimulationOwnerships)
            {
                NitroxServiceLocator.LocateService<SimulationOwnership>().RemoteSimulationOwnershipChange(ownership);
            }
        }
    }
}
