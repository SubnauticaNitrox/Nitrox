using System;
using UnityEngine;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class SerializableTransform
    {
        public SerializableVector3 Position { get; private set; }
        public SerializableVector3 Scale { get; private set; }
        public SerializableQuaternion Rotation { get; private set; }

        public SerializableTransform(SerializableVector3 position, SerializableVector3 scale, SerializableQuaternion rotation)
        {
            this.Position = position;
            this.Scale = scale;
            this.Rotation = rotation;
        }

        public override string ToString()
        {
            return "[Transform Position: " + Position + " Scale: " + Scale + " Rotation: " + Rotation + "]";
        }

        public void setTransform(Transform transform)
        {
            transform.position = Position.ToVector3();
            transform.localScale = Scale.ToVector3();
            transform.rotation = Rotation.ToQuaternion();
        }

        public static SerializableTransform from(Transform transform)
        {
            return new SerializableTransform(SerializableVector3.from(transform.position),
                                             SerializableVector3.from(transform.localScale),
                                             SerializableQuaternion.from(transform.rotation));
        }

    }
}
