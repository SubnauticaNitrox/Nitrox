using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class Transform
    {
        public Vector3 Position { get; private set; }
        public Vector3 Scale { get; private set; }
        public Quaternion Rotation { get; private set; }

        public Transform(Vector3 position, Vector3 scale, Quaternion rotation)
        {
            this.Position = position;
            this.Scale = scale;
            this.Rotation = rotation;
        }

        public override string ToString()
        {
            return "[Transform Position: " + Position + " Scale: " + Scale + " Rotation: " + Rotation + "]";
        }

    }
}
