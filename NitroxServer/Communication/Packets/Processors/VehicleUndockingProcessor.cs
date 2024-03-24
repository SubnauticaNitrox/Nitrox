using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxServer.Communication.Packets.Processors;

public class VehicleUndockingProcessor : AuthenticatedPacketProcessor<VehicleUndocking>
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
        if (packet.UndockingStart)
        {
            if (!entityRegistry.TryGetEntityById(packet.VehicleId, out Entity vehicleEntity))
            {
                PrintStatusCode(StatusCode.INVALID_PACKET, $"Unable to find vehicle to undock {packet.VehicleId}");
                return;
            }

            if (!entityRegistry.GetEntityById(vehicleEntity.ParentId).HasValue)
            {
                PrintStatusCode(StatusCode.INVALID_PACKET, $"Unable to find docked vehicles parent {vehicleEntity.ParentId} to undock from");
                return;
            }

            entityRegistry.RemoveFromParent(vehicleEntity);
        }

        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
