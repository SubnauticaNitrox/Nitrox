using System.Collections.Generic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

namespace Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;

internal sealed record ScannerRoomQueryResult(
    ScannerRoomQueryStatus Status,
    float EffectiveRange,
    NitroxTechType? SelectedTechType,
    ulong Revision,
    IReadOnlyList<ScannerResourceSummary> AvailableResources,
    IReadOnlyList<ScannerResourceTarget> Targets)
{
    public static ScannerRoomQueryResult Error(ScannerRoomQueryStatus status, float effectiveRange, NitroxTechType? selectedTechType) =>
        new(status, effectiveRange, selectedTechType, 0, [], []);
}
