using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic.Spawning.Bases;

namespace Nitrox.Test.Client.GameLogic.Spawning.Bases;

[TestClass]
public sealed class BuildingPostSpawnerTest
{
    [TestMethod]
    public void ScannerRoomSetupPrefersAssignedModuleIdOverBaseFallbackId()
    {
        NitroxId baseId = new("902f69a0-9159-4591-846b-dc234bd6a3c3");
        NitroxId mapRoomId = baseId.Increment();

        BuildingPostSpawner.ResolveScannerRoomId(baseId, mapRoomId).Should().Be(mapRoomId);
        BuildingPostSpawner.ResolveScannerRoomId(baseId, null).Should().Be(baseId);
    }
}
