using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class ItemPosition : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual NitroxVector3 Position { get; protected set; }
        [Index(2)]
        public virtual NitroxQuaternion Rotation { get; protected set; }

        private ItemPosition() { }

        public ItemPosition(NitroxId id, NitroxVector3 position, NitroxQuaternion rotation)
        {
            Id = id;
            Position = position;
            Rotation = rotation;
        }
    }
}
