using System;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace NitroxClient.GameLogic.ScannerRooms;

/// <summary>
/// Coordinates one-off Scanner Room requests emitted by Unity lifecycle and interaction hooks.
/// Periodic refresh scheduling is intentionally owned by a later integration layer.
/// </summary>
internal sealed class ScannerRoomRequestTrigger(Action<float, NitroxTechType?, NitroxVector3?> requestSnapshot)
{
    private readonly Action<float, NitroxTechType?, NitroxVector3?> requestSnapshot = requestSnapshot;
    private bool hasIssuedRequest;

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

    private void Request(float range, NitroxTechType? selectedTechType, NitroxVector3? observedOrigin)
    {
        requestSnapshot(range, selectedTechType, observedOrigin);
        hasIssuedRequest = true;
    }
}
