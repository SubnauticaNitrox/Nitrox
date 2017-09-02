using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient;
using NitroxClient.Communication;
using NitroxModel.Packets;
using NitroxTest.Model;
using System;
using NitroxClient.Map;
using System.Collections.Generic;
using NitroxModel.DataStructures;

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
        private Int3 loadedChunk;
        private Int3 unloadedChunk;

        [TestInitialize]
        public void TestInitialize()
        {
            loadedChunks = new LoadedChunks();
            packetReceiver = new ChunkAwarePacketReceiver(loadedChunks);
            
            loadedChunk = loadedChunks.GetChunk(loadedActionPosition);
            unloadedChunk = loadedChunks.GetChunk(unloadedActionPosition);

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
