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
        
        public void ChunksLoaded(Int3.Bounds batchBounds)
        {
            LargeWorldStreamer.main.StartCoroutine(WaitAndAddChunk(batchBounds));
            markChunksReadyForSync(0.5f);  
        }
        
        private IEnumerator WaitAndAddChunk(Int3.Bounds batchBounds)
        {
            yield return new WaitForSeconds(0.5f);

            foreach(Int3 batch in batchBounds)
            {
                if (!loadedChunks.Contains(batch))
                {
                    Console.WriteLine("loaded chunk: " + batch);
                    loadedChunks.Add(batch);
                    added.Add(batch);
                    chunkAwarePacketReceiver.ChunkLoaded(batch);
                }
            }
        }

        public void ChunksUnloaded(Int3.Bounds batchBounds)
        {
            foreach (Int3 batch in batchBounds)
            {
                if (loadedChunks.Contains(batch))
                {
                    Console.WriteLine("unloaded chunk: " + batch);
                    loadedChunks.Remove(batch);
                    removed.Add(batch);
                    markChunksReadyForSync(0);
                }
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
