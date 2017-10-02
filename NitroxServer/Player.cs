using NitroxModel.DataStructures;
using NitroxModel.Packets.Processors.Abstract;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxServer
{
    public class Player : IProcessorContext
    {
        public String Id { get; }
        public Vector3 Position { get; set; }

        private HashSet<Chunk> visibleChunks;

        public Player(String id)
        {
            this.Id = id;
            this.visibleChunks = new HashSet<Chunk>();
        }

        public void AddChunks(List<Chunk> chunks)
        {
            lock(visibleChunks)
            {
                foreach(Chunk chunk in chunks)
                {
                    visibleChunks.Add(chunk);
                }
            }
        }

        public void RemoveChunks(List<Chunk> chunks)
        {
            lock (visibleChunks)
            {
                foreach (Chunk chunk in chunks)
                {
                    visibleChunks.Remove(chunk);
                }
            }
        }

        public bool HasChunkLoaded(Chunk chunk)
        {
            lock (visibleChunks)
            {
                return visibleChunks.Contains(chunk);
            }
        }
    }
}
