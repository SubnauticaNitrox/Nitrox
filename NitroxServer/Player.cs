using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.Packets.Processors.Abstract;
using UnityEngine;

namespace NitroxServer
{
    public class Player : IProcessorContext
    {
        public string Id { get; }
        public Vector3 Position { get; set; }

        private readonly HashSet<Chunk> visibleChunks = new HashSet<Chunk>();

        public Player(string id)
        {
            Id = id;
        }

        public void AddChunks(IEnumerable<Chunk> chunks)
        {
            lock (visibleChunks)
            {
                foreach (Chunk chunk in chunks)
                {
                    visibleChunks.Add(chunk);
                }
            }
        }

        public void RemoveChunks(IEnumerable<Chunk> chunks)
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
