using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

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
