using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeaTreaderSpawnedChunkProcessor(
    GameLogic.EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaTreaderSpawnedChunk>(entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, SeaTreaderSpawnedChunk packet) => await TransmitIfCanSeeEntities(packet, context.Sender.PlayerId, packet.CreatureId);
}
