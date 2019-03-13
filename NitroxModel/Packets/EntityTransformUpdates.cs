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

        public void AddUpdate(string guid, Transform transform)
        {
            Updates.Add(new EntityTransformUpdate(guid, transform.position, transform.rotation, transform.localScale, transform.localPosition, transform.localRotation));
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
            public Vector3 Scale { get; }
            public Vector3 LocalPosition { get; }
            public Quaternion LocalRotation { get; }

            public EntityTransformUpdate(string guid, Vector3 position, Quaternion rotation, Vector3 scale, Vector3 localPosition, Quaternion localRotation)
            {
                Guid = guid;
                Position = position;
                Rotation = rotation;
                Scale = scale;
                LocalPosition = localPosition;
                LocalRotation = localRotation;
            }

            public override string ToString()
            {
                return "[EntityTransformUpdate (" + Guid + " " + Position + " " + Rotation + " " + Scale + " " + LocalPosition + " " + LocalRotation + ")]";
            }
        }
    }
}
