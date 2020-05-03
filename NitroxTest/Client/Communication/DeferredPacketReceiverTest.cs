using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.Communication;
using NitroxClient.Map;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using NitroxTest.Model;

namespace NitroxTest.Client.Communication
{
    [TestClass]
    public class DeferredPacketReceiverTest
    {
        // Test Data
        private const ushort PLAYER_ID = 1;
        private const int CELL_LEVEL = 3;
        private readonly Vector3 loadedActionPosition = new Vector3(50, 50, 50);
        private readonly Vector3 unloadedActionPosition = new Vector3(200, 200, 200);
        private readonly VisibleCells visibleCells = new VisibleCells();
        private AbsoluteEntityCell loadedCell;
        private PacketReceiver packetReceiver;

        [TestInitialize]
        public void TestInitialize()
        {
            packetReceiver = new PacketReceiver();
            Map.Main = new SubnauticaMap();

            loadedCell = new AbsoluteEntityCell(loadedActionPosition, CELL_LEVEL);
            new AbsoluteEntityCell(unloadedActionPosition, CELL_LEVEL);

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
