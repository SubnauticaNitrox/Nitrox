using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using NitroxClient.GameLogic.ScannerRooms;

namespace Nitrox.Test.Client.GameLogic.ScannerRooms;

[TestClass]
public sealed class ScannerRoomLocalIntentTrackerTest
{
    [TestMethod]
    public void FirstCancellationIsAnIntentAndNestedCancellationIsDeduplicated()
    {
        ScannerRoomLocalIntentTracker tracker = new();

        tracker.TryBegin(null).Should().BeTrue();
        tracker.TryBegin(null).Should().BeFalse();
    }

    [TestMethod]
    public void ClearAllowsTheSameCanonicalIntentToBeSentAgain()
    {
        ScannerRoomLocalIntentTracker tracker = new();
        NitroxTechType quartz = new("Quartz");
        tracker.TryBegin(quartz).Should().BeTrue();

        tracker.Clear();

        tracker.TryBegin(new NitroxTechType("Quartz")).Should().BeTrue();
    }

    [TestMethod]
    public void AChangedSelectionSupersedesThePendingIntent()
    {
        ScannerRoomLocalIntentTracker tracker = new();

        tracker.TryBegin(new NitroxTechType("Quartz")).Should().BeTrue();
        tracker.TryBegin(new NitroxTechType("Copper")).Should().BeTrue();
        tracker.TryBegin(new NitroxTechType("Copper")).Should().BeFalse();
    }
}
