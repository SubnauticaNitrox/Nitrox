using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class SimulationOwnershipChangeProcessor : ClientPacketProcessor<SimulationOwnershipChange>
    {
        public override void Process(SimulationOwnershipChange simulationOwnershipChange)
        {
            foreach(OwnedGuid ownedGuid in simulationOwnershipChange.OwnedGuids)
            {
                Multiplayer.Logic.SimulationOwnership.AddOwnedGuid(ownedGuid.Guid, ownedGuid.PlayerId);
            }
        }
    }
}
