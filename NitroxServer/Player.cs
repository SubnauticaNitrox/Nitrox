using NitroxModel.DataStructures;
using NitroxModel.Packets.Processors.Abstract;
using System;

namespace NitroxServer
{
    public class Player : IProcessorContext
    {
        public String Id { get; private set; }
        public Vector3 Position { get; set; }

        public Player(String id)
        {
            this.Id = id;
        }
    }
}
