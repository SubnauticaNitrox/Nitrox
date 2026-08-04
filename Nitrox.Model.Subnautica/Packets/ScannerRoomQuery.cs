using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class ScannerRoomQuery(
    NitroxId mapRoomId,
    uint requestId,
    float reportedRange,
    NitroxTechType? selectedTechType,
    ulong knownRevision,
    NitroxVector3? observedOrigin) : Packet
{
    public NitroxId MapRoomId { get; } = mapRoomId;
    public uint RequestId { get; } = requestId;
    public float ReportedRange { get; } = reportedRange;
    public NitroxTechType? SelectedTechType { get; } = selectedTechType;
    public ulong KnownRevision { get; } = knownRevision;
    public NitroxVector3? ObservedOrigin { get; } = observedOrigin;
}
