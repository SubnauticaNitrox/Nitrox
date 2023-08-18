using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nitrox.Test.Client.Communication;
using NitroxClient.Map;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Packets;

namespace NitroxClient.Communication;

[TestClass]
public class DeferredPacketReceiverTest
{
    private readonly VisibleCells visibleCells = new();
    private PacketReceiver packetReceiver;

    // Test Data
    private const ushort PLAYER_ID = 1;
    private const int CELL_LEVEL = 3;
    private readonly NitroxVector3 loadedActionPosition = new(50, 50, 50);
    private AbsoluteEntityCell loadedCell;

    [TestInitialize]
    public void TestInitialize()
    {
        packetReceiver = new PacketReceiver();
        loadedCell = new AbsoluteEntityCell(loadedActionPosition, CELL_LEVEL);
        visibleCells.Add(loadedCell);
    }

    [TestMethod]
    public void NonActionPacket()
    {
        TestNonActionPacket packet = new(PLAYER_ID);
        packetReceiver.Add(packet);

        Packet storedPacket = packetReceiver.GetNextPacket();
        Assert.IsNotNull(storedPacket);
        Assert.IsNull(packetReceiver.GetNextPacket());
        Assert.AreEqual(packet, storedPacket);
        Assert.AreEqual(packet.PlayerId, PLAYER_ID);
    }
}
