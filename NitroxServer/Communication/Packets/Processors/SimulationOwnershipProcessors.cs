using NitroxModel.Core;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    class SimulationOwnershipRequestProcessor : AuthenticatedPacketProcessor<SimulationOwnershipRequest>
    {
        public override void Process(SimulationOwnershipRequest ownershipRequest, Player player)
        {
            NitroxServiceLocator.LocateService<SimulationOwnershipData>().SimulationOwnershipRequest(ownershipRequest.Guid, player, ownershipRequest.LockType);
        }
    }

    class SimulationOwnershipReleaseProcessor : AuthenticatedPacketProcessor<SimulationOwnershipRelease>
    {
        public override void Process(SimulationOwnershipRelease ownershipRequest, Player player)
        {
            NitroxServiceLocator.LocateService<SimulationOwnershipData>().SimulationOwnershipRelease(ownershipRequest.Guid, player);
        }
    }
}
