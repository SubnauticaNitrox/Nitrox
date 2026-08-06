using Microsoft.Extensions.Logging;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.GameLogic.PlayerAnimation;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.MultiplayerSession;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors;
using NSubstitute;

namespace Nitrox.Test.Server.Packets.Processors;

// Disambiguates from Subnautica's global-namespace Player in Assembly-CSharp, which would otherwise win name resolution.
using Player = Nitrox.Server.Subnautica.Models.Player;

[TestClass]
public sealed class VehicleMovementsPacketProcessorTest
{
    [TestMethod]
    public async Task UnauthorizedEntryIsRemovedWhileAuthorizedSiblingIsMutatedAndRelayed()
    {
        EntityRegistry registry = new(Substitute.For<ILogger<EntityRegistry>>());
        SimulationOwnershipData ownership = new();
        RecordingPacketSender packetSender = new();
        Player sender = CreatePlayer(1, "Sender");
        Player otherOwner = CreatePlayer(2, "Other owner");
        VehicleEntity unauthorizedVehicle = AddSeamoth(registry);
        VehicleEntity authorizedVehicle = AddSeamoth(registry);
        ownership.TryToAcquire(unauthorizedVehicle.Id, otherOwner, SimulationLockType.EXCLUSIVE).Should().BeTrue();
        ownership.TryToAcquire(authorizedVehicle.Id, sender, SimulationLockType.EXCLUSIVE).Should().BeTrue();
        DrivenVehicleMovementData unauthorizedMovement = new(
            unauthorizedVehicle.Id,
            new NitroxVector3(20, 0, 0),
            NitroxQuaternion.Identity,
            0,
            0,
            true);
        NitroxVector3 authorizedPosition = new(10, 0, 0);
        DrivenVehicleMovementData authorizedMovement = new(
            authorizedVehicle.Id,
            authorizedPosition,
            NitroxQuaternion.Identity,
            0,
            0,
            true);
        VehicleMovements packet = new([unauthorizedMovement, authorizedMovement], 1);
        VehicleMovementsPacketProcessor processor = new(registry, ownership, EnabledNullLogger<VehicleMovementsPacketProcessor>.Instance);

        await processor.Process(new AuthProcessorContext(sender, packetSender), packet);

        packet.Data.Should().ContainSingle().Which.Should().BeSameAs(authorizedMovement);
        unauthorizedVehicle.Transform.Position.Should().Be(NitroxVector3.Zero);
        authorizedVehicle.Transform.Position.Should().Be(authorizedPosition);
        sender.Position.Should().Be(authorizedPosition);
        (Packet relayedPacket, SessionId excludedSessionId) = packetSender.Others.Should().ContainSingle().Which;
        relayedPacket.Should().BeSameAs(packet);
        excludedSessionId.Should().Be(sender.SessionId);
        ((VehicleMovements)relayedPacket).Data.Should().ContainSingle().Which.Should().BeSameAs(authorizedMovement);
    }

    private static VehicleEntity AddSeamoth(EntityRegistry registry)
    {
        VehicleEntity vehicle = new(
            new NitroxId(),
            0,
            new NitroxTransform(NitroxVector3.Zero, NitroxQuaternion.Identity, NitroxVector3.One),
            "seamoth-class-id",
            true,
            new NitroxId(),
            new NitroxTechType("Seamoth"),
            null);
        registry.AddEntity(vehicle);
        return vehicle;
    }

    private static Player CreatePlayer(ushort id, string name)
    {
        SessionId sessionId = id;
        PlayerContext context = new(
            name,
            sessionId,
            new NitroxId(),
            false,
            new PlayerSettings(new NitroxColor(1, 1, 1)),
            false,
            SubnauticaGameMode.SURVIVAL,
            null,
            IntroCinematicMode.COMPLETED,
            new PlayerAnimation(AnimChangeType.UNDERWATER, AnimChangeState.ON));
        return new Player(
            (PeerId)id,
            sessionId,
            name,
            false,
            context,
            NitroxVector3.Zero,
            NitroxQuaternion.Identity,
            context.PlayerNitroxId,
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

    private sealed class EnabledNullLogger<T> : ILogger<T>
    {
        public static EnabledNullLogger<T> Instance { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}
