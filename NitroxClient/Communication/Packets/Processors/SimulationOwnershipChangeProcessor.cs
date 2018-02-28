using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
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
        private readonly IMultiplayerSession multiplayerSession;
        private readonly SimulationOwnership simulationOwnershipManager;

        public SimulationOwnershipChangeProcessor(IMultiplayerSession multiplayerSession, SimulationOwnership simulationOwnershipManager)
        {
            this.multiplayerSession = multiplayerSession;
            this.simulationOwnershipManager = simulationOwnershipManager;
        }

        public override void Process(SimulationOwnershipChange simulationOwnershipChange)
        {
            foreach (OwnedGuid ownedGuid in simulationOwnershipChange.OwnedGuids)
            {
                if (multiplayerSession.Reservation.PlayerId == ownedGuid.PlayerId && ownedGuid.IsEntity)
                {
                    SimulateEntity(ownedGuid);
                }

                simulationOwnershipManager.AddOwnedGuid(ownedGuid.Guid, ownedGuid.PlayerId);
            }
        }

        private void SimulateEntity(OwnedGuid ownedGuid)
        {
            Optional<GameObject> gameObject = GuidHelper.GetObjectFrom(ownedGuid.Guid);

            if (gameObject.IsPresent())
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
