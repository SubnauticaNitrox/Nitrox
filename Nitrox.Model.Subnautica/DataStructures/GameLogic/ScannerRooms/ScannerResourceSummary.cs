using System;
using System.Runtime.Serialization;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

[Serializable, DataContract]
public sealed class ScannerResourceSummary(NitroxTechType techType, int count)
{
    [DataMember(Order = 1)]
    public NitroxTechType TechType { get; } = techType;

    [DataMember(Order = 2)]
    public int Count { get; } = count;
}
