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
using Nitrox.Model.Subnautica.MultiplayerSession;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors;
using NSubstitute;

namespace Nitrox.Test.Server.Packets.Processors;

using Player = Nitrox.Server.Subnautica.Models.Player;

[TestClass]
public sealed class PlayerPingCreatedProcessorTest
{
    [TestMethod]
    public async Task ValidPingIsForwardedWithCanonicalSender()
    {
        Fixture fixture = new();
        Player sender = fixture.AddPlayer("Sender");
        Player spoofedPlayer = fixture.AddPlayer("Spoofed");
        NitroxId pingId = new();
        NitroxVector3 position = new(1, 2, 3);

        await fixture.Process(sender, new PlayerPingCreated(spoofedPlayer.SessionId, "Test ping", position, pingId, 1));

        (Packet packet, SessionId excludedSessionId) = fixture.PacketSender.Others.Should().ContainSingle().Which;
        PlayerPingCreated ping = packet.Should().BeOfType<PlayerPingCreated>().Which;
        ping.SessionId.Should().Be(sender.SessionId);
        ping.Text.Should().Be("Test ping");
        ping.Position.Should().Be(position);
        ping.PingId.Should().Be(pingId);
        ping.VoiceLineIndex.Should().Be(1);
        excludedSessionId.Should().Be(sender.SessionId);
    }

    [TestMethod]
    public async Task InvalidVoiceLineIndexIsRejected()
    {
        Fixture fixture = new();
        Player sender = fixture.AddPlayer("Sender");

        await fixture.Process(sender, new PlayerPingCreated(sender.SessionId, "Test ping", NitroxVector3.Zero, new NitroxId(), PlayerPingCreated.VOICE_LINE_COUNT));

        fixture.PacketSender.Others.Should().BeEmpty();
    }

    private sealed class Fixture
    {
        private readonly PlayerManager playerManager;
        private readonly SessionManager sessionManager = new(null!, Substitute.For<ILogger<SessionManager>>());
        private int nextPort = 14000;

        public RecordingPacketSender PacketSender { get; } = new();
        public PlayerPingCreatedProcessor Processor { get; } = new(Substitute.For<ILogger<PlayerPingCreatedProcessor>>());

        public Fixture()
        {
            playerManager = new PlayerManager(
                sessionManager,
                Options.Create(new SubnauticaServerOptions { MaxConnections = 20 }),
                Substitute.For<ILogger<PlayerManager>>());
        }

        public Player AddPlayer(string name)
        {
            IPEndPoint endpoint = new(IPAddress.Loopback, nextPort++);
            SessionManager.Session session = sessionManager.GetOrCreateSession(endpoint);
            playerManager.ReservePlayerContext(
                session.Id,
                endpoint,
                new PlayerSettings(new NitroxColor(0f, 1f, 1f)),
                new AuthenticationContext(name, Optional.Empty));
            return playerManager.CreatePlayerData(session.Id, out _);
        }

        public Task Process(Player sender, PlayerPingCreated packet)
        {
            return Processor.Process(new AuthProcessorContext(sender, PacketSender), packet);
        }
    }

    private sealed class RecordingPacketSender : IPacketSender
    {
        public List<(Packet Packet, SessionId ExcludedSessionId)> Others { get; } = [];

        public ValueTask SendPacketAsync<T>(T packet, SessionId sessionId) where T : Packet => ValueTask.CompletedTask;
        public ValueTask SendPacketToAllAsync<T>(T packet) where T : Packet => ValueTask.CompletedTask;

        public ValueTask SendPacketToOthersAsync<T>(T packet, SessionId excludedSessionId) where T : Packet
        {
            Others.Add((packet, excludedSessionId));
            return ValueTask.CompletedTask;
        }
    }
}
