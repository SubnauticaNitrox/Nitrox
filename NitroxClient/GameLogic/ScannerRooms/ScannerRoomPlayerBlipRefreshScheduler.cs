namespace NitroxClient.GameLogic.ScannerRooms;

/// <summary>
/// Keeps remote-player Scanner Room blips no more than a fixed interval behind, independent of vanilla scan-speed upgrades.
/// </summary>
internal sealed class ScannerRoomPlayerBlipRefreshScheduler
{
    internal const double REFRESH_INTERVAL_SECONDS = 3;

    private double nextRefresh = double.NegativeInfinity;

    public bool IsRefreshDue(double now) => now >= nextRefresh;

    public void MarkRefreshed(double now) => nextRefresh = now + REFRESH_INTERVAL_SECONDS;
}
