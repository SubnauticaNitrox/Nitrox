using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;
using NitroxClient.GameLogic.ScannerRooms;

namespace Nitrox.Test.Client.GameLogic.ScannerRooms;

[TestClass]
public sealed class ScannerRoomTargetPreparationTest
{
    private readonly NitroxId roomId = new("c2759b41-36e5-442d-8772-85fc6b92918e");
    private readonly NitroxTechType quartz = new("Quartz");

    [TestMethod]
    public void VisitsAtMostOneBudgetAndWithholdsPartialResourcesUntilComplete()
    {
        using ScannerRoomTargetPreparation<string> preparation = new(Snapshot(600));
        int prepareCalls = 0;

        preparation.Advance(256, target =>
        {
            prepareCalls++;
            return target.TrackerIndex.ToString();
        }).Should().Be(256);

        prepareCalls.Should().Be(256);
        preparation.IsComplete.Should().BeFalse();
        preparation.TryTakeCompleted(out _).Should().BeFalse();

        preparation.Advance(256, target => target.TrackerIndex.ToString()).Should().Be(256);
        preparation.Advance(256, target => target.TrackerIndex.ToString()).Should().Be(88);

        preparation.IsComplete.Should().BeTrue();
        preparation.TryTakeCompleted(out List<string>? resources).Should().BeTrue();
        resources.Should().HaveCount(600);
        preparation.TryTakeCompleted(out _).Should().BeFalse();
    }

    [TestMethod]
    public void CancelledPreparationCannotPublishStalePartialResources()
    {
        ScannerRoomTargetPreparation<string> stale = new(Snapshot(3));
        stale.Advance(1, target => target.TrackerIndex.ToString()).Should().Be(1);

        stale.Cancel();

        stale.Advance(3, target => target.TrackerIndex.ToString()).Should().Be(0);
        stale.TryTakeCompleted(out _).Should().BeFalse();

        using ScannerRoomTargetPreparation<string> replacement = new(Snapshot(1, revision: 2));
        replacement.Advance(1, _ => "replacement").Should().Be(1);
        replacement.TryTakeCompleted(out List<string>? resources).Should().BeTrue();
        resources.Should().Equal("replacement");
    }

    [TestMethod]
    public void DuplicateTargetsConsumeBudgetButProduceOnePreparedResource()
    {
        ScannerResourceTarget target = Target(1);
        ScannerRoomSnapshot snapshot = new(roomId, 300, quartz, 1, [], [target, target]);
        using ScannerRoomTargetPreparation<string> preparation = new(snapshot);

        preparation.Advance(1, _ => "first").Should().Be(1);
        preparation.TryTakeCompleted(out _).Should().BeFalse();
        preparation.Advance(1, _ => "duplicate").Should().Be(1);

        preparation.TryTakeCompleted(out List<string>? resources).Should().BeTrue();
        resources.Should().Equal("first");
    }

    [TestMethod]
    public void EmptySnapshotIsImmediatelyReadyForAtomicPublication()
    {
        using ScannerRoomTargetPreparation<string> preparation = new(Snapshot(0));

        preparation.IsComplete.Should().BeTrue();
        preparation.Advance(1, _ => throw new InvalidOperationException("No target should be prepared")).Should().Be(0);
        preparation.TryTakeCompleted(out List<string>? resources).Should().BeTrue();
        resources.Should().BeEmpty();
    }

    private ScannerRoomSnapshot Snapshot(int targetCount, ulong revision = 1) =>
        new(
            roomId,
            300,
            quartz,
            revision,
            [new ScannerResourceSummary(quartz, targetCount)],
            Enumerable.Range(1, targetCount).Select(index => Target((ushort)index)).ToList());

    private ScannerResourceTarget Target(ushort trackerIndex) =>
        new(
            new NitroxId($"00000000-0000-0000-0000-{trackerIndex:D12}"),
            trackerIndex,
            quartz,
            new NitroxVector3(trackerIndex, 0, 0));
}
