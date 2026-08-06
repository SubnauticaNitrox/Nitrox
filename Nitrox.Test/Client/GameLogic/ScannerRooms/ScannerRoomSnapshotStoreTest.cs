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
        ScannerRoomQueryTicket query = store.BeginQuery(roomId, 300, State(quartz));

        ScannerRoomSnapshotApplyResult secondResult = store.AcceptPage(Page(query.RequestId, 1, 2, [], [Target(2)]));

        secondResult.Should().Be(ScannerRoomSnapshotApplyResult.WaitingForPages);
        store.TryGetSnapshot(roomId, out _).Should().BeFalse();

        ScannerRoomSnapshotApplyResult firstResult = store.AcceptPage(
            Page(query.RequestId, 0, 2, [new ScannerResourceSummary(quartz, 2)], [Target(1)]),
            out ScannerRoomQueryStatus? acceptedStatus);

        firstResult.Should().Be(ScannerRoomSnapshotApplyResult.Applied);
        acceptedStatus.Should().Be(ScannerRoomQueryStatus.Complete);
        store.TryGetSnapshot(roomId, out ScannerRoomSnapshot? snapshot).Should().BeTrue();
        snapshot!.AvailableResources.Should().ContainSingle(summary => summary.Count == 2);
        snapshot.Targets.Select(target => target.TrackerIndex).Should().Equal((ushort)1, (ushort)2);
    }

    [TestMethod]
    public void IgnoresDuplicateAndStalePages()
    {
        ScannerRoomSnapshotStore store = new();
        ScannerRoomQueryTicket stale = store.BeginQuery(roomId, 300, State(quartz));
        ScannerRoomQueryTicket current = store.BeginQuery(roomId, 300, State(quartz));
        ScannerRoomSnapshotPageData currentPage = Page(current.RequestId, 0, 2, [new ScannerResourceSummary(quartz, 2)], [Target(1)]);

        store.AcceptPage(Page(stale.RequestId, 0, 1, [], [])).Should().Be(ScannerRoomSnapshotApplyResult.Ignored);
        store.AcceptPage(currentPage).Should().Be(ScannerRoomSnapshotApplyResult.WaitingForPages);
        store.AcceptPage(currentPage).Should().Be(ScannerRoomSnapshotApplyResult.Ignored);
        store.AcceptPage(Page(current.RequestId, 1, 2, [], [Target(2)])).Should().Be(ScannerRoomSnapshotApplyResult.Applied);
    }

    [TestMethod]
    public void RejectsPagesWhoseCanonicalStateChangesMidSnapshot()
    {
        ScannerRoomSnapshotStore store = new();
        ScannerRoomQueryTicket query = store.BeginQuery(roomId, 300, State(quartz));

        store.AcceptPage(Page(query.RequestId, 0, 2, [], [Target(1)])).Should().Be(ScannerRoomSnapshotApplyResult.WaitingForPages);
        ScannerRoomSnapshotPageData advancedStatePage = new(
            roomId,
            query.RequestId,
            ScannerRoomQueryStatus.Complete,
            300,
            State(quartz, 8),
            42,
            1,
            2,
            [],
            [Target(2)]);

        store.AcceptPage(advancedStatePage).Should().Be(ScannerRoomSnapshotApplyResult.Ignored);
        store.TryGetSnapshot(roomId, out _).Should().BeFalse();
    }

    [TestMethod]
    public void ProcessesEachOutOfOrderPagePayloadOnlyOnceBeforeAtomicReplacement()
    {
        ScannerRoomSnapshotStore store = new();
        ScannerRoomQueryTicket query = store.BeginQuery(roomId, 300, State(quartz));
        CountingReadOnlyList<ScannerResourceTarget> firstPageTargets = new(
            Enumerable.Range(1, 256).Select(index => Target((ushort)index)).ToList());
        CountingReadOnlyList<ScannerResourceTarget> secondPageTargets = new([Target(257)]);

        store.AcceptPage(Page(query.RequestId, 1, 2, [], secondPageTargets)).Should().Be(ScannerRoomSnapshotApplyResult.WaitingForPages);

        secondPageTargets.EnumerationCount.Should().Be(1);
        firstPageTargets.EnumerationCount.Should().Be(0);
        store.TryGetSnapshot(roomId, out _).Should().BeFalse();

        store.AcceptPage(Page(query.RequestId, 0, 2, [], firstPageTargets)).Should().Be(ScannerRoomSnapshotApplyResult.Applied);

        firstPageTargets.EnumerationCount.Should().Be(1);
        secondPageTargets.EnumerationCount.Should().Be(1);
        store.TryGetSnapshot(roomId, out ScannerRoomSnapshot? snapshot).Should().BeTrue();
        snapshot!.Targets.Should().HaveCount(257);
        snapshot.Targets.Select(target => target.TrackerIndex).Should().Equal(Enumerable.Range(1, 257).Select(index => (ushort)index));
    }

    [TestMethod]
    public void OutOfOrderDuplicateTargetsKeepTheFirstOccurrenceInPageOrder()
    {
        ScannerRoomSnapshotStore store = new();
        ScannerRoomQueryTicket query = store.BeginQuery(roomId, 300, State(quartz));
        ScannerResourceTarget laterOccurrence = Target(1, new NitroxVector3(9, 0, 0));
        ScannerResourceTarget earlierOccurrence = Target(1, new NitroxVector3(1, 0, 0));

        store.AcceptPage(Page(query.RequestId, 1, 2, [], [laterOccurrence])).Should().Be(ScannerRoomSnapshotApplyResult.WaitingForPages);
        store.AcceptPage(Page(query.RequestId, 0, 2, [], [earlierOccurrence])).Should().Be(ScannerRoomSnapshotApplyResult.Applied);

        store.TryGetSnapshot(roomId, out ScannerRoomSnapshot? snapshot).Should().BeTrue();
        snapshot!.Targets.Should().ContainSingle().Which.Position.Should().Be(new NitroxVector3(1, 0, 0));
    }

    [TestMethod]
    public void ReusesRevisionOnlyForMatchingRangeAndScanState()
    {
        ScannerRoomSnapshotStore store = new();
        ScannerRoomQueryTicket first = store.BeginQuery(roomId, 300, State(quartz));
        store.AcceptPage(Page(first.RequestId, 0, 1, [new ScannerResourceSummary(quartz, 1)], [Target(1)])).Should().Be(ScannerRoomSnapshotApplyResult.Applied);

        store.BeginQuery(roomId, 300, State(quartz)).KnownRevision.Should().Be(42);
        store.BeginQuery(roomId, 350, State(quartz)).KnownRevision.Should().Be(0);
        store.BeginQuery(roomId, 300, State(new NitroxTechType("Copper"))).KnownRevision.Should().Be(0);
        store.BeginQuery(roomId, 300, State(quartz, 8)).KnownRevision.Should().Be(0);
    }

    [TestMethod]
    public void NotModifiedLeavesCurrentSnapshotInPlace()
    {
        ScannerRoomSnapshotStore store = new();
        ScannerRoomQueryTicket first = store.BeginQuery(roomId, 300, State(quartz));
        store.AcceptPage(Page(first.RequestId, 0, 1, [new ScannerResourceSummary(quartz, 1)], [Target(1)]));
        ScannerRoomQueryTicket refresh = store.BeginQuery(roomId, 300, State(quartz));

        ScannerRoomSnapshotPageData notModified = Page(refresh.RequestId, 0, 1, [], [], ScannerRoomQueryStatus.NotModified);
        store.AcceptPage(notModified, out ScannerRoomQueryStatus? acceptedStatus).Should().Be(ScannerRoomSnapshotApplyResult.NotModified);
        acceptedStatus.Should().Be(ScannerRoomQueryStatus.NotModified);
        store.TryGetSnapshot(roomId, out ScannerRoomSnapshot? snapshot).Should().BeTrue();
        snapshot!.Targets.Should().ContainSingle();
    }

    [TestMethod]
    public void InvalidNotModifiedResponseTerminatesPendingRequestAsFailure()
    {
        ScannerRoomSnapshotStore store = new();
        ScannerRoomQueryTicket refresh = store.BeginQuery(roomId, 300, State(quartz));

        ScannerRoomSnapshotPageData notModified = Page(refresh.RequestId, 0, 1, [], [], ScannerRoomQueryStatus.NotModified);

        store.AcceptPage(notModified).Should().Be(ScannerRoomSnapshotApplyResult.Failed);
    }

    [TestMethod]
    public void NotModifiedCannotReuseSnapshotFromDifferentQueryParameters()
    {
        ScannerRoomSnapshotStore store = new();
        ScannerRoomQueryTicket first = store.BeginQuery(roomId, 300, State(quartz));
        store.AcceptPage(Page(first.RequestId, 0, 1, [new ScannerResourceSummary(quartz, 1)], [Target(1)]));
        ScannerRoomQueryTicket changedRange = store.BeginQuery(roomId, 350, State(quartz));
        ScannerRoomSnapshotPageData notModified = new(
            roomId,
            changedRange.RequestId,
            ScannerRoomQueryStatus.NotModified,
            350,
            State(quartz),
            42,
            0,
            1,
            [],
            []);

        store.AcceptPage(notModified).Should().Be(ScannerRoomSnapshotApplyResult.Failed);
    }

    [TestMethod]
    public void ExposesRejectedStatusOnlyForWellFormedCurrentResponse()
    {
        ScannerRoomSnapshotStore store = new();
        ScannerRoomQueryTicket stale = store.BeginQuery(roomId, 300, State(quartz));
        ScannerRoomQueryTicket current = store.BeginQuery(roomId, 300, State(quartz));

        store.AcceptPage(
                Page(stale.RequestId, 0, 1, [], [], ScannerRoomQueryStatus.Rejected),
                out ScannerRoomQueryStatus? staleStatus)
            .Should().Be(ScannerRoomSnapshotApplyResult.Ignored);
        staleStatus.Should().BeNull();

        store.AcceptPage(
                Page(current.RequestId, 0, 1, [], [], ScannerRoomQueryStatus.Rejected),
                out ScannerRoomQueryStatus? acceptedStatus)
            .Should().Be(ScannerRoomSnapshotApplyResult.Failed);
        acceptedStatus.Should().Be(ScannerRoomQueryStatus.Rejected);
    }

    [TestMethod]
    public void DoesNotExposeRejectedStatusFromMalformedEnvelope()
    {
        ScannerRoomSnapshotStore store = new();
        ScannerRoomQueryTicket current = store.BeginQuery(roomId, 300, State(quartz));

        store.AcceptPage(
                Page(current.RequestId, 1, 2, [], [], ScannerRoomQueryStatus.Rejected),
                out ScannerRoomQueryStatus? acceptedStatus)
            .Should().Be(ScannerRoomSnapshotApplyResult.Failed);

        acceptedStatus.Should().BeNull();
    }

    [TestMethod]
    public void CancelsOnlyTheMatchingTimedOutRequest()
    {
        ScannerRoomSnapshotStore store = new();
        ScannerRoomQueryTicket timedOut = store.BeginQuery(roomId, 300, State(quartz));

        store.CancelQuery(roomId, timedOut.RequestId + 1).Should().BeFalse();
        store.CancelQuery(roomId, timedOut.RequestId).Should().BeTrue();
        store.AcceptPage(Page(timedOut.RequestId, 0, 1, [], [])).Should().Be(ScannerRoomSnapshotApplyResult.Ignored);
    }

    private ScannerRoomSnapshotPageData Page(
        uint requestId,
        ushort pageIndex,
        ushort pageCount,
        IReadOnlyList<ScannerResourceSummary> summaries,
        IReadOnlyList<ScannerResourceTarget> targets,
        ScannerRoomQueryStatus status = ScannerRoomQueryStatus.Complete) =>
        new(roomId, requestId, status, 300, State(quartz), 42, pageIndex, pageCount, summaries, targets);

    private static ScannerRoomScanState State(NitroxTechType? selectedTechType, ulong version = 7) =>
        new(selectedTechType, version);

    private ScannerResourceTarget Target(ushort trackerIndex) => Target(trackerIndex, new NitroxVector3(trackerIndex, 0, 0));

    private ScannerResourceTarget Target(ushort trackerIndex, NitroxVector3 position) =>
        new(new NitroxId($"00000000-0000-0000-0000-{trackerIndex:D12}"), trackerIndex, quartz, position);

    private sealed class CountingReadOnlyList<T>(IReadOnlyList<T> values) : IReadOnlyList<T>
    {
        public int EnumerationCount { get; private set; }

        public int Count => values.Count;

        public T this[int index] => values[index];

        public IEnumerator<T> GetEnumerator()
        {
            EnumerationCount++;
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
