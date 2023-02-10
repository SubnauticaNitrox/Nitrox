using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    class SubRootChangedPacketProcessor : AuthenticatedPacketProcessor<SubRootChanged>
    {
        private readonly PlayerManager playerManager;
        private readonly EntityRegistry entityRegistry;

        public SubRootChangedPacketProcessor(PlayerManager playerManager, EntityRegistry entityRegistry)
        {
            this.playerManager = playerManager;
            this.entityRegistry = entityRegistry;
        }

        public override void Process(SubRootChanged packet, Player player)
        {
            entityRegistry.ReparentEntity(player.GameObjectId, packet.SubRootId.OrNull());
            player.SubRootId = packet.SubRootId;
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
