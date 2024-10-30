using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class VehicleMovementsPacketProcessor : AuthenticatedPacketProcessor<VehicleMovements>
{
    private readonly PlayerManager playerManager;
    private readonly EntityRegistry entityRegistry;

    public VehicleMovementsPacketProcessor(PlayerManager playerManager, EntityRegistry entityRegistry)
    {
        this.playerManager = playerManager;
        this.entityRegistry = entityRegistry;
    }

    public override void Process(VehicleMovements packet, Player player)
    {
        foreach (MovementData movementData in packet.Data)
        {
            if (entityRegistry.TryGetEntityById(movementData.Id, out WorldEntity worldEntity))
            {
                worldEntity.Transform.Position = movementData.Position;
                worldEntity.Transform.Rotation = movementData.Rotation;
            }
        }

        // TODO: sync driving player movement without adding much more data (maybe have a nullable DriverPosition field in there)
        

        /*if (player.Id == packet.PlayerId)
        {
            player.Position = packet.VehicleMovementData.DriverPosition ?? packet.Position;
        }*/

        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
