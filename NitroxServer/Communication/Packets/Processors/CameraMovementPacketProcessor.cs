using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    class CameraMovementPacketProcessor : AuthenticatedPacketProcessor<CameraMovement>
    {
        private readonly PlayerManager playerManager;
        private readonly EntityManager entityManager;

        public CameraMovementPacketProcessor(PlayerManager playerManager, EntityManager entityManager)
        {
            this.playerManager = playerManager;
            this.entityManager = entityManager;
        }

        public override void Process(CameraMovement packet, Player player)
        {
            Optional<Entity> entity = entityManager.GetEntityById(packet.CameraMovementData.Id);
            if (entity.HasValue)
            {
                entity.Value.Transform.Position = packet.Position;
                entity.Value.Transform.Rotation = packet.BodyRotation;
            }

            if (player.Id == packet.PlayerId)
            {
                player.Position = packet.Position;
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
