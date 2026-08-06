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
        ScannerRoomScanState scanState = new(quartz, 7);
        manager.SeedScanState(roomId, scanState);
        manager.RequestSnapshot(roomId, 300, scanState, null);
        ScannerRoomQuery query = packetSender.LastPacket.Should().BeOfType<ScannerRoomQuery>().Which;
        query.ExpectedScanStateVersion.Should().Be(7);

        manager.EnqueuePage(StatusPage(query, query.RequestId + 1, ScannerRoomQueryStatus.Rejected, scanState));
        manager.ProcessQueuedPages(1).Should().Be(1);
        updates.Should().BeEmpty();

        manager.EnqueuePage(StatusPage(query, query.RequestId, ScannerRoomQueryStatus.Rejected, scanState));
        manager.ProcessQueuedPages(1).Should().Be(1);

        updates.Should().ContainSingle().Which.Should().Be(
            new ScannerRoomSnapshotUpdate(roomId, ScannerRoomSnapshotApplyResult.Failed, ScannerRoomQueryStatus.Rejected));
    }

    [TestMethod]
    public void SendsExplicitNormalizedScanStateChangeRequest()
    {
        RecordingPacketSender packetSender = new();
        using ScannerRoomManager manager = new(
            packetSender,
            new ScannerRoomSnapshotStore(),
            Substitute.For<IMultiplayerSession>());

        manager.RequestScanStateChange(roomId, NitroxTechType.None).Should().BeTrue();

        ScannerRoomScanStateChangeRequest packet = packetSender.LastPacket.Should().BeOfType<ScannerRoomScanStateChangeRequest>().Which;
        packet.MapRoomId.Should().Be(roomId);
        packet.DesiredTechType.Should().BeNull();
    }

    [TestMethod]
    public void LivePacketBeforeEntitySpawnWinsOverStaleAndEqualEntityState()
    {
        using ScannerRoomManager manager = CreateManager();
        List<ScannerRoomScanStateUpdate> updates = [];
        manager.ScanStateChanged += updates.Add;
        ScannerRoomScanState liveState = new(new NitroxTechType("Copper"), 6);

        manager.ApplyScanState(roomId, liveState);
        manager.SeedScanState(roomId, new ScannerRoomScanState(quartz, 5));
        manager.SeedScanState(roomId, new ScannerRoomScanState(quartz, 6));

        manager.TryGetScanState(roomId, out ScannerRoomScanState? cached).Should().BeTrue();
        cached.Should().BeSameAs(liveState);
        updates.Should().ContainSingle().Which.ScanState.Should().BeSameAs(liveState);
    }

    [TestMethod]
    public void EqualVersionCanonicalReplyCanCorrectCachedSelection()
    {
        using ScannerRoomManager manager = CreateManager();
        List<ScannerRoomScanStateUpdate> updates = [];
        manager.ScanStateChanged += updates.Add;
        manager.SeedScanState(roomId, new ScannerRoomScanState(quartz, 5));
        ScannerRoomScanState correction = new(new NitroxTechType("Copper"), 5);

        manager.ApplyScanState(roomId, correction);

        manager.TryGetScanState(roomId, out ScannerRoomScanState? cached).Should().BeTrue();
        cached.Should().BeSameAs(correction);
        updates.Should().HaveCount(2);
        updates[^1].ScanState.Should().BeSameAs(correction);
    }

    [TestMethod]
    public void LowerVersionLiveUpdateIsIgnored()
    {
        using ScannerRoomManager manager = CreateManager();
        List<ScannerRoomScanStateUpdate> updates = [];
        manager.ScanStateChanged += updates.Add;
        ScannerRoomScanState current = new(quartz, 6);
        manager.ApplyScanState(roomId, current);

        manager.ApplyScanState(roomId, new ScannerRoomScanState(new NitroxTechType("Copper"), 5));

        manager.TryGetScanState(roomId, out ScannerRoomScanState? cached).Should().BeTrue();
        cached.Should().BeSameAs(current);
        updates.Should().ContainSingle();
    }

    [TestMethod]
    public void AdvancingStateSupersedesQueuedSnapshotPages()
    {
        RecordingPacketSender packetSender = new();
        using ScannerRoomManager manager = CreateManager(packetSender);
        ScannerRoomScanState initial = new(quartz, 1);
        manager.SeedScanState(roomId, initial);
        manager.RequestSnapshot(roomId, 300, initial, null);
        ScannerRoomQuery query = packetSender.LastPacket.Should().BeOfType<ScannerRoomQuery>().Which;
        manager.EnqueuePage(new ScannerRoomSnapshotPage(
            roomId,
            query.RequestId,
            ScannerRoomQueryStatus.Complete,
            300,
            initial,
            42,
            0,
            1,
            [],
            []));

        manager.ApplyScanState(roomId, new ScannerRoomScanState(quartz, 2));

        manager.ProcessQueuedPages(1).Should().Be(0);
        manager.TryGetSnapshot(roomId, out _).Should().BeFalse();
    }

    [TestMethod]
    public void FirstNonEmptyStateAfterCacheClearSupersedesImplicitEmptyQuery()
    {
        RecordingPacketSender packetSender = new();
        using ScannerRoomManager manager = CreateManager(packetSender);
        manager.RequestSnapshot(roomId, 300, ScannerRoomScanState.Empty, null);
        ScannerRoomQuery query = packetSender.LastPacket.Should().BeOfType<ScannerRoomQuery>().Which;
        manager.EnqueuePage(new ScannerRoomSnapshotPage(
            roomId,
            query.RequestId,
            ScannerRoomQueryStatus.Complete,
            300,
            ScannerRoomScanState.Empty,
            42,
            0,
            1,
            [],
            []));

        ScannerRoomScanState reconnectedState = new(quartz, 3);
        manager.ApplyScanState(roomId, reconnectedState);

        manager.ProcessQueuedPages(1).Should().Be(0);
        manager.TryGetScanState(roomId, out ScannerRoomScanState? cached).Should().BeTrue();
        cached.Should().BeSameAs(reconnectedState);
        manager.TryGetSnapshot(roomId, out _).Should().BeFalse();
    }

    [TestMethod]
    public void StateOutdatedPageAdvancesCacheAndPublishesCanonicalStatus()
    {
        RecordingPacketSender packetSender = new();
        using ScannerRoomManager manager = CreateManager(packetSender);
        ScannerRoomScanState initial = new(quartz, 1);
        ScannerRoomScanState canonical = new(new NitroxTechType("Copper"), 2);
        manager.SeedScanState(roomId, initial);
        manager.RequestSnapshot(roomId, 300, initial, null);
        ScannerRoomQuery query = packetSender.LastPacket.Should().BeOfType<ScannerRoomQuery>().Which;
        List<ScannerRoomSnapshotUpdate> snapshotUpdates = [];
        manager.SnapshotChanged += snapshotUpdates.Add;

        manager.EnqueuePage(StatusPage(query, query.RequestId, ScannerRoomQueryStatus.StateOutdated, canonical));
        manager.ProcessQueuedPages(1).Should().Be(1);

        snapshotUpdates.Should().ContainSingle().Which.Should().Be(
            new ScannerRoomSnapshotUpdate(roomId, ScannerRoomSnapshotApplyResult.Failed, ScannerRoomQueryStatus.StateOutdated));
        manager.TryGetScanState(roomId, out ScannerRoomScanState? cached).Should().BeTrue();
        cached.Should().BeSameAs(canonical);
    }

    [TestMethod]
    public void ClearDropsCachedScanState()
    {
        using ScannerRoomManager manager = CreateManager();
        manager.SeedScanState(roomId, new ScannerRoomScanState(quartz, 1));

        manager.Clear();

        manager.TryGetScanState(roomId, out _).Should().BeFalse();
    }

    private static ScannerRoomSnapshotPage StatusPage(
        ScannerRoomQuery query,
        uint requestId,
        ScannerRoomQueryStatus status,
        ScannerRoomScanState scanState) =>
        new(
            query.MapRoomId,
            requestId,
            status,
            query.ReportedRange,
            scanState,
            0,
            0,
            1,
            [],
            []);

    private static ScannerRoomManager CreateManager(RecordingPacketSender? packetSender = null) =>
        new(packetSender ?? new RecordingPacketSender(), new ScannerRoomSnapshotStore(), Substitute.For<IMultiplayerSession>());

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
