using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class SeaTreaderSpawnedChunkProcessor(
    PlayerManager playerManager,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaTreaderSpawnedChunk>(playerManager, entityRegistry)
{
    public override void Process(SeaTreaderSpawnedChunk packet, Player sender) => TransmitIfCanSeeEntities(packet, sender, [packet.CreatureId]);
}
