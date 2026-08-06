using System;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

namespace NitroxClient.GameLogic.ScannerRooms;

/// <summary>
/// Coordinates Scanner Room requests emitted by lifecycle, interaction, and periodic refresh hooks.
/// </summary>
internal sealed class ScannerRoomRequestTrigger(Action<float, ScannerRoomScanState, NitroxVector3?> requestSnapshot)
{
    private readonly Action<float, ScannerRoomScanState, NitroxVector3?> requestSnapshot = requestSnapshot;
    private bool hasIssuedRequest;
    private QueryState? lastRequest;

    public bool TryRequestInitial(float range, ScannerRoomScanState expectedScanState, NitroxVector3? observedOrigin)
    {
        if (hasIssuedRequest)
        {
            return false;
        }

        Request(range, expectedScanState, observedOrigin);
        return true;
    }

    public void RequestImmediate(float range, ScannerRoomScanState expectedScanState, NitroxVector3? observedOrigin) =>
        Request(range, expectedScanState, observedOrigin);

    public bool TryRequestIfChanged(float range, ScannerRoomScanState expectedScanState, NitroxVector3? observedOrigin)
    {
        QueryState request = QueryState.From(range, expectedScanState);
        if (lastRequest == request)
        {
            return false;
        }

        Request(range, expectedScanState, observedOrigin);
        return true;
    }

    private void Request(float range, ScannerRoomScanState expectedScanState, NitroxVector3? observedOrigin)
    {
        requestSnapshot(range, expectedScanState, observedOrigin);
        hasIssuedRequest = true;
        lastRequest = QueryState.From(range, expectedScanState);
    }

    private readonly record struct QueryState(float EffectiveRange, string? SelectedTechType, ulong ExpectedScanStateVersion)
    {
        public static QueryState From(float range, ScannerRoomScanState expectedScanState) =>
            new(ScannerRoomQueryParameters.NormalizeRange(range), expectedScanState.SelectedTechType?.Name, expectedScanState.Version);
    }
}
