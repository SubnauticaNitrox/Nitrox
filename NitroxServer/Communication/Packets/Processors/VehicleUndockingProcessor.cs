using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    class VehicleUndockingProcessor : AuthenticatedPacketProcessor<VehicleUndocking>
    {
        private readonly PlayerManager playerManager;
        private readonly EntityRegistry entityRegistry;

        public VehicleUndockingProcessor(PlayerManager playerManager, EntityRegistry entityRegistry)
        {
            this.playerManager = playerManager;
            this.entityRegistry = entityRegistry;
        }

        public override void Process(VehicleUndocking packet, Player player)
        {
            Optional<Entity> vehicle = entityRegistry.GetEntityById(packet.VehicleId);

            if (!vehicle.HasValue)
            {
                Log.Error($"Unable to find vehicle to undock {packet.VehicleId}");
                return;
            }

            Optional<Entity> parent = entityRegistry.GetEntityById(vehicle.Value.ParentId);

            if (!parent.HasValue)
            {
                Log.Error($"Unable to find docked vehicles parent {vehicle.Value.ParentId} to undock from");
                return;
            }

            parent.Value.ChildEntities.Remove(vehicle.Value);

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
