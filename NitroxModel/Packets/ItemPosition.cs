using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ItemPosition : Packet
    {
        public NitroxId Id { get; }
        public NitroxVector3 Position { get; }
        public NitroxQuaternion Rotation { get; }
        
        public ItemPosition(NitroxId id, NitroxVector3 position, NitroxQuaternion rotation)
        {
            Id = id;
            Position = position;
            Rotation = rotation;
        }

        public override string ToString()
        {
            return "[ItemPosition position: " + Position + " Rotation: " + Rotation + " id: " + Id + "]";
        }
    }
}
