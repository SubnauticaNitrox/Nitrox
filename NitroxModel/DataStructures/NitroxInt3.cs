using System;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxModel.DataStructures
{
    /*
     * NitroxInt3 allows NitroxModel to be decoupled from Assembly-csharp-firstpass
     * as it is common code between subnautica and below zero
     */
    [Serializable]
    [ProtoContract]
    public class NitroxInt3
    {
        [ProtoMember(1)]
        public int X { get; set; }

        [ProtoMember(2)]
        public int Y { get; set; }

        [ProtoMember(3)]
        public int Z { get; set; }

        public NitroxInt3()
        {
            // For serialization purposes
        }

        public NitroxInt3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return "[Int3 - {" + X + ", " + Y + ", " + Z + "}]";
        }

        public override bool Equals(object obj)
        {
            NitroxInt3 int3 = obj as NitroxInt3;

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

        public static NitroxInt3 Floor(float x, float y, float z)
        {
            return new NitroxInt3(Convert.ToInt32(Math.Floor(x)),
                            Convert.ToInt32(Math.Floor(y)), 
                            Convert.ToInt32(Math.Floor(z)));
        }

        public static NitroxInt3 Floor(NitroxVector3 vector)
        {
            return Floor(vector.X, vector.Y, vector.Z);
        }

        public static NitroxInt3 Ceil(float x, float y, float z)
        {
            return new NitroxInt3(Convert.ToInt32(Math.Ceiling(x)),
                            Convert.ToInt32(Math.Ceiling(y)),
                            Convert.ToInt32(Math.Ceiling(z)));
        }

        public static NitroxInt3 Ceil(NitroxVector3 vector)
        {
            return Ceil(vector.X, vector.Y, vector.Z);
        }

        public static NitroxInt3 operator <<(NitroxInt3 u, int s)
        {
            return new NitroxInt3(u.X << s, u.Y << s, u.Z << s);
        }

        public static NitroxInt3 operator >>(NitroxInt3 u, int s)
        {
            return new NitroxInt3(u.X >> s, u.Y >> s, u.Z >> s);
        }

        public static bool operator ==(NitroxInt3 u, NitroxInt3 v)
        {
            return u.Equals(v);
        }

        public static bool operator !=(NitroxInt3 u, NitroxInt3 v)
        {
            return !u.Equals(v);
        }

        public static NitroxInt3 operator +(NitroxInt3 u, NitroxInt3 v)
        {
            return new NitroxInt3(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        }

        public static NitroxInt3 operator +(NitroxInt3 u, int s)
        {
            return new NitroxInt3(u.X + s, u.Y + s, u.Z + s);
        }

        public static NitroxVector3 operator +(NitroxInt3 u, NitroxVector3 v)
        {
            return new NitroxVector3(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        }
        
        public static implicit operator NitroxVector3(NitroxInt3 v)
        {
            return new NitroxVector3(v.X, v.Y, v.Z);
        }

        public static NitroxInt3 operator -(NitroxInt3 u, NitroxInt3 v)
        {
            return new NitroxInt3(u.X - v.X, u.Y - v.Y, u.Z - v.Z);
        }

        public static NitroxInt3 operator -(NitroxInt3 u, int s)
        {
            return new NitroxInt3(u.X - s, u.Y - s, u.Z - s);
        }

        public static NitroxInt3 operator *(NitroxInt3 u, NitroxInt3 v)
        {
            return new NitroxInt3(u.X * v.X, u.Y * v.Y, u.Z * v.Z);
        }

        public static NitroxInt3 operator *(NitroxInt3 u, int s)
        {
            return new NitroxInt3(u.X * s, u.Y * s, u.Z * s);
        }

        public static NitroxInt3 operator /(NitroxInt3 u, NitroxInt3 v)
        {
            return new NitroxInt3(u.X / v.X, u.Y / v.Y, u.Z / v.Z);
        }

        public static NitroxInt3 operator /(NitroxInt3 u, int s)
        {
            return new NitroxInt3(u.X / s, u.Y / s, u.Z / s);
        }

    }
}
