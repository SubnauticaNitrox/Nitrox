using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
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
    }
}
