namespace NitroxClient.GameLogic.ScannerRooms;

[TestClass]
public sealed class ScannerRoomRefreshSchedulerTest
{
    [TestMethod]
    public void RefreshesSelectedTargetsEveryFiveSecondsWithoutAnOpenUi()
    {
        ScannerRoomRefreshScheduler scheduler = new();
        scheduler.SetActivity(true, false, 100);

        scheduler.IsRefreshDue(104.999).Should().BeFalse();
        scheduler.IsRefreshDue(105).Should().BeTrue();

        scheduler.MarkRefreshed(105);
        scheduler.IsRefreshDue(109.999).Should().BeFalse();
        scheduler.IsRefreshDue(110).Should().BeTrue();
    }

    [TestMethod]
    public void RefreshesCatalogEveryTenSecondsOnlyWhileUiIsActive()
    {
        ScannerRoomRefreshScheduler scheduler = new();
        scheduler.SetActivity(false, true, 20);

        scheduler.IsRefreshDue(29.999).Should().BeFalse();
        scheduler.IsRefreshDue(30).Should().BeTrue();

        scheduler.MarkRefreshed(30);
        scheduler.SetActivity(false, false, 31);
        scheduler.IsRefreshDue(100).Should().BeFalse();
    }

    [TestMethod]
    public void OneRefreshCoalescesSelectedTargetAndCatalogDeadlines()
    {
        ScannerRoomRefreshScheduler scheduler = new();
        scheduler.SetActivity(true, true, 0);

        scheduler.IsRefreshDue(5).Should().BeTrue();
        scheduler.MarkRefreshed(5);

        scheduler.IsRefreshDue(9.999).Should().BeFalse();
        scheduler.IsRefreshDue(10).Should().BeTrue();
        scheduler.MarkRefreshed(10);
        scheduler.IsRefreshDue(14.999).Should().BeFalse();
    }

    [TestMethod]
    public void SuppressesOnlyImmediatelyRepeatedInteractionRefreshes()
    {
        ScannerRoomRefreshScheduler scheduler = new();

        scheduler.WasRefreshedRecently(10).Should().BeFalse();
        scheduler.MarkRefreshed(10);
        scheduler.WasRefreshedRecently(10.5).Should().BeTrue();
        scheduler.WasRefreshedRecently(11).Should().BeTrue();
        scheduler.WasRefreshedRecently(11.001).Should().BeFalse();
    }
}
