using System;
using System.Collections.Generic;
using DTO = NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EntityTransformUpdates : Packet
    {
        public EntityTransformUpdates()
        {
            Updates = new List<EntityTransformUpdate>();
        }

        public EntityTransformUpdates(List<EntityTransformUpdate> updates)
        {
            Updates = updates;
        }

        public List<EntityTransformUpdate> Updates { get; }

        public void AddUpdate(DTO.NitroxId id, DTO.Vector3 position, DTO.Quaternion rotation)
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
            public EntityTransformUpdate(DTO.NitroxId id, DTO.Vector3 position, DTO.Quaternion rotation)
            {
                Id = id;
                Position = position;
                Rotation = rotation;
            }

            public DTO.NitroxId Id { get; }
            public DTO.Vector3 Position { get; }
            public DTO.Quaternion Rotation { get; }

            public override string ToString()
            {
                return "(" + Id + " " + Position + " " + Rotation + ")";
            }
        }
    }
}
