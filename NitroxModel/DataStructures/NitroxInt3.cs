using System;
using System.Collections;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxModel.DataStructures
{
    /*
     * NitroxInt3 model to allow NitroxModel to be decoupled from Assembly-csharp-firstpass
     * as it is common between subnautica and below zero.
     */
    [Serializable]
    [ProtoContract]
    public struct NitroxInt3 : IEquatable<NitroxInt3>
    {
        [ProtoMember(1)]
        public int X { get; set; }

        [ProtoMember(2)]
        public int Y { get; set; }

        [ProtoMember(3)]
        public int Z { get; set; }

        public NitroxInt3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public NitroxInt3(int v)
        {
            X = v;
            Y = v;
            Z = v;
        }

        public static Bounds CenterSize(NitroxInt3 center, NitroxInt3 size)
        {
            NitroxInt3 min = center - size / 2;
            NitroxInt3 max = min + size - 1;
            return new Bounds(min, max);
        }

        public void Next(NitroxInt3 mins, NitroxInt3 maxs)
        {
            Z++;
            if (Y > maxs.Z)
            {
                Y++;
                if (Y > maxs.Y)
                {
                    X++;
                    Y = mins.Y;
                }
                Z = mins.Z;
            }
        }

        public int ToArrayIndex(int sizeY, int sizeZ)
        {
            return X * (sizeY * sizeZ) + Y * sizeZ + Z;
        }

        public struct Bounds
        {
            NitroxInt3 Mins { get; set; }
            NitroxInt3 Maxs { get; set; }

            public Bounds(NitroxInt3 min, NitroxInt3 max)
            {
                Mins = min;
                Maxs = max;
            }

            public static Bounds FinerBounds(NitroxInt3 coarseCell, int finePerCoarseCell)
            {
                return FinerBounds(new Bounds(coarseCell, coarseCell), new NitroxInt3(finePerCoarseCell));
            }

            public static Bounds FinerBounds(NitroxInt3 coarseCell, NitroxInt3 finePerCoarseCell)
            {
                return FinerBounds(new Bounds(coarseCell, coarseCell), finePerCoarseCell);
            }

            public static Bounds FinerBounds(Bounds coarseBounds, NitroxInt3 finePerCoarseCell)
            {
                return new Bounds(coarseBounds.Mins * finePerCoarseCell, (coarseBounds.Maxs + 1) * finePerCoarseCell - 1);
            }

            public static Bounds InnerCoarserBounds(Bounds fineBounds, NitroxInt3 finePerCoarseCell)
            {
                return new Bounds(CeilDiv(fineBounds.Mins, finePerCoarseCell), FloorDiv(fineBounds.Maxs + 1, finePerCoarseCell) - 1);
            }

            public static Bounds OuterCoarserBounds(Bounds fineBounds, NitroxInt3 finePerCoarseCell)
            {
                return new Bounds(FloorDiv(fineBounds.Mins, finePerCoarseCell), CeilDiv(fineBounds.Maxs + 1, finePerCoarseCell) - 1);
            }

            public Bounds Clamp(NitroxInt3 cmins, NitroxInt3 cmaxs)
            {
                return new Bounds(Mins.Clamp(cmins, cmaxs), Maxs.Clamp(cmins, cmaxs));
            }

            public static Bounds operator -(Bounds b, NitroxInt3 s)
            {
                return new Bounds(b.Mins - s, b.Maxs - s);
            }

            public RangeEnumerator GetRangeEnumerator()
            {
                return new RangeEnumerator(Mins, Maxs);
            }

            public RangeEnumerator GetEnumerator()
            {
                return GetRangeEnumerator();
            }
        }

        public struct RangeEnumerator : IEnumerator<NitroxInt3>, IEnumerator, IDisposable
        {
            object IEnumerator.Current {
                get
                {
                    return current;
                }
            }

            public NitroxInt3 Current {
                get
                {
                    return current;
                }
            }

            private NitroxInt3 current;
            private NitroxInt3 mins;
            private NitroxInt3 maxs;

            public RangeEnumerator(NitroxInt3 mins, NitroxInt3 maxs)
            {
                current = mins;
                this.mins = mins;
                this.maxs = maxs;

                Reset();
            }

            public void Dispose()
            {
                // Nothing to Dispose but Interfaces...
            }

            public bool MoveNext()
            {
                current.Next(mins, maxs);
                return current <= maxs;
            }

            public bool MoveNext(int step)
            {
                for (int i = 0; i < step; i++)
                {
                    current.Next(mins, maxs);
                }
                return current <= maxs;
            }

            public void Reset()
            {
                current = new NitroxInt3(mins.X, mins.Y, mins.Z - 1);
            }
        }

        public override string ToString()
        {
            return $"[NitroxInt3 - {X}, {Y}, {Z}]";
        }

        public bool Equals(NitroxInt3 other)
        {
            return X == other.X
                && Y == other.Y
                && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            return (obj is NitroxInt3 other) && Equals(other);
        }

        public override int GetHashCode()
        {
            int hashCode = -307843816;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();

            return hashCode;
        }

        public static NitroxInt3 Min(NitroxInt3 a, NitroxInt3 b)
        {
            return new NitroxInt3(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
        }

        public static NitroxInt3 Max(NitroxInt3 a, NitroxInt3 b)
        {
            return new NitroxInt3(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
        }

        public NitroxInt3 Clamp(NitroxInt3 mins, NitroxInt3 maxs)
        {
            return Min(maxs, Max(mins, this));
        }

        public static NitroxInt3 PositiveModulo(NitroxInt3 left, NitroxInt3 right)
        {
            return new NitroxInt3(PositiveModulo(left.X, right.X), PositiveModulo(left.Y, right.Y), PositiveModulo(left.Z, right.Z));
        }

        public static int PositiveModulo(int v, int m)
        {
            return (v % m + m) % m;
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

        public static NitroxInt3 FloorDiv(NitroxInt3 left, int right)
        {
            return new NitroxInt3(FloorDiv(left.X, right), FloorDiv(left.Y, right), FloorDiv(left.Z, right));
        }

        public static NitroxInt3 FloorDiv(NitroxInt3 left, NitroxInt3 right)
        {
            return new NitroxInt3(FloorDiv(left.X, right.X), FloorDiv(left.Y, right.Y), FloorDiv(left.Z, right.Z));
        }

        public static int FloorDiv(int a, int b)
        {
            return (a - (a % b + b) % b) / b;
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

        public static int CeilDiv(int a, int b)
        {
            return (a - (a % b - b) % b) / b;
        }

        public static NitroxInt3 CeilDiv(NitroxInt3 left, NitroxInt3 right)
        {
            return new NitroxInt3(CeilDiv(left.X, right.X), CeilDiv(left.Y, right.Y), CeilDiv(left.Z, right.Z));
        }

        public static bool operator ==(NitroxInt3 u, NitroxInt3 v)
        {
            return u.Equals(v);
        }

        public static bool operator !=(NitroxInt3 u, NitroxInt3 v)
        {
            return !u.Equals(v);
        }

        public static NitroxInt3 operator <<(NitroxInt3 u, int s)
        {
            return new NitroxInt3(u.X << s, u.Y << s, u.Z << s);
        }

        public static NitroxInt3 operator >>(NitroxInt3 u, int s)
        {
            return new NitroxInt3(u.X >> s, u.Y >> s, u.Z >> s);
        }

        public static bool operator <(NitroxInt3 u, NitroxInt3 v)
        {
            return u.X < v.X && u.Y < v.Y && u.Z < v.Z;
        }

        public static bool operator >(NitroxInt3 u, NitroxInt3 v)
        {
            return u.X > v.X && u.Y > v.Y && u.Z > v.Z;
        }

        public static bool operator <=(NitroxInt3 u, NitroxInt3 v)
        {
            return u.X <= v.X && u.Y <= v.Y && u.Z <= v.Z;
        }

        public static bool operator >=(NitroxInt3 u, NitroxInt3 v)
        {
            return u.X >= v.X && u.Y >= v.Y && u.Z >= v.Z;
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

        public static explicit operator NitroxInt3(NitroxVector3 v)
        {
            return new NitroxInt3((int)v.X, (int)v.Y, (int)v.Z);
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
