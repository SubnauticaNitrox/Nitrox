using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

sealed class SeaTreaderSpawnedChunkProcessor(
    IPacketSender packetSender,
    PlayerManager playerManager,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaTreaderSpawnedChunk>(packetSender, playerManager, entityRegistry)
{
    public override void Process(SeaTreaderSpawnedChunk packet, Player sender) => TransmitIfCanSeeEntities(packet, sender, [packet.CreatureId]);
}
