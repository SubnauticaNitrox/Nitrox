using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    class VehicleDockingProcessor : AuthenticatedPacketProcessor<VehicleDocking>
    {
        private readonly PlayerManager playerManager;
        private readonly EntityRegistry entityRegistry;

        public VehicleDockingProcessor(PlayerManager playerManager, EntityRegistry entityRegistry)
        {
            this.playerManager = playerManager;
            this.entityRegistry = entityRegistry;
        }

        public override void Process(VehicleDocking packet, Player player)
        {
            Optional<Entity> vehicle = entityRegistry.GetEntityById(packet.VehicleId);

            if (!vehicle.HasValue)
            {
                Log.Error($"Unable to find vehicle to dock {packet.VehicleId}");
                return;
            }

            Optional<Entity> dock = entityRegistry.GetEntityById(packet.DockId);

            if (!dock.HasValue)
            {
                Log.Error($"Unable to find dock {packet.DockId} for docking vehicle {packet.VehicleId}");
                return;
            }

            vehicle.Value.ParentId = packet.DockId;
            dock.Value.ChildEntities.Add(vehicle.Value);

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
