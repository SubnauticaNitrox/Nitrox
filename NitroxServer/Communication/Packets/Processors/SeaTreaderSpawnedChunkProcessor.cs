using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

public class SeaTreaderSpawnedChunkProcessor(
    PlayerManager playerManager,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaTreaderSpawnedChunk>(playerManager, entityRegistry)
{
    public override void Process(SeaTreaderSpawnedChunk packet, Player sender) => TransmitIfCanSeeEntities(packet, sender, [packet.CreatureId]);
}
