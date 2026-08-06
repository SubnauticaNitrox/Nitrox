using System.Collections.Generic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

namespace Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;

internal sealed record ScannerRoomQueryResult(
    ScannerRoomQueryStatus Status,
    float EffectiveRange,
    ScannerRoomScanState ScanState,
    ulong Revision,
    IReadOnlyList<ScannerResourceSummary> AvailableResources,
    IReadOnlyList<ScannerResourceTarget> Targets)
{
    public static ScannerRoomQueryResult Error(ScannerRoomQueryStatus status, float effectiveRange, ScannerRoomScanState scanState) =>
        new(status, effectiveRange, scanState, 0, [], []);
}
