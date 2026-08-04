using System;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

namespace NitroxClient.GameLogic.ScannerRooms;

/// <summary>
/// Coordinates Scanner Room requests emitted by lifecycle, interaction, and periodic refresh hooks.
/// </summary>
internal sealed class ScannerRoomRequestTrigger(Action<float, NitroxTechType?, NitroxVector3?> requestSnapshot)
{
    private readonly Action<float, NitroxTechType?, NitroxVector3?> requestSnapshot = requestSnapshot;
    private bool hasIssuedRequest;
    private QueryState? lastRequest;

    public bool TryRequestInitial(float range, NitroxTechType? selectedTechType, NitroxVector3? observedOrigin)
    {
        if (hasIssuedRequest)
        {
            return false;
        }

        Request(range, selectedTechType, observedOrigin);
        return true;
    }

    public void RequestImmediate(float range, NitroxTechType? selectedTechType, NitroxVector3? observedOrigin) =>
        Request(range, selectedTechType, observedOrigin);

    public bool TryRequestIfChanged(float range, NitroxTechType? selectedTechType, NitroxVector3? observedOrigin)
    {
        QueryState request = QueryState.From(range, selectedTechType);
        if (lastRequest == request)
        {
            return false;
        }

        Request(range, selectedTechType, observedOrigin);
        return true;
    }

    private void Request(float range, NitroxTechType? selectedTechType, NitroxVector3? observedOrigin)
    {
        requestSnapshot(range, selectedTechType, observedOrigin);
        hasIssuedRequest = true;
        lastRequest = QueryState.From(range, selectedTechType);
    }

    private readonly record struct QueryState(float EffectiveRange, string? SelectedTechType)
    {
        public static QueryState From(float range, NitroxTechType? selectedTechType)
        {
            NitroxTechType? normalizedSelection = ScannerRoomQueryParameters.NormalizeSelection(selectedTechType);
            return new QueryState(ScannerRoomQueryParameters.NormalizeRange(range), normalizedSelection?.Name);
        }
    }
}
