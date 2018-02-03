using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Packets;
using NitroxModel.Tcp;
using System.Collections.Generic;
using System.Linq;

namespace NitroxTest.Model.Tcp
{
    [TestClass]
    public class MessageBufferTest
    {
        private MessageBuffer messageBuffer;

        [TestInitialize]
        public void TestInitialize()
        {
            messageBuffer = new MessageBuffer();
        }

        [TestMethod]
        public void SinglePacket()
        {
            string playerId = "player1";
            TestNonActionPacket packet = new TestNonActionPacket(playerId);

            int length = WritePacketToReceivedBuffer(packet, 0);
            Queue<Packet> packets = GetResultingPackets(messageBuffer.GetReceivedPackets(length));

            Assert.AreEqual(1, packets.Count);
            Assert.AreEqual(playerId, ((TestNonActionPacket)packets.Dequeue()).PlayerId);
        }

        [TestMethod]
        public void MultiplePackets()
        {
            TestNonActionPacket packet1 = new TestNonActionPacket("Player1");
            TestNonActionPacket packet2 = new TestNonActionPacket("Player2");

            int length = WritePacketToReceivedBuffer(packet1, 0);
            length = WritePacketToReceivedBuffer(packet2, length);
            Queue<Packet> packets = GetResultingPackets(messageBuffer.GetReceivedPackets(length));

            Assert.AreEqual(2, packets.Count);
            Assert.AreEqual(packet1.PlayerId, ((TestNonActionPacket)packets.Dequeue()).PlayerId);
            Assert.AreEqual(packet2.PlayerId, ((TestNonActionPacket)packets.Dequeue()).PlayerId);
        }

        [TestMethod]
        public void ReceivedPartialPacket()
        {
            TestNonActionPacket packet1 = new TestNonActionPacket("Player1");

            int partialLengthReceived = MessageBuffer.HEADER_BYTE_SIZE;
            int length = WritePacketToReceivedBuffer(packet1, 0);

            Queue<Packet> packets = GetResultingPackets(messageBuffer.GetReceivedPackets(partialLengthReceived));
            Assert.AreEqual(0, packets.Count);

            messageBuffer.ReceivingBuffer = messageBuffer.ReceivingBuffer.Skip(partialLengthReceived).ToArray();

            packets = GetResultingPackets(messageBuffer.GetReceivedPackets(length - partialLengthReceived));
            Assert.AreEqual(1, packets.Count);
            Assert.AreEqual(packet1.PlayerId, ((TestNonActionPacket)packets.Dequeue()).PlayerId);
        }

        [TestMethod]
        public void MultipleWithPartial()
        {
            TestNonActionPacket packet1 = new TestNonActionPacket("Player1");
            TestNonActionPacket packet2 = new TestNonActionPacket("Player2");

            int length = WritePacketToReceivedBuffer(packet1, 0);
            length = WritePacketToReceivedBuffer(packet2, length);

            int fullWithPartialLength = length / 2 + 20;
            Queue<Packet> packets = GetResultingPackets(messageBuffer.GetReceivedPackets(fullWithPartialLength));
            Assert.AreEqual(1, packets.Count);
            Assert.AreEqual(packet1.PlayerId, ((TestNonActionPacket)packets.Dequeue()).PlayerId);

            messageBuffer.ReceivingBuffer = messageBuffer.ReceivingBuffer.Skip(fullWithPartialLength).ToArray();

            packets = GetResultingPackets(messageBuffer.GetReceivedPackets(length - fullWithPartialLength));
            Assert.AreEqual(1, packets.Count);
            Assert.AreEqual(packet2.PlayerId, ((TestNonActionPacket)packets.Dequeue()).PlayerId);
        }

        private Queue<Packet> GetResultingPackets(IEnumerable<Packet> packets)
        {
            Queue<Packet> results = new Queue<Packet>();

            foreach(Packet packet in packets)
            {
                results.Enqueue(packet);
            }

            return results;
        }

        private int WritePacketToReceivedBuffer(Packet packet, int offset)
        {
            byte[] packetData = packet.SerializeWithHeaderData();

            for(int i = 0; i < packetData.Length; i++)
            {
                messageBuffer.ReceivingBuffer[i + offset] = packetData[i];
            }

            return offset + packetData.Length;
        }
        
    }
}
