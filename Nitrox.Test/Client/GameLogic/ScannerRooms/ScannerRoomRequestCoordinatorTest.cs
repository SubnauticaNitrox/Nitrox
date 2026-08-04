using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace NitroxClient.GameLogic.ScannerRooms;

[TestClass]
public sealed class ScannerRoomRequestCoordinatorTest
{
    private readonly NitroxId firstRoomId = new("bfe45cb4-d4a5-48dc-869d-d72e2c4a4845");
    private readonly NitroxId secondRoomId = new("22a35e55-1656-431e-88d6-31a9ec478d62");

    [TestMethod]
    public void RetainsOneInFlightRequestWhenRequestsOverlap()
    {
        ScannerRoomRequestCoordinator coordinator = new();
        ScannerRoomRequestParameters first = Request(300, "Quartz", 1);
        ScannerRoomRequestParameters overlapping = Request(350, "Copper", 2);

        coordinator.EnqueueOrReplace(firstRoomId, first, out ScannerRoomDispatch dispatch).Should().BeTrue();
        dispatch.Should().Be(new ScannerRoomDispatch(firstRoomId, first));
        coordinator.ConfirmDispatch(firstRoomId, 1, 0).Should().BeTrue();
        coordinator.EnqueueOrReplace(firstRoomId, overlapping, out _).Should().BeFalse();

        coordinator.ObserveResponse(firstRoomId, 1, ScannerRoomSnapshotApplyResult.Ignored, 0.2, out _).Should().BeFalse();
        coordinator.ObserveResponse(firstRoomId, 1, ScannerRoomSnapshotApplyResult.WaitingForPages, 0.3, out _).Should().BeFalse();
        coordinator.ObserveResponse(firstRoomId, 1, ScannerRoomSnapshotApplyResult.Applied, 1, out dispatch).Should().BeTrue();
        dispatch.Should().Be(new ScannerRoomDispatch(firstRoomId, overlapping));
    }

    [TestMethod]
    public void CoalescesOverlappingRequestsToLatestParameters()
    {
        ScannerRoomRequestCoordinator coordinator = new();
        ScannerRoomRequestParameters first = Request(300, "Quartz", 1);
        ScannerRoomRequestParameters superseded = Request(350, "Copper", 2);
        ScannerRoomRequestParameters latest = Request(500, "Diamond", 3);

        coordinator.EnqueueOrReplace(firstRoomId, first, out _).Should().BeTrue();
        coordinator.ConfirmDispatch(firstRoomId, 1, 0).Should().BeTrue();
        coordinator.EnqueueOrReplace(firstRoomId, superseded, out _).Should().BeFalse();
        coordinator.EnqueueOrReplace(firstRoomId, latest, out _).Should().BeFalse();

        coordinator.ObserveResponse(firstRoomId, 1, ScannerRoomSnapshotApplyResult.Applied, 1, out ScannerRoomDispatch dispatch).Should().BeTrue();
        dispatch.Should().Be(new ScannerRoomDispatch(firstRoomId, latest));
    }

    [DataTestMethod]
    [DataRow(ScannerRoomSnapshotApplyResult.Applied)]
    [DataRow(ScannerRoomSnapshotApplyResult.NotModified)]
    [DataRow(ScannerRoomSnapshotApplyResult.Failed)]
    public void DispatchesQueuedRequestAfterEveryTerminalResponse(ScannerRoomSnapshotApplyResult terminalResult)
    {
        ScannerRoomRequestCoordinator coordinator = new();
        ScannerRoomRequestParameters first = Request(300, "Quartz", 1);
        ScannerRoomRequestParameters queued = Request(350, "Copper", 2);

        coordinator.EnqueueOrReplace(firstRoomId, first, out _).Should().BeTrue();
        coordinator.ConfirmDispatch(firstRoomId, 1, 0).Should().BeTrue();
        coordinator.EnqueueOrReplace(firstRoomId, queued, out _).Should().BeFalse();

        coordinator.ObserveResponse(firstRoomId, 1, terminalResult, 1, out ScannerRoomDispatch dispatch).Should().BeTrue();
        dispatch.Should().Be(new ScannerRoomDispatch(firstRoomId, queued));
    }

    [TestMethod]
    public void CorrelatesTerminalResponseToActiveRequestId()
    {
        ScannerRoomRequestCoordinator coordinator = new();
        ScannerRoomRequestParameters first = Request(300, "Quartz", 1);
        ScannerRoomRequestParameters queued = Request(350, "Copper", 2);

        coordinator.EnqueueOrReplace(firstRoomId, first, out _).Should().BeTrue();
        coordinator.ConfirmDispatch(firstRoomId, 7, 0).Should().BeTrue();
        coordinator.EnqueueOrReplace(firstRoomId, queued, out _).Should().BeFalse();

        coordinator.ObserveResponse(firstRoomId, 6, ScannerRoomSnapshotApplyResult.Applied, 1, out _).Should().BeFalse();
        coordinator.ObserveResponse(firstRoomId, 7, ScannerRoomSnapshotApplyResult.Applied, 1, out ScannerRoomDispatch dispatch).Should().BeTrue();
        dispatch.Should().Be(new ScannerRoomDispatch(firstRoomId, queued));
    }

