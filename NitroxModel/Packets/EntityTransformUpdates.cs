using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using UnityEngine;

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

        public void AddUpdate(NitroxId id, Vector3 position, Quaternion rotation)
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
            public Vector3 Position { get; }
            public Quaternion Rotation { get; }

            public EntityTransformUpdate(NitroxId id, Vector3 position, Quaternion rotation)
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
