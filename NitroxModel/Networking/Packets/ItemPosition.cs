using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record ItemPosition : Packet
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
    }
}