    [TestMethod]
    public void DifferentRoomsMaintainIndependentInFlightRequests()
    {
        ScannerRoomRequestCoordinator coordinator = new();
        ScannerRoomRequestParameters first = Request(300, "Quartz", 1);
        ScannerRoomRequestParameters second = Request(500, "Diamond", 2);

        coordinator.EnqueueOrReplace(firstRoomId, first, out ScannerRoomDispatch firstDispatch).Should().BeTrue();
        coordinator.EnqueueOrReplace(secondRoomId, second, out ScannerRoomDispatch secondDispatch).Should().BeTrue();

        firstDispatch.Should().Be(new ScannerRoomDispatch(firstRoomId, first));
        secondDispatch.Should().Be(new ScannerRoomDispatch(secondRoomId, second));
    }

    [TestMethod]
    public void AbortedSendPromotesQueuedRequestAndReleasesWhenEmpty()
    {
        ScannerRoomRequestCoordinator coordinator = new();
        ScannerRoomRequestParameters first = Request(300, "Quartz", 1);
        ScannerRoomRequestParameters queued = Request(500, "Diamond", 2);

        coordinator.EnqueueOrReplace(firstRoomId, first, out _).Should().BeTrue();
        coordinator.ConfirmDispatch(firstRoomId, 7, 0).Should().BeTrue();
        coordinator.EnqueueOrReplace(firstRoomId, queued, out _).Should().BeFalse();

        coordinator.AbortDispatch(firstRoomId, 6, out _).Should().BeFalse();
        coordinator.AbortDispatch(firstRoomId, 7, out ScannerRoomDispatch dispatch).Should().BeTrue();
        dispatch.Should().Be(new ScannerRoomDispatch(firstRoomId, queued));
        coordinator.ConfirmDispatch(firstRoomId, 8, 1).Should().BeTrue();
        coordinator.AbortDispatch(firstRoomId, 8, out _).Should().BeFalse();

        coordinator.EnqueueOrReplace(firstRoomId, first, out dispatch).Should().BeTrue();
        dispatch.Should().Be(new ScannerRoomDispatch(firstRoomId, first));
    }

    [TestMethod]
    public void MissingTerminalResponseExpiresAndPromotesLatestRequest()
    {
        ScannerRoomRequestCoordinator coordinator = new();
        ScannerRoomRequestParameters first = Request(300, "Quartz", 1);
        ScannerRoomRequestParameters latest = Request(500, "Diamond", 2);

        coordinator.EnqueueOrReplace(firstRoomId, first, out _).Should().BeTrue();
        coordinator.ConfirmDispatch(firstRoomId, 7, 0).Should().BeTrue();
        coordinator.EnqueueOrReplace(firstRoomId, latest, out _).Should().BeFalse();

        coordinator.TryExpire(firstRoomId, ScannerRoomRequestCoordinator.REQUEST_TIMEOUT_SECONDS - 0.001, out _, out _).Should().BeFalse();
        coordinator.TryExpire(
            firstRoomId,
            ScannerRoomRequestCoordinator.REQUEST_TIMEOUT_SECONDS,
            out ScannerRoomExpiredRequest expired,
            out ScannerRoomDispatch dispatch).Should().BeTrue();
        expired.Should().Be(new ScannerRoomExpiredRequest(firstRoomId, 7));
        dispatch.Should().Be(new ScannerRoomDispatch(firstRoomId, latest));
    }

    [TestMethod]
    public void PartialPageExtendsMissingTerminalTimeout()
    {
        ScannerRoomRequestCoordinator coordinator = new();
        ScannerRoomRequestParameters request = Request(300, "Quartz", 1);

        coordinator.EnqueueOrReplace(firstRoomId, request, out _).Should().BeTrue();
        coordinator.ConfirmDispatch(firstRoomId, 1, 0).Should().BeTrue();
        coordinator.ObserveResponse(
            firstRoomId,
            1,
            ScannerRoomSnapshotApplyResult.WaitingForPages,
            ScannerRoomRequestCoordinator.REQUEST_TIMEOUT_SECONDS - 1,
            out _).Should().BeFalse();

        coordinator.TryExpire(firstRoomId, ScannerRoomRequestCoordinator.REQUEST_TIMEOUT_SECONDS, out _, out _).Should().BeFalse();
        coordinator.TryExpire(
            firstRoomId,
            ScannerRoomRequestCoordinator.REQUEST_TIMEOUT_SECONDS * 2 - 1,
            out ScannerRoomExpiredRequest expired,
            out ScannerRoomDispatch retry).Should().BeTrue();
        expired.Should().Be(new ScannerRoomExpiredRequest(firstRoomId, 1));
        retry.Should().Be(new ScannerRoomDispatch(firstRoomId, request));
    }

    private static ScannerRoomRequestParameters Request(float range, string techType, float origin) =>
        new(range, new NitroxTechType(techType), new NitroxVector3(origin, 0, 0));
}
