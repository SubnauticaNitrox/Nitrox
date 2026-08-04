using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

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

        trigger.TryRequestInitial(300, quartz, origin).Should().BeTrue();
        trigger.TryRequestInitial(300, quartz, origin).Should().BeFalse();

        requests.Should().ContainSingle().Which.Should().Be(new Request(300, quartz, origin));
    }

    [TestMethod]
    public void ImmediateInteractionsAlwaysIssueARequest()
    {
        List<Request> requests = [];
        ScannerRoomRequestTrigger trigger = CreateTrigger(requests);

        trigger.RequestImmediate(300, quartz, origin);
        trigger.RequestImmediate(300, quartz, origin);

        requests.Should().HaveCount(2);
        trigger.TryRequestInitial(300, quartz, origin).Should().BeFalse();
    }

    private static ScannerRoomRequestTrigger CreateTrigger(ICollection<Request> requests) =>
        new((range, selectedTechType, observedOrigin) => requests.Add(new Request(range, selectedTechType, observedOrigin)));

    private sealed record Request(float Range, NitroxTechType? SelectedTechType, NitroxVector3? ObservedOrigin);
}
