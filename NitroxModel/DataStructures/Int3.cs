using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxModel.DataStructures
{
    /*
     * Int3 model to allow NitroxModel to be decoupled from Assembly-csharp-firstpass
     * as it is common between subnautica and below zero.
     */
    [Serializable]
    [ProtoContract]
    public struct Int3 : IEquatable<Int3>, IEqualityComparer<Int3>, IComparable<Int3>
    {
        [ProtoMember(1)]
        public int X { get; set; }

        [ProtoMember(2)]
        public int Y { get; set; }

        [ProtoMember(3)]
        public int Z { get; set; }

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

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Int3))
            {
                return false;
            }
                
            Int3 int3 = (Int3)obj;

            return Equals(int3);
        }
        public bool Equals(Int3 other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z;
        }

        public bool Equals(Int3 x, Int3 y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(Int3 obj)
        {
            return obj.GetHashCode();
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

        public static Int3 Floor(NitroxVector3 vector)
        {
            return Floor(vector.X, vector.Y, vector.Z);
        }

        public static Int3 Ceil(float x, float y, float z)
        {
            return new Int3(Convert.ToInt32(Math.Ceiling(x)),
                            Convert.ToInt32(Math.Ceiling(y)),
                            Convert.ToInt32(Math.Ceiling(z)));
        }

        public static Int3 Ceil(NitroxVector3 vector)
        {
            return Ceil(vector.X, vector.Y, vector.Z);
        }

        public int CompareTo(Int3 other)
        {
            return X.CompareTo(other.X) + Y.CompareTo(other.Y) + Z.CompareTo(other.Z);
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
        public static bool operator <(Int3 u, Int3 v)
        {
            return u.X < v.X && u.Y < v.Y && u.Z < v.Z;
        }
        public static bool operator >(Int3 u, Int3 v)
        {
            return u.X > v.X && u.Y > v.Y && u.Z > v.Z;
        }
        public static bool operator <=(Int3 u, Int3 v)
        {
            return u.X <= v.X && u.Y <= v.Y && u.Z <= v.Z;
        }
        public static bool operator >=(Int3 u, Int3 v)
        {
            return u.X >= v.X && u.Y >= v.Y && u.Z >= v.Z;
        }


        public static Int3 operator +(Int3 u, Int3 v)
        {
            return new Int3(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        }

        public static Int3 operator +(Int3 u, int s)
        {
            return new Int3(u.X + s, u.Y + s, u.Z + s);
        }

        public static NitroxVector3 operator +(Int3 u, NitroxVector3 v)
        {
            return new NitroxVector3(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        }

        public static implicit operator NitroxVector3(Int3 v)
        {
            return new NitroxVector3(v.X, v.Y, v.Z);
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

    }
}
