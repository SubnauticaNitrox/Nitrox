using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

namespace NitroxClient.GameLogic.ScannerRooms;

[TestClass]
public sealed class ScannerRoomRequestTriggerTest
{
    private readonly NitroxTechType quartz = new("Quartz");
    private readonly NitroxVector3 origin = new(1, 2, 3);

    [TestMethod]
    public void InitialRequestIsOnlyIssuedOnce()
    {
        List<Request> requests = [];
        ScannerRoomRequestTrigger trigger = CreateTrigger(requests);
        ScannerRoomScanState scanState = new(quartz, 1);

        trigger.TryRequestInitial(300, scanState, origin).Should().BeTrue();
        trigger.TryRequestInitial(300, scanState, origin).Should().BeFalse();

        requests.Should().ContainSingle().Which.Should().Be(new Request(300, scanState, origin));
    }

    [TestMethod]
    public void ImmediateInteractionsAlwaysIssueARequest()
    {
        List<Request> requests = [];
        ScannerRoomRequestTrigger trigger = CreateTrigger(requests);
        ScannerRoomScanState scanState = new(quartz, 1);

        trigger.RequestImmediate(300, scanState, origin);
        trigger.RequestImmediate(300, scanState, origin);

        requests.Should().HaveCount(2);
        trigger.TryRequestInitial(300, scanState, origin).Should().BeFalse();
    }

    [TestMethod]
    public void StateChangesAreDeduplicatedAfterNormalization()
    {
        List<Request> requests = [];
        ScannerRoomRequestTrigger trigger = CreateTrigger(requests);

        trigger.TryRequestIfChanged(349, new ScannerRoomScanState(NitroxTechType.None, 0), origin).Should().BeTrue();
        trigger.TryRequestIfChanged(300, new ScannerRoomScanState(null, 0), origin).Should().BeFalse();
        trigger.TryRequestIfChanged(350, ScannerRoomScanState.Empty, origin).Should().BeTrue();
        trigger.TryRequestIfChanged(350, new ScannerRoomScanState(quartz, 1), origin).Should().BeTrue();
        trigger.TryRequestIfChanged(399, new ScannerRoomScanState(new NitroxTechType("Quartz"), 1), origin).Should().BeFalse();
        trigger.TryRequestIfChanged(399, new ScannerRoomScanState(quartz, 2), origin).Should().BeTrue();

        requests.Should().HaveCount(4);
    }

    private static ScannerRoomRequestTrigger CreateTrigger(ICollection<Request> requests) =>
        new((range, scanState, observedOrigin) => requests.Add(new Request(range, scanState, observedOrigin)));

    private sealed record Request(float Range, ScannerRoomScanState ScanState, NitroxVector3? ObservedOrigin);
}
