using System;
using System.Runtime.Serialization;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

[Serializable, DataContract]
public sealed class ScannerResourceTarget(NitroxId entityId, ushort trackerIndex, NitroxTechType techType, NitroxVector3 position)
{
    [DataMember(Order = 1)]
    public NitroxId EntityId { get; } = entityId;

    [DataMember(Order = 2)]
    public ushort TrackerIndex { get; } = trackerIndex;

    [DataMember(Order = 3)]
    public NitroxTechType TechType { get; } = techType;

    [DataMember(Order = 4)]
    public NitroxVector3 Position { get; } = position;
}
