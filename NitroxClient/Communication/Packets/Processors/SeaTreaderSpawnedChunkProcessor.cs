using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
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
