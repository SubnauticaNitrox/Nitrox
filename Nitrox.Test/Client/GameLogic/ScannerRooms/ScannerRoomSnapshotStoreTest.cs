using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

namespace NitroxClient.GameLogic.ScannerRooms;

[TestClass]
public sealed class ScannerRoomSnapshotStoreTest
{
    private readonly NitroxId roomId = new("c17b4eef-af1b-48e4-b606-f80248744225");
    private readonly NitroxTechType quartz = new("Quartz");

    [TestMethod]
    public void AppliesPagesAtomicallyWhenTheyArriveOutOfOrder()
    {
        ScannerRoomSnapshotStore store = new();
        ScannerRoomQueryTicket query = store.BeginQuery(roomId, 300, quartz);

        ScannerRoomSnapshotApplyResult secondResult = store.AcceptPage(Page(query.RequestId, 1, 2, [], [Target(2)]));

        secondResult.Should().Be(ScannerRoomSnapshotApplyResult.WaitingForPages);
        store.TryGetSnapshot(roomId, out _).Should().BeFalse();

        ScannerRoomSnapshotApplyResult firstResult = store.AcceptPage(Page(query.RequestId, 0, 2, [new ScannerResourceSummary(quartz, 2)], [Target(1)]));

        firstResult.Should().Be(ScannerRoomSnapshotApplyResult.Applied);
        store.TryGetSnapshot(roomId, out ScannerRoomSnapshot? snapshot).Should().BeTrue();
        snapshot!.AvailableResources.Should().ContainSingle(summary => summary.Count == 2);
        snapshot.Targets.Select(target => target.TrackerIndex).Should().Equal((ushort)1, (ushort)2);
    }

    [TestMethod]
    public void IgnoresDuplicateAndStalePages()
    {
        ScannerRoomSnapshotStore store = new();
        ScannerRoomQueryTicket stale = store.BeginQuery(roomId, 300, quartz);
        ScannerRoomQueryTicket current = store.BeginQuery(roomId, 300, quartz);
        ScannerRoomSnapshotPageData currentPage = Page(current.RequestId, 0, 2, [new ScannerResourceSummary(quartz, 2)], [Target(1)]);

        store.AcceptPage(Page(stale.RequestId, 0, 1, [], [])).Should().Be(ScannerRoomSnapshotApplyResult.Ignored);
        store.AcceptPage(currentPage).Should().Be(ScannerRoomSnapshotApplyResult.WaitingForPages);
        store.AcceptPage(currentPage).Should().Be(ScannerRoomSnapshotApplyResult.Ignored);
        store.AcceptPage(Page(current.RequestId, 1, 2, [], [Target(2)])).Should().Be(ScannerRoomSnapshotApplyResult.Applied);
    }

    [TestMethod]
    public void ReusesRevisionOnlyForMatchingRangeAndSelection()
    {
        ScannerRoomSnapshotStore store = new();
        ScannerRoomQueryTicket first = store.BeginQuery(roomId, 300, quartz);
        store.AcceptPage(Page(first.RequestId, 0, 1, [new ScannerResourceSummary(quartz, 1)], [Target(1)])).Should().Be(ScannerRoomSnapshotApplyResult.Applied);

        store.BeginQuery(roomId, 300, quartz).KnownRevision.Should().Be(42);
        store.BeginQuery(roomId, 350, quartz).KnownRevision.Should().Be(0);
        store.BeginQuery(roomId, 300, new NitroxTechType("Copper")).KnownRevision.Should().Be(0);
    }

    [TestMethod]
    public void NotModifiedLeavesCurrentSnapshotInPlace()
    {
        ScannerRoomSnapshotStore store = new();
        ScannerRoomQueryTicket first = store.BeginQuery(roomId, 300, quartz);
        store.AcceptPage(Page(first.RequestId, 0, 1, [new ScannerResourceSummary(quartz, 1)], [Target(1)]));
        ScannerRoomQueryTicket refresh = store.BeginQuery(roomId, 300, quartz);

        ScannerRoomSnapshotPageData notModified = Page(refresh.RequestId, 0, 1, [], [], ScannerRoomQueryStatus.NotModified);
        store.AcceptPage(notModified).Should().Be(ScannerRoomSnapshotApplyResult.NotModified);
        store.TryGetSnapshot(roomId, out ScannerRoomSnapshot? snapshot).Should().BeTrue();
        snapshot!.Targets.Should().ContainSingle();
    }

    private ScannerRoomSnapshotPageData Page(
        uint requestId,
        ushort pageIndex,
        ushort pageCount,
        IReadOnlyList<ScannerResourceSummary> summaries,
        IReadOnlyList<ScannerResourceTarget> targets,
        ScannerRoomQueryStatus status = ScannerRoomQueryStatus.Complete) =>
        new(roomId, requestId, status, 300, quartz, 42, pageIndex, pageCount, summaries, targets);

    private ScannerResourceTarget Target(ushort trackerIndex) =>
        new(new NitroxId($"00000000-0000-0000-0000-{trackerIndex:D12}"), trackerIndex, quartz, new NitroxVector3(trackerIndex, 0, 0));
}
