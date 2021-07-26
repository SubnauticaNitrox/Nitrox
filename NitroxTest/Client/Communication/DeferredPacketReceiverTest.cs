using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.Communication;
using NitroxClient.Map;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxTest.Model;
using UnityEngine;

namespace NitroxTest.Client.Communication
{
    [TestClass]
    public class DeferredPacketReceiverTest
    {
        private readonly VisibleCells visibleCells = new VisibleCells();
        private PacketReceiver packetReceiver;

        // Test Data
        private const ushort PLAYER_ID = 1;
        private const int CELL_LEVEL = 3;
        private readonly Vector3 loadedActionPosition = new Vector3(50, 50, 50);
        private readonly Vector3 unloadedActionPosition = new Vector3(200, 200, 200);
        private AbsoluteEntityCell loadedCell;
        private AbsoluteEntityCell unloadedCell;
        private Int3 cellId = Int3.zero;

        [TestInitialize]
        public void TestInitialize()
        {
            packetReceiver = new PacketReceiver();
            NitroxModel.Helper.Map.Main = new NitroxModel_Subnautica.Helper.SubnauticaMap();

            loadedCell = new AbsoluteEntityCell(loadedActionPosition, CELL_LEVEL);
            unloadedCell = new AbsoluteEntityCell(unloadedActionPosition, CELL_LEVEL);

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
    }
}
