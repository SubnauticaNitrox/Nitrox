using NitroxClient.MonoBehaviours;
using NitroxModel.Networking.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class SeaTreaderChunkPickedUpProcessor : IClientPacketProcessor<SeaTreaderChunkPickedUp>
{
    public Task Process(IPacketProcessContext context, SeaTreaderChunkPickedUp packet)
    {
        if (NitroxEntity.TryGetComponentFrom(packet.ChunkId, out SinkingGroundChunk sinkingGroundChunk))
        {
            GameObject.Destroy(sinkingGroundChunk.gameObject);
        }

        return Task.CompletedTask;
    }
}
