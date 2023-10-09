using System;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Bases;

[Serializable, DataContract]
public record struct NitroxBaseFace(NitroxInt3 Cell, int Direction)
{
    [DataMember(Order = 1)]
    public NitroxInt3 Cell = Cell;

    [DataMember(Order = 2)]
    public int Direction = Direction;

    public override readonly string ToString()
    {
        return $"NitroxBaseFace ({Cell}) {Direction}";
    }
}
