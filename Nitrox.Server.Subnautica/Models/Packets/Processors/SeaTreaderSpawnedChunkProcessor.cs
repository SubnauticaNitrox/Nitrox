using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

sealed class SeaTreaderSpawnedChunkProcessor(
    PlayerManager playerManager,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaTreaderSpawnedChunk>(playerManager, entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, SeaTreaderSpawnedChunk packet) => await TransmitIfCanSeeEntitiesAsync(context, packet, [packet.CreatureId]);
}
