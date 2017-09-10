using NitroxClient.Communication;
using NitroxClient.Map;
using NitroxModel.Packets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Chunks
    {
        private PacketSender packetSender;
        private LoadedChunks loadedChunks;
        private ChunkAwarePacketReceiver chunkAwarePacketReceiver;

        private bool chunksPendingSync = false;
        private float timeWhenChunksBecameOutOfSync;

        private HashSet<Int3> added = new HashSet<Int3>();
        private HashSet<Int3> removed = new HashSet<Int3>();

        public Chunks(PacketSender packetSender, LoadedChunks loadedChunks, ChunkAwarePacketReceiver chunkAwarePacketReceiver)
        {
            this.packetSender = packetSender;
            this.loadedChunks = loadedChunks;
            this.chunkAwarePacketReceiver = chunkAwarePacketReceiver;
        }
        
        public void AddChunk(Vector3 chunk, MonoBehaviour mb)
        {
            if (chunk != null && loadedChunks != null && mb != null)
            {
                Int3 owningChunk = new Int3((int)chunk.x, (int)chunk.y, (int)chunk.z);
                mb.StartCoroutine(WaitAndAddChunk(owningChunk, mb));
                markChunksReadyForSync(mb, 0.5f);                
            }
        }
        
        private IEnumerator WaitAndAddChunk(Int3 owningChunk, MonoBehaviour mb)
        {
            yield return new WaitForSeconds(0.5f);

            if (!loadedChunks.Contains(owningChunk))
            {
                loadedChunks.Add(owningChunk);
                added.Add(owningChunk);
                chunkAwarePacketReceiver.ChunkLoaded(owningChunk);
            }
        }

        public void RemoveChunk(VoxelandChunk chunk, MonoBehaviour mb)
        {
            if (chunk?.transform != null && loadedChunks != null)
            {
                Int3 owningChunk = new Int3((int)chunk.transform.position.x, (int)chunk.transform.position.y, (int)chunk.transform.position.z);

                if (loadedChunks.Contains(owningChunk))
                {
                    loadedChunks.Remove(owningChunk);
                    removed.Add(owningChunk);
                    markChunksReadyForSync(mb, 0);
                }
            }
        }

        private void markChunksReadyForSync(MonoBehaviour mb, float delay)
        {
            if (chunksPendingSync == false)
            {
                timeWhenChunksBecameOutOfSync = Time.time;
                mb.StartCoroutine(WaitAndSyncChunks(delay));
                chunksPendingSync = true;
            }
        }

        private IEnumerator WaitAndSyncChunks(float delay)
        {
            yield return new WaitForSeconds(delay);

            while (chunksPendingSync)
            {
                float currentTime = Time.time;
                float elapsed = currentTime - timeWhenChunksBecameOutOfSync;

                if (elapsed >= 0.1)
                {
                    VisibleChunksChanged chunksChanged = new VisibleChunksChanged(packetSender.PlayerId, added, removed);
                    packetSender.Send(chunksChanged);

                    added.Clear();
                    removed.Clear();

                    chunksPendingSync = false;
                    yield break;
                }
                
                yield return new WaitForSeconds(0.05f);
            }
        }
        
    }
}
