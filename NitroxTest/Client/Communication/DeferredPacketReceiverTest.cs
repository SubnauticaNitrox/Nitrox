using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.Communication;
using NitroxClient.Map;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxTest.Model;
using UnityEngine;

namespace NitroxTest.Client.Communication
{
    [TestClass]
    public class DeferredPacketReceiverTest
    {
        private VisibleCells visibleCells;
        private DeferringPacketReceiver packetReceiver;

        // Test Data
        private const string PLAYER_ID = "TestPlayer";
        private readonly Vector3 loadedActionPosition = new Vector3(50, 50, 50);
        private readonly Vector3 unloadedActionPosition = new Vector3(200, 200, 200);
        private AbsoluteEntityCell loadedCell;
        private AbsoluteEntityCell unloadedCell;
        private Int3 cellId = Int3.zero;

        [TestInitialize]
        public void TestInitialize()
        {
            packetReceiver = new DeferringPacketReceiver(visibleCells);

            Int3 loadedBatchId = LargeWorldStreamer.main.GetContainingBatch(loadedActionPosition);
            Int3 unloadedBatchId = LargeWorldStreamer.main.GetContainingBatch(unloadedActionPosition);

            loadedCell = new AbsoluteEntityCell(loadedBatchId, cellId, 3);
            unloadedCell = new AbsoluteEntityCell(unloadedBatchId, cellId, 3);

            visibleCells.Add(loadedCell);
        }

        [TestMethod]
        public void NonActionPacket()
        {
            Packet packet = new TestNonActionPacket(PLAYER_ID);
            packetReceiver.PacketReceived(packet);

            Queue<Packet> packets = packetReceiver.GetReceivedPackets();

            Assert.AreEqual(1, packets.Count);
            Assert.AreEqual(packet, packets.Dequeue());
        }

        [TestMethod]
        public void ActionPacketInLoadedCell()
        {
            Packet packet = new TestActionPacket(loadedActionPosition);
            packetReceiver.PacketReceived(packet);

            Queue<Packet> packets = packetReceiver.GetReceivedPackets();

            Assert.AreEqual(1, packets.Count);
            Assert.AreEqual(packet, packets.Dequeue());
        }

        [TestMethod]
        public void ActionPacketInUnloadedCell()
        {
            Packet packet = new TestActionPacket(unloadedActionPosition);
            packetReceiver.PacketReceived(packet);

            Queue<Packet> packets = packetReceiver.GetReceivedPackets();

            Assert.AreEqual(0, packets.Count);
        }

        [TestMethod]
        public void PacketPrioritizedAfterBeingDeferred()
        {
            Packet packet1 = new TestActionPacket(unloadedActionPosition);
            packetReceiver.PacketReceived(packet1);

            Assert.AreEqual(0, packetReceiver.GetReceivedPackets().Count);

            Packet packet2 = new TestNonActionPacket(PLAYER_ID);
            packetReceiver.PacketReceived(packet2);

            visibleCells.Add(unloadedCell);
            packetReceiver.CellLoaded(unloadedCell);

            Queue<Packet> packets = packetReceiver.GetReceivedPackets();

            Assert.AreEqual(2, packets.Count);
            Assert.AreEqual(packet1, packets.Dequeue());
            Assert.AreEqual(packet2, packets.Dequeue());
        }
    }
}
