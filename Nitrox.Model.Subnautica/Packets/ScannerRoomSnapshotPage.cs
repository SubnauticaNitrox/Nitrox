using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class ScannerRoomSnapshotPage(
    NitroxId mapRoomId,
    uint requestId,
    ScannerRoomQueryStatus status,
    float effectiveRange,
    ScannerRoomScanState scanState,
    ulong revision,
    ushort pageIndex,
    ushort pageCount,
    List<ScannerResourceSummary> availableResources,
    List<ScannerResourceTarget> targets) : Packet
{
    public NitroxId MapRoomId { get; } = mapRoomId;
    public uint RequestId { get; } = requestId;
    public ScannerRoomQueryStatus Status { get; } = status;
    public float EffectiveRange { get; } = effectiveRange;
    public ScannerRoomScanState ScanState { get; } = scanState;
    public ulong Revision { get; } = revision;
    public ushort PageIndex { get; } = pageIndex;
    public ushort PageCount { get; } = pageCount;
    public List<ScannerResourceSummary> AvailableResources { get; } = availableResources;
    public List<ScannerResourceTarget> Targets { get; } = targets;
}
