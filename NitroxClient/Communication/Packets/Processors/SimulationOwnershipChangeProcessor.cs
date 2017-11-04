using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class SimulationOwnershipChangeProcessor : ClientPacketProcessor<SimulationOwnershipChange>
    {
        private readonly PacketSender packetSender;

        public SimulationOwnershipChangeProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(SimulationOwnershipChange simulationOwnershipChange)
        {
            foreach(OwnedGuid ownedGuid in simulationOwnershipChange.OwnedGuids)
            {
                if (packetSender.PlayerId == ownedGuid.PlayerId && ownedGuid.IsEntity)
                {
                    SimulateEntity(ownedGuid);
                }

                Multiplayer.Logic.SimulationOwnership.AddOwnedGuid(ownedGuid.Guid, ownedGuid.PlayerId);
            }
        }

        private void SimulateEntity(OwnedGuid ownedGuid)
        {
            Optional<GameObject> gameObject = GuidHelper.GetObjectFrom(ownedGuid.Guid);

            if(gameObject.IsPresent())
            {
                EntityPositionBroadcaster.WatchEntity(ownedGuid.Guid, gameObject.Get());
            }
            else
            {
                Log.Error("Expected to simulate an unknown entity: " + ownedGuid.Guid);
            }
        }
    }
}
