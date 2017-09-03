using System;
using UnityEngine;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class SerializableVector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public SerializableVector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public override string ToString()
        {
            return "[Vector3 - {" + X + ", " + Y + ", " + Z + "}]";
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

        public static SerializableVector3 from(Vector3 vector3)
        {
            return new SerializableVector3(vector3.x, vector3.y, vector3.z);
        }
    }
}
