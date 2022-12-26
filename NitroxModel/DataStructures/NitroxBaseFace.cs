using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures;

[Serializable, ProtoContract]
public struct NitroxBaseFace : IEquatable<NitroxBaseFace>
{
    [ProtoMember(1)]
    public NitroxInt3 Cell;

    [ProtoMember(2)]
    public int Direction;

    public NitroxBaseFace(NitroxInt3 cell, int direction)
    {
        Cell = cell;
        Direction = direction;
    }

    public override int GetHashCode()
    {
        int num = 923;
        num = 31 * num + Cell.GetHashCode();
        return 31 * num + Direction.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj is NitroxBaseFace nitroxBaseFace && Equals(nitroxBaseFace);
    }

    public bool Equals(NitroxBaseFace other)
    {
        return Cell == other.Cell && Direction == other.Direction;
    }

    public static bool operator ==(NitroxBaseFace lhs, NitroxBaseFace rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(NitroxBaseFace lhs, NitroxBaseFace rhs)
    {
        return !lhs.Equals(rhs);
    }

    public override string ToString()
    {
        return $"NitroxBaseFace ({Cell}) {Direction}";
    }
}
