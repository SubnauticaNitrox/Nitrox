using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [ProtoContract]
    [Serializable]
    public struct NitroxVector4
    {
        [ProtoMember(1)]
        public float X { get; set; }

        [ProtoMember(2)]
        public float Y { get; set; }

        [ProtoMember(3)]
        public float Z { get; set; }

        [ProtoMember(4)]
        public float W { get; set; }

        public NitroxVector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public float Magnitude => (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

        public override string ToString()
        {
            return $"[Vector4 - {X}, {Y}, {Z}, {W}]";
        }
    }
}
