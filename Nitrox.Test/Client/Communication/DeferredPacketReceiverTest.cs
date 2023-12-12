using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nitrox.Test.Client.Communication;
using NitroxModel.Packets;

namespace NitroxClient.Communication;

[TestClass]
public class DeferredPacketReceiverTest
{
    private PacketReceiver packetReceiver;

    // Test Data
    private const ushort PLAYER_ID = 1;

    [TestInitialize]
    public void TestInitialize()
    {
        packetReceiver = new PacketReceiver();
        ClientAutoFacRegistrar registrar = new ClientAutoFacRegistrar();
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
