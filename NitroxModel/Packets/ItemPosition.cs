using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ItemPosition : Packet
    {
        public ItemPosition(NitroxId id, Vector3 position, Quaternion rotation)
        {
            Id = id;
            Position = position;
            Rotation = rotation;
        }

        public NitroxId Id { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }

        public override string ToString()
        {
            return "[ItemPosition position: " + Position + " Rotation: " + Rotation + " id: " + Id + "]";
        }
    }
}
