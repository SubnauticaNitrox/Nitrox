using Nitrox.Model.DataStructures.GameLogic.Entities;
using Nitrox.Model.DataStructures.Util;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    class PlayerMovementProcessor : AuthenticatedPacketProcessor<PlayerMovement>
    {
        private readonly PlayerManager playerManager;
        private readonly EntityRegistry entityRegistry;

        public PlayerMovementProcessor(PlayerManager playerManager, EntityRegistry entityRegistry)
        {
            this.playerManager = playerManager;
            this.entityRegistry = entityRegistry;
        }

        public override void Process(PlayerMovement packet, Player player)
        {
            Optional<PlayerEntity> playerEntity = entityRegistry.GetEntityById<PlayerEntity>(player.PlayerContext.PlayerNitroxId);

            if (playerEntity.HasValue)
            {
                playerEntity.Value.Transform.Position = packet.Position;
                playerEntity.Value.Transform.Rotation = packet.BodyRotation;
            }

            player.Position = packet.Position;
            player.Rotation = packet.BodyRotation;
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
