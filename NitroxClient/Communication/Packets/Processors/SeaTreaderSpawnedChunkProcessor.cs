using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.Networking.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class SeaTreaderSpawnedChunkProcessor : IClientPacketProcessor<SeaTreaderSpawnedChunk>
{
    public Task Process(IPacketProcessContext context, SeaTreaderSpawnedChunk packet)
    {
        if (NitroxEntity.TryGetComponentFrom(packet.CreatureId, out SeaTreader seaTreader) &&
            seaTreader.TryGetComponentInChildren(out SeaTreaderSounds seaTreaderSounds))
        {
            GameObject chunkObject = GameObjectHelper.InstantiateWithId(seaTreaderSounds.stepChunkPrefab, packet.ChunkId);
            chunkObject.transform.position = packet.Position.ToUnity();
            chunkObject.transform.rotation = packet.Rotation.ToUnity();
        }

        return Task.CompletedTask;
    }
}
