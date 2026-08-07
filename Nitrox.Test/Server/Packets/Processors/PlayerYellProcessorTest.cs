using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nitrox.Model.Configuration;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.MultiplayerSession;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors;
using NSubstitute;

namespace Nitrox.Test.Server.Packets.Processors;

using Player = Nitrox.Server.Subnautica.Models.Player;

[TestClass]
public sealed class PlayerYellProcessorTest
{
    [TestMethod]
    public async Task ValidYellIsSentOnlyToNearbyPlayersWithCanonicalSender()
    {
        Fixture fixture = new();
        Player sender = fixture.AddPlayer("Sender", NitroxVector3.Zero);
        Player nearby = fixture.AddPlayer("Nearby", new NitroxVector3(PlayerYell.MAX_AUDIBLE_DISTANCE - 1, 0, 0));
        fixture.AddPlayer("FarAway", new NitroxVector3(PlayerYell.MAX_AUDIBLE_DISTANCE + 1, 0, 0));
        PlayerYell.MAX_AUDIBLE_DISTANCE.Should().Be(50f);

        await fixture.Process(sender, new PlayerYell(nearby.SessionId, 7, true));

        (Packet packet, SessionId sessionId) = fixture.PacketSender.Direct.Should().ContainSingle().Which;
        PlayerYell yell = packet.Should().BeOfType<PlayerYell>().Which;
        yell.SessionId.Should().Be(sender.SessionId);
        yell.SoundIndex.Should().Be(7);
        yell.IsInsideVehicle.Should().BeFalse();
        sessionId.Should().Be(nearby.SessionId);
    }

    [TestMethod]
    public async Task InvalidSoundIndexIsRejected()
    {
        Fixture fixture = new();
        Player sender = fixture.AddPlayer("Sender", NitroxVector3.Zero);
        fixture.AddPlayer("Nearby", new NitroxVector3(10, 0, 0));

        await fixture.Process(sender, new PlayerYell(sender.SessionId, PlayerYell.SOUND_COUNT));

        fixture.PacketSender.Direct.Should().BeEmpty();
    }

    [TestMethod]
    public async Task VehiclePassengersCanYellWithCanonicalDryAudio()
    {
        Fixture fixture = new();
        VehicleEntity cyclops = fixture.AddVehicle("Cyclops", NitroxVector3.Zero);
        Player nearby = fixture.AddPlayer("Nearby", new NitroxVector3(10, 0, 0));

        Player passenger = fixture.AddPlayer("Passenger", NitroxVector3.Zero);
        passenger.PlayerContext!.PassengerSeamoth = new NitroxId();
        await fixture.Process(passenger, new PlayerYell(passenger.SessionId, 1));

        passenger.Position = new NitroxVector3(PlayerYell.MAX_AUDIBLE_DISTANCE + 1, 0, 0);
        Player cyclopsOccupant = fixture.AddPlayer("CyclopsOccupant", NitroxVector3.Zero);
        cyclopsOccupant.SubRootId = Optional.Of(cyclops.Id);
        await fixture.Process(cyclopsOccupant, new PlayerYell(cyclopsOccupant.SessionId, 2));

        fixture.PacketSender.Direct.Should().HaveCount(2);
        fixture.PacketSender.Direct.Should().AllSatisfy(delivery =>
        {
            delivery.Packet.Should().BeOfType<PlayerYell>().Which.IsInsideVehicle.Should().BeTrue();
            delivery.SessionId.Should().Be(nearby.SessionId);
        });
        fixture.PacketSender.Direct.Select(delivery => delivery.Packet.As<PlayerYell>().SoundIndex)
               .Should().Equal(1, 2);
    }

