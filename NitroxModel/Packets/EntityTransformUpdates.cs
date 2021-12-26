using System;
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

        [Serializable]
        public class EntityTransformUpdate
        {
            public NitroxId Id { get; }
            public NitroxVector3 Position { get; }
            public NitroxQuaternion Rotation { get; }

            public EntityTransformUpdate(NitroxId id, NitroxVector3 position, NitroxQuaternion rotation)
            {
                Id = id;
                Position = position;
                Rotation = rotation;
            }
        }
    }
}
