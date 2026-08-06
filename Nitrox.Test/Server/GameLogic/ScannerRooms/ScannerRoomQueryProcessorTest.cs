using Nitrox.Model.DataStructures;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;
using Nitrox.Server.Subnautica.Models.Packets.Processors;

namespace Nitrox.Test.Server.GameLogic.ScannerRooms;

[TestClass]
public sealed class ScannerRoomQueryProcessorTest
{
    [TestMethod]
    public async Task LimiterQueuesQueriesPerSessionWithoutBlockingOtherPlayers()
    {
        ScannerRoomQueryLimiter limiter = new(TimeSpan.Zero);

        IDisposable firstLease = await limiter.EnterAsync((SessionId)1);
        Task<IDisposable> queuedLeaseTask = limiter.EnterAsync((SessionId)1);
        IDisposable otherPlayerLease = await limiter.EnterAsync((SessionId)2);

        queuedLeaseTask.IsCompleted.Should().BeFalse();
        otherPlayerLease.Dispose();
        firstLease.Dispose();

        IDisposable queuedLease = await queuedLeaseTask;
        queuedLease.Dispose();
    }

    [TestMethod]
    public void CompleteSnapshotIsPagedWithoutTruncation()
    {
        NitroxId roomId = new("704110ab-6442-4973-94ea-158de10f381a");
        NitroxTechType quartz = new("Quartz");
        ScannerRoomScanState scanState = new(quartz, 4);
        ScannerRoomQuery query = new(roomId, 9, 500, scanState.Version, 0, null);
        List<ScannerResourceTarget> targets = Enumerable.Range(0, 513)
                                                        .Select(index => new ScannerResourceTarget(
                                                                    new NitroxId($"00000000-0000-0000-0000-{index:D12}"),
                                                                    checked((ushort)index),
                                                                    quartz,
                                                                    new NitroxVector3(index, 0, 0)))
                                                        .ToList();
        ScannerRoomQueryResult result = new(
            ScannerRoomQueryStatus.Complete,
            500,
            scanState,
            42,
            [new ScannerResourceSummary(quartz, targets.Count)],
            targets);

        IReadOnlyList<ScannerRoomSnapshotPage> pages = ScannerRoomQueryProcessor.CreatePages(query, result);

        pages.Should().HaveCount(3);
        pages.Select(page => page.PageIndex).Should().Equal((ushort)0, (ushort)1, (ushort)2);
        pages.Should().OnlyContain(page => page.PageCount == 3 && page.RequestId == query.RequestId && page.MapRoomId == roomId);
        pages.Should().OnlyContain(page => page.ScanState.Version == 4 && page.ScanState.SelectedTechType!.Equals(quartz));
        pages.Select(page => page.Targets.Count).Should().Equal(256, 256, 1);
        pages[0].AvailableResources.Should().ContainSingle(summary => summary.Count == 513);
        pages.Skip(1).Should().OnlyContain(page => page.AvailableResources.Count == 0);
        pages.SelectMany(page => page.Targets).Select(target => target.EntityId).Should().Equal(targets.Select(target => target.EntityId));
    }

    [TestMethod]
    public void NotModifiedResponseUsesOneEmptyPage()
    {
        NitroxId roomId = new("b710541e-6285-46bd-9fdd-625bd3a9df0c");
        ScannerRoomQuery query = new(roomId, 10, 300, 0, 77, null);
        ScannerRoomQueryResult result = new(ScannerRoomQueryStatus.NotModified, 300, ScannerRoomScanState.Empty, 77, [], []);

        ScannerRoomSnapshotPage page = ScannerRoomQueryProcessor.CreatePages(query, result).Should().ContainSingle().Which;

        page.Status.Should().Be(ScannerRoomQueryStatus.NotModified);
        page.PageIndex.Should().Be(0);
        page.PageCount.Should().Be(1);
        page.AvailableResources.Should().BeEmpty();
        page.Targets.Should().BeEmpty();
    }

    [TestMethod]
    public void StateOutdatedResponseReturnsCanonicalStateOnOneEmptyPage()
    {
        NitroxId roomId = new("61c5a5df-3d11-46f3-9fd6-133fa37d8a17");
        NitroxTechType quartz = new("Quartz");
        ScannerRoomQuery query = new(roomId, 11, 300, 3, 0, null);
        ScannerRoomScanState canonicalState = new(quartz, 4);
        ScannerRoomQueryResult result = ScannerRoomQueryResult.Error(
            ScannerRoomQueryStatus.StateOutdated,
            300,
            canonicalState);

        ScannerRoomSnapshotPage page = ScannerRoomQueryProcessor.CreatePages(query, result).Should().ContainSingle().Which;

        page.Status.Should().Be(ScannerRoomQueryStatus.StateOutdated);
        page.ScanState.Should().BeSameAs(canonicalState);
        page.PageIndex.Should().Be(0);
        page.PageCount.Should().Be(1);
        page.AvailableResources.Should().BeEmpty();
        page.Targets.Should().BeEmpty();
    }
}
