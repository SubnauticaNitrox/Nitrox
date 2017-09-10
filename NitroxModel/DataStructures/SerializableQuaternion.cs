using System;
using UnityEngine;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class SerializableQuaternion
    {
        public float W { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public SerializableQuaternion(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public override string ToString()
        {
            return "[Quaternion - {" + X + ", " + Y + ", " + Z + "," + W + "}]";
        }
        
        public Quaternion ToQuaternion()
        {
            return new Quaternion(X, Y, Z, W);
        }
        
        public static SerializableQuaternion from(Quaternion quaternion)
        {
            return new SerializableQuaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }
    }
}
