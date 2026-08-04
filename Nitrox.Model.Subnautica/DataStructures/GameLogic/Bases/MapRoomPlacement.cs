using System;
using System.Runtime.Serialization;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Bases;

/// <summary>
/// The base-relative cell and world-space scanner origin of a Scanner Room after base geometry is rebuilt.
/// </summary>
[Serializable, DataContract]
public record struct MapRoomPlacement(NitroxInt3 Cell, NitroxVector3? ScanOrigin)
{
    [DataMember(Order = 1)]
    public NitroxInt3 Cell = Cell;

    [DataMember(Order = 2, EmitDefaultValue = false)]
    public NitroxVector3? ScanOrigin = ScanOrigin;
}
