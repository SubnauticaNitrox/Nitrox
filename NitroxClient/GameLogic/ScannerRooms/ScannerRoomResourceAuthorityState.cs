using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

namespace NitroxClient.GameLogic.ScannerRooms;

internal enum ScannerRoomResourceAuthorityMode
{
    Pending,
    Authoritative,
    Rollback
}

/// <summary>
/// Decides whether a Scanner Room may use the client-local resource database. Responses passed here have already
/// been correlated with the room's current request by <see cref="ScannerRoomSnapshotStore"/>.
/// </summary>
internal sealed class ScannerRoomResourceAuthorityState
{
    public ScannerRoomResourceAuthorityMode Mode { get; private set; } = ScannerRoomResourceAuthorityMode.Pending;

    public bool SuppressVanillaResources => Mode != ScannerRoomResourceAuthorityMode.Rollback;

    public static bool IsAuthorityDecision(ScannerRoomSnapshotApplyResult result, ScannerRoomQueryStatus? status) =>
        ResolveMode(result, status) != null;

    public static bool RequiresFallbackClear(
        ScannerRoomResourceAuthorityMode previousMode,
        ScannerRoomResourceAuthorityMode currentMode) =>
        previousMode == ScannerRoomResourceAuthorityMode.Rollback &&
        currentMode == ScannerRoomResourceAuthorityMode.Authoritative;

    public bool ObserveAcceptedResponse(ScannerRoomSnapshotApplyResult result, ScannerRoomQueryStatus? status)
    {
        ScannerRoomResourceAuthorityMode nextMode = ResolveMode(result, status) ?? Mode;

        if (nextMode == Mode)
        {
            return false;
        }

        Mode = nextMode;
        return true;
    }

    public bool ResetToPending()
    {
        if (Mode == ScannerRoomResourceAuthorityMode.Pending)
        {
            return false;
        }

        Mode = ScannerRoomResourceAuthorityMode.Pending;
        return true;
    }

    private static ScannerRoomResourceAuthorityMode? ResolveMode(
        ScannerRoomSnapshotApplyResult result,
        ScannerRoomQueryStatus? status) =>
        (result, status) switch
        {
            (ScannerRoomSnapshotApplyResult.Applied, ScannerRoomQueryStatus.Complete) => ScannerRoomResourceAuthorityMode.Authoritative,
            (ScannerRoomSnapshotApplyResult.NotModified, ScannerRoomQueryStatus.NotModified) => ScannerRoomResourceAuthorityMode.Authoritative,
            (ScannerRoomSnapshotApplyResult.Failed, ScannerRoomQueryStatus.Rejected) => ScannerRoomResourceAuthorityMode.Rollback,
            _ => null
        };
}
