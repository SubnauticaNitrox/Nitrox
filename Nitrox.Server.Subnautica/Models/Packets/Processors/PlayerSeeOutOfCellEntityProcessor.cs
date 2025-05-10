using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerSeeOutOfCellEntityProcessor(EntityRegistry entityRegistry) : IAuthPacketProcessor<PlayerSeeOutOfCellEntity>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public async Task Process(AuthProcessorContext context, PlayerSeeOutOfCellEntity packet)
    {
        // TODO: USE DATABASE
        // if (entityRegistry.GetEntityById(packet.EntityId).HasValue)
        // {
        //     player.OutOfCellVisibleEntities.Add(packet.EntityId);
        // }
    }
}
