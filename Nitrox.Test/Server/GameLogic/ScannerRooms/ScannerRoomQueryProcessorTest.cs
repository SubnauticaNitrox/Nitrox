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
    public void LimiterSerializesQueriesPerSession()
    {
        ScannerRoomQueryLimiter limiter = new();

        limiter.TryEnter((SessionId)1, out IDisposable? firstLease).Should().BeTrue();
        limiter.TryEnter((SessionId)1, out IDisposable? duplicateLease).Should().BeFalse();
        limiter.TryEnter((SessionId)2, out IDisposable? otherPlayerLease).Should().BeTrue();

        duplicateLease.Should().BeNull();
        firstLease!.Dispose();
        otherPlayerLease!.Dispose();
    }

    [TestMethod]
    public void CompleteSnapshotIsPagedWithoutTruncation()
    {
        NitroxId roomId = new("704110ab-6442-4973-94ea-158de10f381a");
        NitroxTechType quartz = new("Quartz");
        ScannerRoomQuery query = new(roomId, 9, 500, quartz, 0, null);
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
            quartz,
            42,
            [new ScannerResourceSummary(quartz, targets.Count)],
            targets);

        IReadOnlyList<ScannerRoomSnapshotPage> pages = ScannerRoomQueryProcessor.CreatePages(query, result);

        pages.Should().HaveCount(3);
        pages.Select(page => page.PageIndex).Should().Equal((ushort)0, (ushort)1, (ushort)2);
        pages.Should().OnlyContain(page => page.PageCount == 3 && page.RequestId == query.RequestId && page.MapRoomId == roomId);
        pages.Select(page => page.Targets.Count).Should().Equal(256, 256, 1);
        pages[0].AvailableResources.Should().ContainSingle(summary => summary.Count == 513);
        pages.Skip(1).Should().OnlyContain(page => page.AvailableResources.Count == 0);
        pages.SelectMany(page => page.Targets).Select(target => target.EntityId).Should().Equal(targets.Select(target => target.EntityId));
    }

    [TestMethod]
    public void NotModifiedResponseUsesOneEmptyPage()
    {
        NitroxId roomId = new("b710541e-6285-46bd-9fdd-625bd3a9df0c");
        ScannerRoomQuery query = new(roomId, 10, 300, null, 77, null);
        ScannerRoomQueryResult result = new(ScannerRoomQueryStatus.NotModified, 300, null, 77, [], []);

        ScannerRoomSnapshotPage page = ScannerRoomQueryProcessor.CreatePages(query, result).Should().ContainSingle().Which;

        page.Status.Should().Be(ScannerRoomQueryStatus.NotModified);
        page.PageIndex.Should().Be(0);
        page.PageCount.Should().Be(1);
        page.AvailableResources.Should().BeEmpty();
        page.Targets.Should().BeEmpty();
    }
}
