namespace NitroxClient.GameLogic.ScannerRooms;

/// <summary>
/// Schedules room refreshes without depending on Unity time, allowing deterministic cadence tests.
/// A single request satisfies both selected-target and open-catalog deadlines.
/// </summary>
internal sealed class ScannerRoomRefreshScheduler
{
    internal const double SELECTED_TARGET_INTERVAL_SECONDS = 5;
    internal const double OPEN_CATALOG_INTERVAL_SECONDS = 10;
    internal const double INTERACTION_DEDUPLICATION_SECONDS = 1;

    private bool selectionActive;
    private bool catalogActive;
    private double nextSelectedTargetRefresh = double.PositiveInfinity;
    private double nextCatalogRefresh = double.PositiveInfinity;
    private double lastRefresh = double.NegativeInfinity;

    public bool HasRefreshActivity => selectionActive || catalogActive;

    public void SetActivity(bool selectionActive, bool catalogActive, double now)
    {
        if (this.selectionActive != selectionActive)
        {
            this.selectionActive = selectionActive;
            nextSelectedTargetRefresh = selectionActive ? now + SELECTED_TARGET_INTERVAL_SECONDS : double.PositiveInfinity;
        }
        if (this.catalogActive != catalogActive)
        {
            this.catalogActive = catalogActive;
            nextCatalogRefresh = catalogActive ? now + OPEN_CATALOG_INTERVAL_SECONDS : double.PositiveInfinity;
        }
    }

    public bool IsRefreshDue(double now) =>
        selectionActive && now >= nextSelectedTargetRefresh || catalogActive && now >= nextCatalogRefresh;

    public bool WasRefreshedRecently(double now) =>
        now >= lastRefresh && now - lastRefresh <= INTERACTION_DEDUPLICATION_SECONDS;

    public void MarkRefreshed(double now)
    {
        lastRefresh = now;
        nextSelectedTargetRefresh = selectionActive ? now + SELECTED_TARGET_INTERVAL_SECONDS : double.PositiveInfinity;
        nextCatalogRefresh = catalogActive ? now + OPEN_CATALOG_INTERVAL_SECONDS : double.PositiveInfinity;
    }
}
