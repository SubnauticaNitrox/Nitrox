using System.Diagnostics;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;
using Nitrox.Server.Subnautica.Models.AppEvents;
using Nitrox.Server.Subnautica.Models.AppEvents.Core;

namespace Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;

internal sealed record ScannerRoomDiagnosticsSnapshot(
    bool ResourceSyncEnabled,
    long RequestsReceived,
    long QueriesCompleted,
    long QueriesInFlight,
    long PeakQueriesInFlight,
    long BatchLoadsInFlight,
    long BatchesRequested,
    long ResourceTypesMatched,
    long TargetsMatched,
    long PagesSent,
    long ThrottledRequests,
    long CompleteResponses,
    long NotModifiedResponses,
    long InvalidRoomResponses,
    long OriginUnavailableResponses,
    long RejectedResponses,
    long StateOutdatedResponses,
    long FailedResponses,
    double TotalQueryDurationMilliseconds,
    double MaximumQueryDurationMilliseconds)
{
    public double AverageQueryDurationMilliseconds => QueriesCompleted == 0 ? 0 : TotalQueryDurationMilliseconds / QueriesCompleted;
}

/// <summary>
/// Low-overhead process-local diagnostics for authoritative Scanner Room queries. The snapshot is intentionally
/// non-persistent: it describes the current server run and is exposed through the existing server summary.
/// </summary>
internal sealed class ScannerRoomDiagnostics(
    IOptions<SubnauticaServerOptions> options,
    ILogger<ScannerRoomDiagnostics> logger) : ISummarize
{
    private const int STATUS_COUNT = (int)ScannerRoomQueryStatus.Failed + 1;

    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly ILogger<ScannerRoomDiagnostics> logger = logger;
    private readonly long[] responseCountsByStatus = new long[STATUS_COUNT];
    private long requestsReceived;
    private long queriesCompleted;
    private long queriesInFlight;
    private long peakQueriesInFlight;
    private long batchLoadsInFlight;
    private long batchesRequested;
    private long resourceTypesMatched;
    private long targetsMatched;
    private long pagesSent;
    private long throttledRequests;
    private long totalQueryDurationTicks;
    private long maximumQueryDurationTicks;

    public long QueryStarted()
    {
        Interlocked.Increment(ref requestsReceived);
        long inFlight = Interlocked.Increment(ref queriesInFlight);
        UpdateMaximum(ref peakQueriesInFlight, inFlight);
        return Stopwatch.GetTimestamp();
    }

    public void QueryCompleted(long startedAt, ScannerRoomQueryStatus status)
    {
        long elapsed = Math.Max(0, Stopwatch.GetTimestamp() - startedAt);
        Interlocked.Add(ref totalQueryDurationTicks, elapsed);
        UpdateMaximum(ref maximumQueryDurationTicks, elapsed);
        RecordStatus(status);
        Interlocked.Increment(ref queriesCompleted);
        Interlocked.Decrement(ref queriesInFlight);
    }

    public void QueryThrottled()
    {
        Interlocked.Increment(ref requestsReceived);
        Interlocked.Increment(ref throttledRequests);
        RecordStatus(ScannerRoomQueryStatus.Rejected);
    }

    public void BatchLoadStarted(int batchCount)
    {
        Interlocked.Add(ref batchesRequested, Math.Max(0, batchCount));
        Interlocked.Increment(ref batchLoadsInFlight);
    }

    public void BatchLoadCompleted() => Interlocked.Decrement(ref batchLoadsInFlight);

    public void ResultMatched(int resourceTypeCount, int targetCount)
    {
        Interlocked.Add(ref resourceTypesMatched, Math.Max(0, resourceTypeCount));
        Interlocked.Add(ref targetsMatched, Math.Max(0, targetCount));
    }

    public void PagesSent(int count) => Interlocked.Add(ref pagesSent, Math.Max(0, count));

    public ScannerRoomDiagnosticsSnapshot GetSnapshot()
    {
        long completed = Interlocked.Read(ref queriesCompleted);
        long totalDuration = Interlocked.Read(ref totalQueryDurationTicks);
        long maximumDuration = Interlocked.Read(ref maximumQueryDurationTicks);
        return new ScannerRoomDiagnosticsSnapshot(
            options.Value.EnableScannerRoomResourceSync,
            Interlocked.Read(ref requestsReceived),
            completed,
            Interlocked.Read(ref queriesInFlight),
            Interlocked.Read(ref peakQueriesInFlight),
            Interlocked.Read(ref batchLoadsInFlight),
            Interlocked.Read(ref batchesRequested),
            Interlocked.Read(ref resourceTypesMatched),
            Interlocked.Read(ref targetsMatched),
            Interlocked.Read(ref pagesSent),
            Interlocked.Read(ref throttledRequests),
            ReadStatus(ScannerRoomQueryStatus.Complete),
            ReadStatus(ScannerRoomQueryStatus.NotModified),
            ReadStatus(ScannerRoomQueryStatus.InvalidRoom),
            ReadStatus(ScannerRoomQueryStatus.OriginUnavailable),
            ReadStatus(ScannerRoomQueryStatus.Rejected),
            ReadStatus(ScannerRoomQueryStatus.StateOutdated),
            ReadStatus(ScannerRoomQueryStatus.Failed),
            ToMilliseconds(totalDuration),
            ToMilliseconds(maximumDuration));
    }

    Task IEvent<ISummarize.Args>.OnEventAsync(ISummarize.Args args)
    {
        ScannerRoomDiagnosticsSnapshot snapshot = GetSnapshot();
        string featureState = snapshot.ResourceSyncEnabled ? "ENABLED" : "DISABLED";
        logger.ZLogInformation($"Scanner Room resource sync: {featureState:@State} (rollback option: {nameof(SubnauticaServerOptions.EnableScannerRoomResourceSync):@Option})");
        logger.ZLogInformation($"Scanner Room queries: {snapshot.RequestsReceived:@Received} received, {snapshot.QueriesCompleted:@Completed} completed, {snapshot.QueriesInFlight:@InFlight} in flight, {snapshot.PeakQueriesInFlight:@PeakInFlight} peak, {snapshot.ThrottledRequests:@Throttled} throttled");
        logger.ZLogInformation($"Scanner Room responses: {snapshot.CompleteResponses:@Complete} complete, {snapshot.NotModifiedResponses:@NotModified} unchanged, {snapshot.InvalidRoomResponses:@InvalidRoom} invalid room, {snapshot.OriginUnavailableResponses:@OriginUnavailable} missing origin, {snapshot.RejectedResponses:@Rejected} rejected, {snapshot.StateOutdatedResponses:@StateOutdated} stale state, {snapshot.FailedResponses:@Failed} failed");
        logger.ZLogInformation($"Scanner Room loading: {snapshot.BatchLoadsInFlight:@ActiveLoads} active loads, {snapshot.BatchesRequested:@Batches} batches requested, {snapshot.ResourceTypesMatched:@ResourceTypes} resource types, {snapshot.TargetsMatched:@Targets} targets, {snapshot.PagesSent:@Pages} pages, {double.Round(snapshot.AverageQueryDurationMilliseconds, 2):@AverageMs} ms average, {double.Round(snapshot.MaximumQueryDurationMilliseconds, 2):@MaximumMs} ms maximum");
        return Task.CompletedTask;
    }

    private void RecordStatus(ScannerRoomQueryStatus status)
    {
        int index = (int)status;
        if ((uint)index < (uint)responseCountsByStatus.Length)
        {
            Interlocked.Increment(ref responseCountsByStatus[index]);
        }
    }

    private long ReadStatus(ScannerRoomQueryStatus status) => Interlocked.Read(ref responseCountsByStatus[(int)status]);

    private static double ToMilliseconds(long stopwatchTicks) => stopwatchTicks * 1000d / Stopwatch.Frequency;

    private static void UpdateMaximum(ref long location, long candidate)
    {
        long current = Interlocked.Read(ref location);
        while (candidate > current)
        {
            long observed = Interlocked.CompareExchange(ref location, candidate, current);
            if (observed == current)
            {
                return;
            }
            current = observed;
        }
    }
}
