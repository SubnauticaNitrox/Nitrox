using NitroxClient.GameLogic.Spawning.Bases;

namespace Nitrox.Test.Client.GameLogic.Spawning.Bases;

[TestClass]
public sealed class InteriorPieceEntitySpawnerTest
{
    [TestMethod]
    public void CompleteMapRoomRestoreAssignsIdBeforeSetupExactlyOnce()
    {
        List<string> operations = [];
        int idAssignments = 0;
        int setupCalls = 0;

        InteriorPieceEntitySpawner.CompleteMapRoomRestore(
            () =>
            {
                idAssignments++;
                operations.Add("assign-id");
            },
            () =>
            {
                setupCalls++;
                operations.Add("setup-scanner-room");
            });

        idAssignments.Should().Be(1);
        setupCalls.Should().Be(1);
        operations.Should().Equal("assign-id", "setup-scanner-room");
    }
}
