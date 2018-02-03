using System;
using System.Collections.Generic;
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

        public void AddUpdate(string guid, Vector3 position, Quaternion rotation)
        {
            Updates.Add(new EntityTransformUpdate(guid, position, rotation));
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
            public string Guid { get; }
            public Vector3 Position { get; }
            public Quaternion Rotation { get; }

            public EntityTransformUpdate(string guid, Vector3 position, Quaternion rotation)
            {
                Guid = guid;
                Position = position;
                Rotation = rotation;
            }

            public override string ToString()
            {
                return "(" + Guid + " " + Position + " " + Rotation + ")";
            }
        }
    }
}
