using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.Communication.Packets.Processors
{
    class PickupItemPacketProcessor : AuthenticatedPacketProcessor<PickupItem>
    {
        private readonly EntityManager entityManager;
        private readonly PlayerManager playerManager;
        private readonly SimulationOwnershipData simulationOwnershipData;
        private readonly PDAStateData pdaStateData;

        public PickupItemPacketProcessor(EntityManager entityManager, PlayerManager playerManager, SimulationOwnershipData simulationOwnershipData, PDAStateData pdaStateData)
        {
            this.entityManager = entityManager;
            this.playerManager = playerManager;
            this.simulationOwnershipData = simulationOwnershipData;
            this.pdaStateData = pdaStateData;
        }

        public override void Process(PickupItem packet, Player player)
        {
            if (simulationOwnershipData.RevokeOwnerOfId(packet.Id))
            {
                ushort serverId = ushort.MaxValue;
                SimulationOwnershipChange simulationOwnershipChange = new SimulationOwnershipChange(packet.Id, serverId, NitroxModel.DataStructures.SimulationLockType.TRANSIENT);
                playerManager.SendPacketToAllPlayers(simulationOwnershipChange);
            }

            // Need to remove the cached progress entry if the object is fully scanned
            if (pdaStateData.CachedProgress.TryGetValue(packet.TechType, out PDAProgressEntry pdaProgressEntry) && pdaProgressEntry.Entries.ContainsKey(packet.Id))
            {
                pdaProgressEntry.Entries.Remove(packet.Id);
                if (pdaProgressEntry.Entries.Count == 0)
                {
                    pdaStateData.CachedProgress.Remove(packet.TechType);
                }
            }

            entityManager.PickUpEntity(packet.Id);
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
