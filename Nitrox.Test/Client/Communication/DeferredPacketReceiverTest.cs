using Nitrox.Test.Client.Communication;
using NitroxModel.Packets;

namespace NitroxClient.Communication;

[TestClass]
public class DeferredPacketReceiverTest
{
    [TestMethod]
    public void NonActionPacket()
    {
        // Arrange
        const ushort PLAYER_ID = 1;
        TestNonActionPacket packet = new(PLAYER_ID);
        PacketReceiver packetReceiver = new();

        // Act
        packetReceiver.Add(packet);
        Packet storedPacket = packetReceiver.GetNextPacket();

        // Assert
        storedPacket.Should().NotBeNull();
        packetReceiver.GetNextPacket().Should().BeNull();
        storedPacket.Should().Be(packet);
        packet.PlayerId.Should().Be(PLAYER_ID);
    }
}
