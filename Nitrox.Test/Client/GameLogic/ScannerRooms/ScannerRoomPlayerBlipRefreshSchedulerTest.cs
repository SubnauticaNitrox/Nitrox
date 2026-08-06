namespace NitroxClient.GameLogic.ScannerRooms;

[TestClass]
public sealed class ScannerRoomPlayerBlipRefreshSchedulerTest
{
    [TestMethod]
    public void RefreshesPlayerBlipsEveryThreeSeconds()
    {
        ScannerRoomPlayerBlipRefreshScheduler scheduler = new();

        scheduler.IsRefreshDue(100).Should().BeTrue();
        scheduler.MarkRefreshed(100);

        scheduler.IsRefreshDue(102.999).Should().BeFalse();
        scheduler.IsRefreshDue(103).Should().BeTrue();

        scheduler.MarkRefreshed(103);
        scheduler.IsRefreshDue(105.999).Should().BeFalse();
        scheduler.IsRefreshDue(106).Should().BeTrue();
    }

    [TestMethod]
    public void VanillaRefreshRestartsIndependentPlayerBlipCadence()
    {
        ScannerRoomPlayerBlipRefreshScheduler scheduler = new();

        scheduler.MarkRefreshed(100);
        scheduler.MarkRefreshed(101);

        scheduler.IsRefreshDue(103.999).Should().BeFalse();
        scheduler.IsRefreshDue(104).Should().BeTrue();
    }
}
