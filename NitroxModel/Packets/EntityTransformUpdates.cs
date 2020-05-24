using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EntityTransformUpdates : Packet
    {
        public List<EntityTransformUpdate> Updates { get; }

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

        public override string ToString()
        {
            string toString = "";

            foreach (EntityTransformUpdate update in Updates)
            {
                toString += update + " ";
            }

            return "[EntityTransformUpdates - Updates: " + toString + "]";
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

            public override string ToString()
            {
                return "(" + Id + " " + Position + " " + Rotation + ")";
            }
        }
    }
}
