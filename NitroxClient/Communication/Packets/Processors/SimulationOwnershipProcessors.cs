using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class SimulationOwnershipChangeProcessor : ClientPacketProcessor<SimulationOwnershipChange>
    {
        public override void Process(SimulationOwnershipChange simulationOwnershipChange)
        {
            NitroxServiceLocator.LocateService<SimulationOwnership>().RemoteSimulationOwnershipChange(simulationOwnershipChange);
        }
    }

    public class SimulationOwnershipReleaseProcessor : ClientPacketProcessor<SimulationOwnershipRelease>
    {
        public override void Process(SimulationOwnershipRelease simulationOwnershipRelease)
        {
            NitroxServiceLocator.LocateService<SimulationOwnership>().RemoteSimulationOwnershipRelease(simulationOwnershipRelease);
        }
    }

    public class SimulationOwnershipResponseProcessor : ClientPacketProcessor<SimulationOwnershipResponse>
    {
        public override void Process(SimulationOwnershipResponse response)
        {
            NitroxServiceLocator.LocateService<SimulationOwnership>().ReceivedSimulationLockResponse(response.Guid, response.LockAquired, response.LockType);
        }
    }
}
