using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class PlayerSeeOutOfCellEntityProcessor : AuthenticatedPacketProcessor<PlayerSeeOutOfCellEntity>
{
    private readonly EntityRegistry entityRegistry;

    public PlayerSeeOutOfCellEntityProcessor(EntityRegistry entityRegistry)
    {
        this.entityRegistry = entityRegistry;
    }

    public override void Process(PlayerSeeOutOfCellEntity packet, Player player)
    {
        if (entityRegistry.GetEntityById(packet.EntityId).HasValue)
        {
            player.OutOfCellVisibleEntities.Add(packet.EntityId);
        }
    }
}
