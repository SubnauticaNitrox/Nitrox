using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.ScannerRooms;
using NSubstitute;

namespace Nitrox.Test.Client.GameLogic.ScannerRooms;

[TestClass]
public sealed class ScannerRoomManagerTest
{
    private readonly NitroxId roomId = new("74f33d57-5de8-4079-b761-bcd98bb9dd85");
    private readonly NitroxTechType quartz = new("Quartz");

    [TestMethod]
    public void PublishesRejectedStatusOnlyAfterStoreAcceptsCurrentRequest()
    {
        RecordingPacketSender packetSender = new();
        IMultiplayerSession multiplayerSession = Substitute.For<IMultiplayerSession>();
        using ScannerRoomManager manager = new(packetSender, new ScannerRoomSnapshotStore(), multiplayerSession);
        List<ScannerRoomSnapshotUpdate> updates = [];
        manager.SnapshotChanged += updates.Add;
        manager.RequestSnapshot(roomId, 300, quartz, null);
        ScannerRoomQuery query = packetSender.LastPacket.Should().BeOfType<ScannerRoomQuery>().Which;

        manager.EnqueuePage(StatusPage(query, query.RequestId + 1, ScannerRoomQueryStatus.Rejected));
        manager.ProcessQueuedPages(1).Should().Be(1);
        updates.Should().BeEmpty();

        manager.EnqueuePage(StatusPage(query, query.RequestId, ScannerRoomQueryStatus.Rejected));
        manager.ProcessQueuedPages(1).Should().Be(1);

        updates.Should().ContainSingle().Which.Should().Be(
            new ScannerRoomSnapshotUpdate(roomId, ScannerRoomSnapshotApplyResult.Failed, ScannerRoomQueryStatus.Rejected));
    }

    private static ScannerRoomSnapshotPage StatusPage(
        ScannerRoomQuery query,
        uint requestId,
        ScannerRoomQueryStatus status) =>
        new(
            query.MapRoomId,
            requestId,
            status,
            query.ReportedRange,
            query.SelectedTechType,
            0,
            0,
            1,
            [],
            []);

    private sealed class RecordingPacketSender : IPacketSender
    {
        public Packet? LastPacket { get; private set; }

        public bool Send<T>(T packet) where T : Packet
        {
            LastPacket = packet;
            return true;
        }
    }
}
