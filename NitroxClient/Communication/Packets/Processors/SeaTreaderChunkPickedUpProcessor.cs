using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class SeaTreaderChunkPickedUpProcessor : IClientPacketProcessor<SeaTreaderChunkPickedUp>
{
    public Task Process(ClientProcessorContext context, SeaTreaderChunkPickedUp packet)
    {
        if (NitroxEntity.TryGetComponentFrom(packet.ChunkId, out SinkingGroundChunk sinkingGroundChunk))
        {
            Object.Destroy(sinkingGroundChunk.gameObject);
        }
        return Task.CompletedTask;
    }
}
