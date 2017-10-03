using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.Communication;
using NitroxClient.Map;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxTest.Model;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxTest.Client.Communication
{
    [TestClass]
    public class ChunkAwarePacketReceiverTest
    {
        private LoadedChunks loadedChunks;
        private ChunkAwarePacketReceiver packetReceiver;

        // Test Data
        private String playerId = "TestPlayer";
        private Vector3 loadedActionPosition = new Vector3(50, 50, 50);
        private Vector3 unloadedActionPosition = new Vector3(200, 200, 200);
        private Chunk loadedChunk;
        private Chunk unloadedChunk;

        [TestInitialize]
        public void TestInitialize()
        {
            packetReceiver = new ChunkAwarePacketReceiver(loadedChunks);

            Int3 loadedBatchId = LargeWorldStreamer.main.GetContainingBatch(loadedActionPosition);
            Int3 unloadedBatchId = LargeWorldStreamer.main.GetContainingBatch(unloadedActionPosition);

            loadedChunk = new Chunk(loadedBatchId, 3);
            unloadedChunk = new Chunk(unloadedBatchId, 3);

            loadedChunks.Add(loadedChunk);
        }

        [TestMethod]
        public void NonActionPacket()
        {
            Packet packet = new TestNonActionPacket(playerId);
            packetReceiver.PacketReceived(packet);

            Queue<Packet> packets = packetReceiver.GetReceivedPackets();

            Assert.AreEqual(1, packets.Count);
            Assert.AreEqual(packet, packets.Dequeue());
        }

        [TestMethod]
        public void ActionPacketInLoadedChunk()
        {
            Packet packet = new TestActionPacket(playerId, loadedActionPosition);
            packetReceiver.PacketReceived(packet);

            Queue<Packet> packets = packetReceiver.GetReceivedPackets();

            Assert.AreEqual(1, packets.Count);
            Assert.AreEqual(packet, packets.Dequeue());
        }

        [TestMethod]
        public void ActionPacketInUnloadedChunk()
        {
            Packet packet = new TestActionPacket(playerId, unloadedActionPosition);
            packetReceiver.PacketReceived(packet);

            Queue<Packet> packets = packetReceiver.GetReceivedPackets();

            Assert.AreEqual(0, packets.Count);
        }

        [TestMethod]
        public void PacketPrioritizedAfterBeingDeferred()
        {
            Packet packet1 = new TestActionPacket(playerId, unloadedActionPosition);
            packetReceiver.PacketReceived(packet1);

            Assert.AreEqual(0, packetReceiver.GetReceivedPackets().Count);

            Packet packet2 = new TestNonActionPacket(playerId);
            packetReceiver.PacketReceived(packet2);

            loadedChunks.Add(unloadedChunk);
            packetReceiver.ChunkLoaded(unloadedChunk);

            Queue<Packet> packets = packetReceiver.GetReceivedPackets();

            Assert.AreEqual(2, packets.Count);
            Assert.AreEqual(packet1, packets.Dequeue());
            Assert.AreEqual(packet2, packets.Dequeue());
        }
    }
}
