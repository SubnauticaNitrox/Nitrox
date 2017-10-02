using NitroxClient.Communication;
using NitroxClient.Map;
using NitroxModel.DataStructures;
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

        private List<Chunk> added = new List<Chunk>();
        private List<Chunk> removed = new List<Chunk>();

        public Chunks(PacketSender packetSender, LoadedChunks loadedChunks, ChunkAwarePacketReceiver chunkAwarePacketReceiver)
        {
            this.packetSender = packetSender;
            this.loadedChunks = loadedChunks;
            this.chunkAwarePacketReceiver = chunkAwarePacketReceiver;
        }

        public void ChunkLoaded(Int3 batchId, int level)
        {
            LargeWorldStreamer.main.StartCoroutine(WaitAndAddChunk(batchId, level));
            markChunksReadyForSync(0.5f);  
        }
        
        private IEnumerator WaitAndAddChunk(Int3 batchId, int level)
        {
            yield return new WaitForSeconds(0.5f);
            
            Chunk chunk = new Chunk(batchId, level);

            if (!loadedChunks.Contains(chunk))
            {
                loadedChunks.Add(chunk);
                added.Add(chunk);
                chunkAwarePacketReceiver.ChunkLoaded(chunk);
            }            
        }

        public void ChunkUnloaded(Int3 batchId, int level)
        {
            Chunk chunk = new Chunk(batchId, level);

            if (loadedChunks.Contains(chunk))
            {
                loadedChunks.Remove(chunk);
                removed.Add(chunk);
                markChunksReadyForSync(0);
            }     
        }

        private void markChunksReadyForSync(float delay)
        {
            if (chunksPendingSync == false)
            {
                timeWhenChunksBecameOutOfSync = Time.time;
                LargeWorldStreamer.main.StartCoroutine(WaitAndSyncChunks(delay));
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
                    VisibleChunksChanged chunksChanged = new VisibleChunksChanged(packetSender.PlayerId, added.ToArray(), removed.ToArray());
                    packetSender.Send(chunksChanged);
                    Console.WriteLine(chunksChanged);

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
