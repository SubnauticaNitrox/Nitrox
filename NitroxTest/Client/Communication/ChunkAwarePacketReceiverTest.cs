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
    public class ChunkAwarePacketReceiverTest
    {
        private readonly LoadedChunks loadedChunks = new LoadedChunks();
        private ChunkAwarePacketReceiver packetReceiver;

        // Test Data
        private const string PLAYER_ID = "TestPlayer";
        private readonly Vector3 loadedActionPosition = new Vector3(50, 50, 50);
        private readonly Vector3 unloadedActionPosition = new Vector3(200, 200, 200);
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
            Packet packet = new TestNonActionPacket(PLAYER_ID);
            packetReceiver.PacketReceived(packet);

            Queue<Packet> packets = packetReceiver.GetReceivedPackets();

            Assert.AreEqual(1, packets.Count);
            Assert.AreEqual(packet, packets.Dequeue());
        }

        [TestMethod]
        public void ActionPacketInLoadedChunk()
        {
            Packet packet = new TestActionPacket(PLAYER_ID, loadedActionPosition);
            packetReceiver.PacketReceived(packet);

            Queue<Packet> packets = packetReceiver.GetReceivedPackets();

            Assert.AreEqual(1, packets.Count);
            Assert.AreEqual(packet, packets.Dequeue());
        }

        [TestMethod]
        public void ActionPacketInUnloadedChunk()
        {
            Packet packet = new TestActionPacket(PLAYER_ID, unloadedActionPosition);
            packetReceiver.PacketReceived(packet);

            Queue<Packet> packets = packetReceiver.GetReceivedPackets();

            Assert.AreEqual(0, packets.Count);
        }

        [TestMethod]
        public void PacketPrioritizedAfterBeingDeferred()
        {
            Packet packet1 = new TestActionPacket(PLAYER_ID, unloadedActionPosition);
            packetReceiver.PacketReceived(packet1);

            Assert.AreEqual(0, packetReceiver.GetReceivedPackets().Count);

            Packet packet2 = new TestNonActionPacket(PLAYER_ID);
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
