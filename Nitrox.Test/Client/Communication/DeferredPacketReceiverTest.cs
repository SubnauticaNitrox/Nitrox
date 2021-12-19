using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient;
using NitroxClient.Communication;
using NitroxClient.Map;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Packets;
using NitroxTest.Model;

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
        private readonly NitroxVector3 loadedActionPosition = new NitroxVector3(50, 50, 50);
        private readonly NitroxVector3 unloadedActionPosition = new NitroxVector3(200, 200, 200);
        private AbsoluteEntityCell loadedCell;
        private AbsoluteEntityCell unloadedCell;
        private Int3 cellId = Int3.zero;

        [TestInitialize]
        public void TestInitialize()
        {
            NitroxServiceLocator.InitializeDependencyContainer(new ClientAutoFacRegistrar(), new TestAutoFacRegistrar());
            NitroxServiceLocator.BeginNewLifetimeScope();

            packetReceiver = NitroxServiceLocator.LocateService<PacketReceiver>();

            loadedCell = new AbsoluteEntityCell(loadedActionPosition, CELL_LEVEL);
            unloadedCell = new AbsoluteEntityCell(unloadedActionPosition, CELL_LEVEL);

            visibleCells.Add(loadedCell);
        }

        [TestMethod]
        public void NonActionPacket()
        {
            TestNonActionPacket packet = new TestNonActionPacket(PLAYER_ID);
            packetReceiver.PacketReceived(packet);

            Queue<Packet> packets = packetReceiver.GetReceivedPackets();

            Assert.AreEqual(1, packets.Count);
            Assert.AreEqual(packet, packets.Dequeue());
            Assert.AreEqual(packet.PlayerId, PLAYER_ID);
        }

        [TestCleanup]
        public void Cleanup()
        {
            NitroxServiceLocator.EndCurrentLifetimeScope();
        }
    }
}
