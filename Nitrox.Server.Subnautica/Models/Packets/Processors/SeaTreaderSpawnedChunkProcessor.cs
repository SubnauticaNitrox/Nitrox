using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

sealed class SeaTreaderSpawnedChunkProcessor(
    PlayerService playerService,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaTreaderSpawnedChunk>(playerService, entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, SeaTreaderSpawnedChunk packet) => await TransmitIfCanSeeEntitiesAsync(context, packet, [packet.CreatureId]);
}
