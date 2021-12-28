using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class EntityTransformUpdates : Packet
    {
        [Index(0)]
        public virtual List<EntityTransformUpdate> Updates { get; protected set; }

        public EntityTransformUpdates()
        {
            Updates = new List<EntityTransformUpdate>();
        }

        public EntityTransformUpdates(List<EntityTransformUpdate> updates)
        {
            Updates = updates;
        }

        public void AddUpdate(NitroxId id, NitroxVector3 position, NitroxQuaternion rotation)
        {
            Updates.Add(new EntityTransformUpdate(id, position, rotation));
        }

        [ZeroFormattable]
        public class EntityTransformUpdate
        {
            [Index(0)]
            public virtual NitroxId Id { get; protected set; }
            [Index(1)]
            public virtual NitroxVector3 Position { get; protected set; }
            [Index(2)]
            public virtual NitroxQuaternion Rotation { get; protected set; }

            public EntityTransformUpdate() { }

            public EntityTransformUpdate(NitroxId id, NitroxVector3 position, NitroxQuaternion rotation)
            {
                Id = id;
                Position = position;
                Rotation = rotation;
            }
        }
    }
}