    [TestMethod]
    public async Task DriversAndMutedPlayersCannotYell()
    {
        Fixture fixture = new();
        fixture.AddPlayer("Nearby", new NitroxVector3(10, 0, 0));

        Player driver = fixture.AddPlayer("Driver", NitroxVector3.Zero);
        driver.PlayerContext!.DrivingVehicle = new NitroxId();
        await fixture.Process(driver, new PlayerYell(driver.SessionId, 0));

        Player muted = fixture.AddPlayer("Muted", NitroxVector3.Zero);
        muted.PlayerContext!.IsMuted = true;
        await fixture.Process(muted, new PlayerYell(muted.SessionId, 3));

        fixture.PacketSender.Direct.Should().BeEmpty();
    }

    [TestMethod]
    public async Task RepeatedYellsAreAllForwarded()
    {
        Fixture fixture = new();
        Player sender = fixture.AddPlayer("Sender", NitroxVector3.Zero);
        Player nearby = fixture.AddPlayer("Nearby", new NitroxVector3(10, 0, 0));

        await fixture.Process(sender, new PlayerYell(sender.SessionId, 4));
        await fixture.Process(sender, new PlayerYell(sender.SessionId, 9));

        fixture.PacketSender.Direct.Should().HaveCount(2);
        fixture.PacketSender.Direct.Select(delivery => delivery.Packet)
               .Should().AllBeOfType<PlayerYell>()
               .And.SatisfyRespectively(
                   packet => packet.As<PlayerYell>().SoundIndex.Should().Be(4),
                   packet => packet.As<PlayerYell>().SoundIndex.Should().Be(9));
        fixture.PacketSender.Direct.Should().AllSatisfy(delivery => delivery.SessionId.Should().Be(nearby.SessionId));
    }

    private sealed class Fixture
    {
        private readonly EntityRegistry entityRegistry = new(Substitute.For<ILogger<EntityRegistry>>());
        private readonly PlayerManager playerManager;
        private readonly SessionManager sessionManager = new(null!, Substitute.For<ILogger<SessionManager>>());
        private int nextPort = 13000;

        public RecordingPacketSender PacketSender { get; } = new();
        public PlayerYellProcessor Processor { get; }

        public Fixture()
        {
            playerManager = new PlayerManager(
                sessionManager,
                Options.Create(new SubnauticaServerOptions { MaxConnections = 20 }),
                Substitute.For<ILogger<PlayerManager>>());
            Processor = new PlayerYellProcessor(entityRegistry, playerManager, Substitute.For<ILogger<PlayerYellProcessor>>());
        }

        public VehicleEntity AddVehicle(string techType, NitroxVector3 position)
        {
            VehicleEntity vehicle = new(
                null,
                0f,
                new NitroxTransform(position, NitroxQuaternion.Identity, NitroxVector3.One),
                $"{techType.ToLowerInvariant()}-class-id",
                true,
                new NitroxId(),
                new NitroxTechType(techType),
                null);
            entityRegistry.AddEntity(vehicle);
            return vehicle;
        }

        public Player AddPlayer(string name, NitroxVector3 position)
        {
            IPEndPoint endpoint = new(IPAddress.Loopback, nextPort++);
            SessionManager.Session session = sessionManager.GetOrCreateSession(endpoint);
            playerManager.ReservePlayerContext(
                session.Id,
                endpoint,
                new PlayerSettings(new NitroxColor(0f, 1f, 1f)),
                new AuthenticationContext(name, Optional.Empty));
            Player player = playerManager.CreatePlayerData(session.Id, out _);
            player.Position = position;
            return player;
        }

        public Task Process(Player sender, PlayerYell packet)
        {
            return Processor.Process(new AuthProcessorContext(sender, PacketSender), packet);
        }
    }

    private sealed class RecordingPacketSender : IPacketSender
    {
        public List<(Packet Packet, SessionId SessionId)> Direct { get; } = [];

        public ValueTask SendPacketAsync<T>(T packet, SessionId sessionId) where T : Packet
        {
            Direct.Add((packet, sessionId));
            return ValueTask.CompletedTask;
        }

        public ValueTask SendPacketToAllAsync<T>(T packet) where T : Packet => ValueTask.CompletedTask;
        public ValueTask SendPacketToOthersAsync<T>(T packet, SessionId excludedSessionId) where T : Packet => ValueTask.CompletedTask;
    }
}
