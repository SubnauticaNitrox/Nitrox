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

        private HashSet<Int3> visibleChunks;

        public Player(String id)
        {
            this.Id = id;
            this.visibleChunks = new HashSet<Int3>();
        }

        public void AddChunks(HashSet<Int3> chunks)
        {
            lock(visibleChunks)
            {
                foreach(Int3 chunk in chunks)
                {
                    visibleChunks.Add(chunk);
                }
            }
        }

        public void RemoveChunks(HashSet<Int3> chunks)
        {
            lock (visibleChunks)
            {
                foreach (Int3 chunk in chunks)
                {
                    visibleChunks.Remove(chunk);
                }
            }
        }

        public bool HasChunkLoaded(Int3 chunk)
        {
            lock (visibleChunks)
            {
                return visibleChunks.Contains(chunk);
            }
        }
    }
}
