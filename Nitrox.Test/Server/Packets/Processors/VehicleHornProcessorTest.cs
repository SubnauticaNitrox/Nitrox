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
public sealed class VehicleHornProcessorTest
{
    [TestMethod]
    public async Task ValidHornIsSentOnlyToNearbyPlayers()
    {
        Fixture fixture = new();
        VehicleEntity seamoth = fixture.AddVehicle("Seamoth", NitroxVector3.Zero);
        Player pilot = fixture.AddPlayer("Pilot", NitroxVector3.Zero);
        Player nearby = fixture.AddPlayer("Nearby", new NitroxVector3(VehicleHorn.MAX_AUDIBLE_DISTANCE - 1, 0, 0));
        fixture.AddPlayer("FarAway", new NitroxVector3(VehicleHorn.MAX_AUDIBLE_DISTANCE + 1, 0, 0));
        pilot.PlayerContext!.DrivingVehicle = seamoth.Id;
        VehicleHorn.MAX_AUDIBLE_DISTANCE.Should().Be(200f);

        await fixture.Process(pilot, new VehicleHorn(seamoth.Id));

        (Packet packet, SessionId sessionId) = fixture.PacketSender.Direct.Should().ContainSingle().Which;
        packet.Should().BeOfType<VehicleHorn>().Which.VehicleId.Should().Be(seamoth.Id);
        sessionId.Should().Be(nearby.SessionId);
    }

    [TestMethod]
    public async Task HornIsRejectedWhenSenderIsNotThePilot()
    {
        Fixture fixture = new();
        VehicleEntity cyclops = fixture.AddVehicle("Cyclops", NitroxVector3.Zero);
        Player sender = fixture.AddPlayer("Passenger", NitroxVector3.Zero);
        fixture.AddPlayer("Nearby", new NitroxVector3(10, 0, 0));

        await fixture.Process(sender, new VehicleHorn(cyclops.Id));

        fixture.PacketSender.Direct.Should().BeEmpty();
    }

    [TestMethod]
    public async Task UnsupportedVehicleCannotHonk()
    {
        Fixture fixture = new();
        VehicleEntity exosuit = fixture.AddVehicle("Exosuit", NitroxVector3.Zero);
        Player sender = fixture.AddPlayer("Pilot", NitroxVector3.Zero);
        fixture.AddPlayer("Nearby", new NitroxVector3(10, 0, 0));
        sender.PlayerContext!.DrivingVehicle = exosuit.Id;

        await fixture.Process(sender, new VehicleHorn(exosuit.Id));

        fixture.PacketSender.Direct.Should().BeEmpty();
    }

    [TestMethod]
    public async Task RepeatedHornsAreAllForwarded()
    {
        Fixture fixture = new();
        VehicleEntity seamoth = fixture.AddVehicle("Seamoth", NitroxVector3.Zero);
        Player pilot = fixture.AddPlayer("Pilot", NitroxVector3.Zero);
        Player nearby = fixture.AddPlayer("Nearby", new NitroxVector3(10, 0, 0));
        pilot.PlayerContext!.DrivingVehicle = seamoth.Id;

        await fixture.Process(pilot, new VehicleHorn(seamoth.Id));
        await fixture.Process(pilot, new VehicleHorn(seamoth.Id));

        fixture.PacketSender.Direct.Should().HaveCount(2);
        fixture.PacketSender.Direct.Should().AllSatisfy(delivery =>
        {
            delivery.Packet.Should().BeOfType<VehicleHorn>().Which.VehicleId.Should().Be(seamoth.Id);
            delivery.SessionId.Should().Be(nearby.SessionId);
        });
    }

    private sealed class Fixture
    {
        private readonly EntityRegistry entityRegistry = new(Substitute.For<ILogger<EntityRegistry>>());
        private readonly PlayerManager playerManager;
        private readonly SessionManager sessionManager = new(null!, Substitute.For<ILogger<SessionManager>>());
        private int nextPort = 12000;

        public RecordingPacketSender PacketSender { get; } = new();
        public VehicleHornProcessor Processor { get; }

        public Fixture()
        {
            playerManager = new PlayerManager(
                sessionManager,
                Options.Create(new SubnauticaServerOptions { MaxConnections = 20 }),
                Substitute.For<ILogger<PlayerManager>>());
            Processor = new VehicleHornProcessor(entityRegistry, playerManager, Substitute.For<ILogger<VehicleHornProcessor>>());
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

        public Task Process(Player sender, VehicleHorn packet)
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
