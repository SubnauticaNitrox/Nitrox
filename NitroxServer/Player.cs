using NitroxModel.DataStructures;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer.Communication.Packets.Processors.WorldSending;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NitroxServer
{
    public class Player : IProcessorContext
    {
        public String Id { get; private set; }
        public Vector3 Position { get; set; }
        public Queue<Chunk> chunkQueues { get; private set; }

        public Player(String id)
        {
            this.Id = id;
            this.chunkQueues = new Queue<Chunk>();
        }
    }
}
