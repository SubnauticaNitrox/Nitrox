using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class SeaTreaderSpawnedChunkProcessor : IClientPacketProcessor<SeaTreaderSpawnedChunk>
{
    public Task Process(ClientProcessorContext context, SeaTreaderSpawnedChunk packet)
    {
        if (NitroxEntity.TryGetComponentFrom(packet.CreatureId, out SeaTreader seaTreader) &&
            seaTreader.TryGetComponentInChildren(out SeaTreaderSounds seaTreaderSounds))
        {
            GameObject chunkObject = GameObjectExtensions.InstantiateWithId(seaTreaderSounds.stepChunkPrefab, packet.ChunkId);
            chunkObject.transform.position = packet.Position.ToUnity();
            chunkObject.transform.rotation = packet.Rotation.ToUnity();
        }
        return Task.CompletedTask;
    }
}
