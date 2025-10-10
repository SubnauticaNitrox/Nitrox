using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.Extensions;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class SeaTreaderSpawnedChunkProcessor : ClientPacketProcessor<SeaTreaderSpawnedChunk>
{
    public override void Process(SeaTreaderSpawnedChunk packet)
    {
        if (NitroxEntity.TryGetComponentFrom(packet.CreatureId, out SeaTreader seaTreader) &&
            seaTreader.TryGetComponentInChildren(out SeaTreaderSounds seaTreaderSounds))
        {
            GameObject chunkObject = GameObjectExtensions.InstantiateWithId(seaTreaderSounds.stepChunkPrefab, packet.ChunkId);
            chunkObject.transform.position = packet.Position.ToUnity();
            chunkObject.transform.rotation = packet.Rotation.ToUnity();
        }
    }
}
