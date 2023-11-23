using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    class BasicMovementPacketProcessor : AuthenticatedPacketProcessor<BasicMovement>
    {
        private readonly PlayerManager playerManager;
        private readonly EntityRegistry entityRegistry;

        public BasicMovementPacketProcessor(PlayerManager playerManager, EntityRegistry entityRegistry)
        {
            this.playerManager = playerManager;
            this.entityRegistry = entityRegistry;
        }

        public override void Process(BasicMovement packet, Player player)
        {
            Optional<WorldEntity> entity = entityRegistry.GetEntityById<WorldEntity>(packet.Id);

            if (entity.HasValue)
            {
                entity.Value.Transform.Position = packet.Position;
                entity.Value.Transform.Rotation = packet.Rotation;
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
