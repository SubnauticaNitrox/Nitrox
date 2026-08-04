using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.GameLogic.ScannerRooms;

[TestClass]
public sealed class ScannerRoomSnapshotPageQueueTest
{
    private readonly NitroxId firstRoomId = new("61ed06a4-71ad-4750-ab1a-73cebf78322a");
    private readonly NitroxId secondRoomId = new("629a6ad3-d5f0-4a16-b334-c43b92219f3e");

    [TestMethod]
    public void ProcessesOnlyTheRequestedNumberOfPagesInFifoOrder()
    {
        ScannerRoomSnapshotPageQueue queue = new();
        queue.Enqueue(Page(firstRoomId, 0));
        queue.Enqueue(Page(firstRoomId, 1));
        queue.Enqueue(Page(firstRoomId, 2));
        List<ushort> processedPageIndexes = [];

        queue.Process(0, page => processedPageIndexes.Add(page.PageIndex)).Should().Be(0);
        queue.Process(1, page => processedPageIndexes.Add(page.PageIndex)).Should().Be(1);

        processedPageIndexes.Should().Equal((ushort)0);
        queue.Count.Should().Be(2);
        queue.HasQueuedPage(firstRoomId, 7).Should().BeTrue();

        queue.Process(1, page => processedPageIndexes.Add(page.PageIndex)).Should().Be(1);
        queue.Process(1, page => processedPageIndexes.Add(page.PageIndex)).Should().Be(1);

        processedPageIndexes.Should().Equal((ushort)0, (ushort)1, (ushort)2);
        queue.Count.Should().Be(0);
        queue.HasQueuedPage(firstRoomId, 7).Should().BeFalse();
    }

    [TestMethod]
    public void RemovingRoomDropsItsPagesAndPreservesOtherPageOrder()
    {
        ScannerRoomSnapshotPageQueue queue = new();
        queue.Enqueue(Page(firstRoomId, 0));
        queue.Enqueue(Page(secondRoomId, 0));
        queue.Enqueue(Page(firstRoomId, 1));
        queue.Enqueue(Page(secondRoomId, 1));
        List<ushort> processedPageIndexes = [];

        queue.RemoveRoom(firstRoomId).Should().Be(2);
        queue.HasQueuedPage(firstRoomId, 7).Should().BeFalse();
        queue.HasQueuedPage(secondRoomId, 7).Should().BeTrue();
        queue.Process(10, page => processedPageIndexes.Add(page.PageIndex)).Should().Be(2);

        processedPageIndexes.Should().Equal((ushort)0, (ushort)1);
        queue.Count.Should().Be(0);
    }

    [TestMethod]
    public void ClearDropsEveryQueuedPage()
    {
        ScannerRoomSnapshotPageQueue queue = new();
        queue.Enqueue(Page(firstRoomId, 0));
        queue.Enqueue(Page(secondRoomId, 0));

        queue.Clear();

        queue.Count.Should().Be(0);
        queue.HasQueuedPage(firstRoomId, 7).Should().BeFalse();
        queue.HasQueuedPage(secondRoomId, 7).Should().BeFalse();
        queue.Process(1, _ => Assert.Fail("Cleared page should not be processed")).Should().Be(0);
    }

    [TestMethod]
    public void RejectsNegativeBudgets()
    {
        ScannerRoomSnapshotPageQueue queue = new();

        Invoking(() => queue.Process(-1, _ => { })).Should().Throw<ArgumentOutOfRangeException>();
    }

    private static ScannerRoomSnapshotPage Page(NitroxId roomId, ushort pageIndex) =>
        new(
            roomId,
            7,
            ScannerRoomQueryStatus.Complete,
            300,
            null,
            42,
            pageIndex,
            3,
            [],
            []);
}
