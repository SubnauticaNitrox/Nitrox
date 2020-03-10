using System;
using ProtoBufNet;
using UnityEngine;

namespace NitroxModel.DataStructures
{
    /*
     * Int3 model to allow NitroxModel to be decoupled from Assembly-csharp-firstpass
     * as it is common between subnautica and below zero.
     */
    [Serializable]
    [ProtoContract]
    public class Int3
    {
        [ProtoMember(1)]
        public int X { get; set; }

        [ProtoMember(2)]
        public int Y { get; set; }

        [ProtoMember(3)]
        public int Z { get; set; }

        public Int3()
        {
            // For serialization purposes
        }

        public Int3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return "[Int3 - {" + X + ", " + Y + ", " + Z + "}]";
        }

        public Vector3 ToVector3()
        {
            return new Vector3((float)X, (float)Y, (float)Z);
        }

        public override bool Equals(object obj)
        {
            Int3 int3 = obj as Int3;

            return !ReferenceEquals(int3, null) &&
                   X == int3.X &&
                   Y == int3.Y &&
                   Z == int3.Z;
        }

        public override int GetHashCode()
        {
            int hashCode = -307843816;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();

            return hashCode;
        }

        public static Int3 Floor(float x, float y, float z)
        {
            return new Int3(Convert.ToInt32(Math.Floor(x)),
                            Convert.ToInt32(Math.Floor(y)), 
                            Convert.ToInt32(Math.Floor(z)));
        }

        public static Int3 Floor(Vector3 vector)
        {
            return Floor(vector.x, vector.y, vector.z);
        }

        public static Int3 Ceil(float x, float y, float z)
        {
            return new Int3(Convert.ToInt32(Math.Ceiling(x)),
                            Convert.ToInt32(Math.Ceiling(y)),
                            Convert.ToInt32(Math.Ceiling(z)));
        }

        public static Int3 Ceil(Vector3 vector)
        {
            return Ceil(vector.x, vector.y, vector.z);
        }

        public static Int3 operator <<(Int3 u, int s)
        {
            return new Int3(u.X << s, u.Y << s, u.Z << s);
        }

        public static Int3 operator >>(Int3 u, int s)
        {
            return new Int3(u.X >> s, u.Y >> s, u.Z >> s);
        }

        public static bool operator ==(Int3 u, Int3 v)
        {
            return u.Equals(v);
        }

        public static bool operator !=(Int3 u, Int3 v)
        {
            return !u.Equals(v);
        }

        public static Int3 operator +(Int3 u, Int3 v)
        {
            return new Int3(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        }

        public static Int3 operator +(Int3 u, int s)
        {
            return new Int3(u.X + s, u.Y + s, u.Z + s);
        }

        public static Vector3 operator +(Int3 u, Vector3 v)
        {
            return new Vector3((float)u.X + v.x, (float)u.Y + v.y, (float)u.Z + v.z);
        }

        public static Int3 operator -(Int3 u, Int3 v)
        {
            return new Int3(u.X - v.X, u.Y - v.Y, u.Z - v.Z);
        }

        public static Int3 operator -(Int3 u, int s)
        {
            return new Int3(u.X - s, u.Y - s, u.Z - s);
        }

        public static Int3 operator *(Int3 u, Int3 v)
        {
            return new Int3(u.X * v.X, u.Y * v.Y, u.Z * v.Z);
        }

        public static Int3 operator *(Int3 u, int s)
        {
            return new Int3(u.X * s, u.Y * s, u.Z * s);
        }

        public static Int3 operator /(Int3 u, Int3 v)
        {
            return new Int3(u.X / v.X, u.Y / v.Y, u.Z / v.Z);
        }

        public static Int3 operator /(Int3 u, int s)
        {
            return new Int3(u.X / s, u.Y / s, u.Z / s);
        }

        public static Int3 operator %(Int3 u, int s)
        {
            return new Int3(u.X % s, u.Y % s, u.Z % s);
        }

    }
}
