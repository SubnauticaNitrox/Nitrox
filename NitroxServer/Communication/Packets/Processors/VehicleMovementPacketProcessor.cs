using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    class VehicleMovementPacketProcessor : AuthenticatedPacketProcessor<VehicleMovement>
    {
        private readonly PlayerManager playerManager;
        private readonly EntityRegistry entityRegistry;

        public VehicleMovementPacketProcessor(PlayerManager playerManager, EntityRegistry entityRegistry)
        {
            this.playerManager = playerManager;
            this.entityRegistry = entityRegistry;
        }

        public override void Process(VehicleMovement packet, Player player)
        {
            Optional<Entity> vehicle = entityRegistry.GetEntityById(packet.VehicleMovementData.Id);

            if (vehicle.HasValue && vehicle.Value is WorldEntity worldVehicle)
            {
                worldVehicle.Transform.Position = packet.VehicleMovementData.Position;
                worldVehicle.Transform.Rotation = packet.VehicleMovementData.Rotation;
            }

            if (player.Id == packet.PlayerId)
            {
                player.Position = packet.VehicleMovementData.DriverPosition ?? packet.Position;
            }

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
