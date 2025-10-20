using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class SeaTreaderChunkPickedUpProcessor : ClientPacketProcessor<SeaTreaderChunkPickedUp>
{
    public override void Process(SeaTreaderChunkPickedUp packet)
    {
        if (NitroxEntity.TryGetComponentFrom(packet.ChunkId, out SinkingGroundChunk sinkingGroundChunk))
        {
            GameObject.Destroy(sinkingGroundChunk.gameObject);
        }
    }
}
