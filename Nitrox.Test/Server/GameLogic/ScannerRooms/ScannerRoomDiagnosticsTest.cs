using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nitrox.Model.Configuration;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;
using Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;
using NSubstitute;

namespace Nitrox.Test.Server.GameLogic.ScannerRooms;

[TestClass]
public sealed class ScannerRoomDiagnosticsTest
{
    [TestMethod]
    public void SnapshotTracksLoadingWorkloadResponsesAndRollbackState()
    {
        SubnauticaServerOptions options = new() { EnableScannerRoomResourceSync = true };
        ScannerRoomDiagnostics diagnostics = new(Options.Create(options), Substitute.For<ILogger<ScannerRoomDiagnostics>>());

        long completeQuery = diagnostics.QueryStarted();
        diagnostics.BatchLoadStarted(12);

        ScannerRoomDiagnosticsSnapshot loading = diagnostics.GetSnapshot();
        loading.ResourceSyncEnabled.Should().BeTrue();
        loading.RequestsReceived.Should().Be(1);
        loading.QueriesInFlight.Should().Be(1);
        loading.BatchLoadsInFlight.Should().Be(1);
        loading.BatchesRequested.Should().Be(12);

        diagnostics.BatchLoadCompleted();
        diagnostics.ResultMatched(3, 7);
        diagnostics.QueryCompleted(completeQuery, ScannerRoomQueryStatus.Complete);
        diagnostics.PagesSent(2);

        long unchangedQuery = diagnostics.QueryStarted();
        diagnostics.QueryCompleted(unchangedQuery, ScannerRoomQueryStatus.NotModified);
        diagnostics.PagesSent(1);
        diagnostics.QueryThrottled();
        diagnostics.PagesSent(1);

        options.EnableScannerRoomResourceSync = false;
        ScannerRoomDiagnosticsSnapshot snapshot = diagnostics.GetSnapshot();
        snapshot.ResourceSyncEnabled.Should().BeFalse();
        snapshot.RequestsReceived.Should().Be(3);
        snapshot.QueriesCompleted.Should().Be(2);
        snapshot.QueriesInFlight.Should().Be(0);
        snapshot.PeakQueriesInFlight.Should().Be(1);
        snapshot.BatchLoadsInFlight.Should().Be(0);
        snapshot.BatchesRequested.Should().Be(12);
        snapshot.ResourceTypesMatched.Should().Be(3);
        snapshot.TargetsMatched.Should().Be(7);
        snapshot.PagesSent.Should().Be(4);
        snapshot.ThrottledRequests.Should().Be(1);
        snapshot.CompleteResponses.Should().Be(1);
        snapshot.NotModifiedResponses.Should().Be(1);
        snapshot.RejectedResponses.Should().Be(1);
        snapshot.FailedResponses.Should().Be(0);
        snapshot.AverageQueryDurationMilliseconds.Should().BeGreaterThanOrEqualTo(0);
        snapshot.MaximumQueryDurationMilliseconds.Should().BeGreaterThanOrEqualTo(snapshot.AverageQueryDurationMilliseconds);
    }

    [TestMethod]
    public void SnapshotCountsEveryProtocolFailureStatus()
    {
        ScannerRoomDiagnostics diagnostics = new(
            Options.Create(new SubnauticaServerOptions()),
            Substitute.For<ILogger<ScannerRoomDiagnostics>>());

        foreach (ScannerRoomQueryStatus status in new[]
                 {
                     ScannerRoomQueryStatus.InvalidRoom,
                     ScannerRoomQueryStatus.OriginUnavailable,
                     ScannerRoomQueryStatus.Rejected,
                     ScannerRoomQueryStatus.StateOutdated,
                     ScannerRoomQueryStatus.Failed
                 })
        {
            long started = diagnostics.QueryStarted();
            diagnostics.QueryCompleted(started, status);
        }

        ScannerRoomDiagnosticsSnapshot snapshot = diagnostics.GetSnapshot();
        snapshot.InvalidRoomResponses.Should().Be(1);
        snapshot.OriginUnavailableResponses.Should().Be(1);
        snapshot.RejectedResponses.Should().Be(1);
        snapshot.StateOutdatedResponses.Should().Be(1);
        snapshot.FailedResponses.Should().Be(1);
        snapshot.QueriesCompleted.Should().Be(5);
        snapshot.QueriesInFlight.Should().Be(0);
    }
}
