using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nitrox.Model.Configuration;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors;
using Nitrox.Server.Subnautica.Models.Resources;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;
using NSubstitute;

namespace Nitrox.Test.Server.GameLogic.ScannerRooms;

// Disambiguates from Subnautica's global-namespace Player in Assembly-CSharp, which would otherwise win name resolution.
using Player = Nitrox.Server.Subnautica.Models.Player;

[TestClass]
public sealed class ScannerRoomScanStateChangeRequestProcessorTest
{
    private static readonly NitroxTechType quartz = new("Quartz");
    private static readonly NitroxTechType copper = new("Copper");

    [TestMethod]
    public async Task AcceptedChangeIsBroadcastIncludingSender()
    {
        Fixture fixture = new(quartz);
        MapRoomEntity room = fixture.AddRoom();

        await fixture.Process(new ScannerRoomScanStateChangeRequest(room.Id, quartz));

        ScannerRoomScanStateChanged packet = fixture.PacketSender.Broadcasts.Should().ContainSingle().Which.Should().BeOfType<ScannerRoomScanStateChanged>().Which;
        packet.MapRoomId.Should().Be(room.Id);
        packet.CanonicalState.SelectedTechType.Should().Be(quartz);
        packet.CanonicalState.Version.Should().Be(1);
        fixture.PacketSender.Direct.Should().BeEmpty();
    }

    [TestMethod]
    public async Task DuplicateChangeRepliesOnlyToSenderWithUnchangedCanonicalState()
    {
        Fixture fixture = new(quartz);
        ScannerRoomScanState persistedState = new(quartz, 7);
        MapRoomEntity room = fixture.AddRoom(persistedState);

        await fixture.Process(new ScannerRoomScanStateChangeRequest(room.Id, new NitroxTechType("Quartz")));

        fixture.PacketSender.Broadcasts.Should().BeEmpty();
        (Packet packet, SessionId sessionId) = fixture.PacketSender.Direct.Should().ContainSingle().Which;
        sessionId.Should().Be(fixture.Player.SessionId);
        ScannerRoomScanStateChanged changed = packet.Should().BeOfType<ScannerRoomScanStateChanged>().Which;
        changed.CanonicalState.Should().BeSameAs(persistedState);
    }

    [TestMethod]
    public async Task RejectedChangeRepliesOnlyToSenderWithUnchangedCanonicalState()
    {
        Fixture fixture = new(quartz);
        ScannerRoomScanState persistedState = new(quartz, 7);
        MapRoomEntity room = fixture.AddRoom(persistedState);

        await fixture.Process(new ScannerRoomScanStateChangeRequest(room.Id, copper));

        fixture.PacketSender.Broadcasts.Should().BeEmpty();
        (Packet packet, SessionId sessionId) = fixture.PacketSender.Direct.Should().ContainSingle().Which;
        sessionId.Should().Be(fixture.Player.SessionId);
        ScannerRoomScanStateChanged changed = packet.Should().BeOfType<ScannerRoomScanStateChanged>().Which;
        changed.CanonicalState.Should().BeSameAs(persistedState);
    }

    private sealed class Fixture
    {
        private readonly EntityRegistry registry = new(Substitute.For<ILogger<EntityRegistry>>());
        private readonly ScannerRoomScanStateChangeRequestProcessor processor;

        public RecordingPacketSender PacketSender { get; } = new();
        public Player Player { get; } = CreatePlayer();

        public Fixture(params NitroxTechType[] knownTechTypes)
        {
            ScannerRoomScanStateService service = new(
                registry,
                new TestCatalog(knownTechTypes),
                Options.Create(new SubnauticaServerOptions { EnableScannerRoomResourceSync = true }),
                Substitute.For<ILogger<ScannerRoomScanStateService>>());
            processor = new ScannerRoomScanStateChangeRequestProcessor(
                service,
                Substitute.For<ILogger<ScannerRoomScanStateChangeRequestProcessor>>());
        }

        public MapRoomEntity AddRoom(ScannerRoomScanState? state = null)
        {
            MapRoomEntity room = new(
                new NitroxId(),
                new NitroxId(),
                new NitroxInt3(0, 0, 0),
                NitroxVector3.Zero,
                state ?? ScannerRoomScanState.Empty);
            registry.AddEntity(room);
            return room;
        }

        public Task Process(ScannerRoomScanStateChangeRequest packet) =>
            processor.Process(new AuthProcessorContext(Player, PacketSender), packet);
    }

    private sealed class RecordingPacketSender : IPacketSender
    {
        public List<Packet> Broadcasts { get; } = [];
        public List<(Packet Packet, SessionId SessionId)> Direct { get; } = [];

        public ValueTask SendPacketAsync<T>(T packet, SessionId sessionId) where T : Packet
        {
            Direct.Add((packet, sessionId));
            return ValueTask.CompletedTask;
        }

        public ValueTask SendPacketToAllAsync<T>(T packet) where T : Packet
        {
            Broadcasts.Add(packet);
            return ValueTask.CompletedTask;
        }

        public ValueTask SendPacketToOthersAsync<T>(T packet, SessionId excludedSessionId) where T : Packet => ValueTask.CompletedTask;
    }

    private sealed class TestCatalog(IEnumerable<NitroxTechType> knownTechTypes) : IScannerRoomResourceCatalog
    {
        private readonly HashSet<NitroxTechType> knownTechTypes = [.. knownTechTypes];

        public float MaximumRelativeOffset => 0;
        public bool IsKnownTechType(NitroxTechType techType) => knownTechTypes.Contains(techType);

        public bool TryGetDescriptors(string classId, out IReadOnlyList<ScannerResourceDescriptor> descriptors)
        {
            descriptors = [];
            return false;
        }
    }

    private static Player CreatePlayer() => new(
        (PeerId)1,
        (SessionId)7,
        "Scanner tester",
        false,
        null,
        NitroxVector3.Zero,
        NitroxQuaternion.Identity,
        new NitroxId(),
        Optional.Empty,
        Perms.PLAYER,
        new PlayerStatsData(45, 45, 100, 100, 100, 0),
        SubnauticaGameMode.SURVIVAL,
        [],
        [],
        new Dictionary<string, NitroxId>(),
        new Dictionary<string, float>(),
        new Dictionary<string, PingInstancePreference>(),
        [],
        false,
        true);
}
