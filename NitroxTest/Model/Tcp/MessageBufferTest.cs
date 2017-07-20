using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Packets;
using NitroxModel.Tcp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

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
            String playerId = "player1";
            PlayerPacket packet = new TestNonActionPacket(playerId);

            int length = WritePacketToReceivedBuffer(packet, 0);
            Queue<PlayerPacket> packets = GetResultingPackets(messageBuffer.GetReceivedPackets(length));

            Assert.AreEqual(1, packets.Count);
            Assert.AreEqual(playerId, packets.Dequeue().PlayerId);
        }

        [TestMethod]
        public void MultiplePackets()
        {
            PlayerPacket packet1 = new TestNonActionPacket("Player1");
            PlayerPacket packet2 = new TestNonActionPacket("Player2");

            int length = WritePacketToReceivedBuffer(packet1, 0);
            length = WritePacketToReceivedBuffer(packet2, length);
            Queue<PlayerPacket> packets = GetResultingPackets(messageBuffer.GetReceivedPackets(length));

            Assert.AreEqual(2, packets.Count);
            Assert.AreEqual(packet1.PlayerId, packets.Dequeue().PlayerId);
            Assert.AreEqual(packet2.PlayerId, packets.Dequeue().PlayerId);
        }

        [TestMethod]
        public void receivedPartialPacket()
        {
            PlayerPacket packet1 = new TestNonActionPacket("Player1");

            int partialLengthReceived = 2;
            int length = WritePacketToReceivedBuffer(packet1, 0);

            Queue<PlayerPacket> packets = GetResultingPackets(messageBuffer.GetReceivedPackets(partialLengthReceived));
            Assert.AreEqual(0, packets.Count);

            messageBuffer.ReceivingBuffer = messageBuffer.ReceivingBuffer.Skip(partialLengthReceived).ToArray();

            packets = GetResultingPackets(messageBuffer.GetReceivedPackets(length - partialLengthReceived));
            Assert.AreEqual(1, packets.Count);
            Assert.AreEqual(packet1.PlayerId, packets.Dequeue().PlayerId);
        }

        [TestMethod]
        public void MultipleWithPartial()
        {
            PlayerPacket packet1 = new TestNonActionPacket("Player1");
            PlayerPacket packet2 = new TestNonActionPacket("Player2");

            int length = WritePacketToReceivedBuffer(packet1, 0);
            length = WritePacketToReceivedBuffer(packet2, length);

            int fullWithPartialLength = length / 2 + 20;
            Queue<PlayerPacket> packets = GetResultingPackets(messageBuffer.GetReceivedPackets(fullWithPartialLength));
            Assert.AreEqual(1, packets.Count);
            Assert.AreEqual(packet1.PlayerId, packets.Dequeue().PlayerId);

            messageBuffer.ReceivingBuffer = messageBuffer.ReceivingBuffer.Skip(fullWithPartialLength).ToArray();

            packets = GetResultingPackets(messageBuffer.GetReceivedPackets(length - fullWithPartialLength));
            Assert.AreEqual(1, packets.Count);
            Assert.AreEqual(packet2.PlayerId, packets.Dequeue().PlayerId);
        }

        private Queue<PlayerPacket> GetResultingPackets(IEnumerable<Packet> packets)
        {
            Queue<PlayerPacket> results = new Queue<PlayerPacket>();

            foreach(PlayerPacket packet in packets)
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
