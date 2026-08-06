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

namespace Nitrox.Test.Server.GameLogic;

// Disambiguates from Subnautica's global-namespace Player in Assembly-CSharp, which would otherwise win name resolution.
using Player = Nitrox.Server.Subnautica.Models.Player;

[TestClass]
public sealed class SeamothPassengerServiceTest
{
    [TestMethod]
    public void EntryRequiresDifferentExclusiveDriverOfExactSeamoth()
    {
        Fixture fixture = new();
        Player driver = fixture.CreatePlayer("Driver");
        Player passenger = fixture.CreatePlayer("Passenger");
        VehicleEntity seamoth = fixture.AddVehicle("Seamoth");

        fixture.Service.Change(passenger, seamoth.Id).Status.Should().Be(SeamothPassengerChangeStatus.Rejected);

        fixture.Ownership.TryToAcquire(seamoth.Id, driver, SimulationLockType.TRANSIENT).Should().BeTrue();
        driver.PlayerContext!.DrivingVehicle = seamoth.Id;
        fixture.Service.Change(passenger, seamoth.Id).Status.Should().Be(SeamothPassengerChangeStatus.Rejected);

        fixture.Ownership.TryToAcquire(seamoth.Id, driver, SimulationLockType.EXCLUSIVE).Should().BeTrue();
        SeamothPassengerChangeResult accepted = fixture.Service.Change(passenger, seamoth.Id);

        accepted.Status.Should().Be(SeamothPassengerChangeStatus.Changed);
        accepted.State.Accepted.Should().BeTrue();
        accepted.State.SessionId.Should().Be(passenger.SessionId);
        accepted.State.SeamothId.Value.Should().Be(seamoth.Id);
        accepted.State.SeatIndex.Should().Be(0);
        passenger.PlayerContext.PassengerSeamoth.Should().Be(seamoth.Id);
        passenger.PlayerContext.DrivingVehicle.Should().BeNull();
    }

    [TestMethod]
    public void ExosuitIsRejectedEvenWithAValidExclusiveDriver()
    {
        Fixture fixture = new();
        Player driver = fixture.CreatePlayer("Driver");
        Player passenger = fixture.CreatePlayer("Passenger");
        VehicleEntity exosuit = fixture.AddVehicle("Exosuit");
        fixture.EnableDriver(driver, exosuit);

        SeamothPassengerChangeResult result = fixture.Service.Change(passenger, exosuit.Id);

        result.Status.Should().Be(SeamothPassengerChangeStatus.Rejected);
        result.State.Accepted.Should().BeFalse();
        passenger.PlayerContext!.PassengerSeamoth.Should().BeNull();
    }

    [TestMethod]
    public void AssignsThreeStableSeatsAndRejectsCapacityOverflow()
    {
        Fixture fixture = new();
        Player driver = fixture.CreatePlayer("Driver");
        VehicleEntity seamoth = fixture.AddVehicle("Seamoth");
        fixture.EnableDriver(driver, seamoth);
        Player[] passengers = Enumerable.Range(0, 4).Select(index => fixture.CreatePlayer($"Passenger {index}")).ToArray();

        SeamothPassengerChangeResult[] results = passengers.Select(player => fixture.Service.Change(player, seamoth.Id)).ToArray();

        results.Take(3).Should().OnlyContain(result => result.Status == SeamothPassengerChangeStatus.Changed && result.State.Accepted);
        results.Take(3).Select(result => result.State.SeatIndex).Should().Equal((byte)0, (byte)1, (byte)2);
        results[3].Status.Should().Be(SeamothPassengerChangeStatus.Rejected);
        results[3].State.Accepted.Should().BeFalse();
    }

    [TestMethod]
    public void DirectVehicleSwitchIsRejectedWithCurrentCanonicalState()
    {
        Fixture fixture = new();
        Player firstDriver = fixture.CreatePlayer("First driver");
        Player secondDriver = fixture.CreatePlayer("Second driver");
        Player passenger = fixture.CreatePlayer("Passenger");
        VehicleEntity firstSeamoth = fixture.AddVehicle("Seamoth");
        VehicleEntity secondSeamoth = fixture.AddVehicle("Seamoth");
        fixture.EnableDriver(firstDriver, firstSeamoth);
        fixture.EnableDriver(secondDriver, secondSeamoth);
        fixture.Service.Change(passenger, firstSeamoth.Id);

        SeamothPassengerChangeResult result = fixture.Service.Change(passenger, secondSeamoth.Id);

        result.Status.Should().Be(SeamothPassengerChangeStatus.Rejected);
        result.State.Accepted.Should().BeFalse();
        result.State.SeamothId.Value.Should().Be(firstSeamoth.Id);
        passenger.PlayerContext!.PassengerSeamoth.Should().Be(firstSeamoth.Id);
    }

    [TestMethod]
    public void DriverExitClearsEveryPassengerWithoutChangingVehicleOwnership()
    {
        Fixture fixture = new();
        Player driver = fixture.CreatePlayer("Driver");
        Player firstPassenger = fixture.CreatePlayer("First passenger");
        Player secondPassenger = fixture.CreatePlayer("Second passenger");
        VehicleEntity seamoth = fixture.AddVehicle("Seamoth");
        fixture.EnableDriver(driver, seamoth);
        fixture.Service.HandlePilotModeChanged(driver, seamoth.Id, true).Should().BeEmpty();
        fixture.Service.Change(firstPassenger, seamoth.Id);
        fixture.Service.Change(secondPassenger, seamoth.Id);

        IReadOnlyList<SeamothPassengerStateChanged> states = fixture.Service.HandlePilotModeChanged(driver, seamoth.Id, false);

        states.Should().HaveCount(2).And.OnlyContain(state => !state.SeamothId.HasValue && state.Accepted);
        firstPassenger.PlayerContext!.PassengerSeamoth.Should().BeNull();
        secondPassenger.PlayerContext!.PassengerSeamoth.Should().BeNull();
        fixture.Ownership.TryGetLock(seamoth.Id, out SimulationOwnershipData.PlayerLock playerLock).Should().BeTrue();
        playerLock.Player.Should().BeSameAs(driver);
    }

    [TestMethod]
    public async Task ProcessorBroadcastsChangesAndRepliesCanonicalRejectionsOnlyToSender()
    {
        Fixture fixture = new();
        Player driver = fixture.CreatePlayer("Driver");
        Player passenger = fixture.CreatePlayer("Passenger");
        VehicleEntity seamoth = fixture.AddVehicle("Seamoth");
        fixture.EnableDriver(driver, seamoth);
        SeamothPassengerStateChangeRequestProcessor processor = new(fixture.Service);

        await processor.Process(new AuthProcessorContext(passenger, fixture.PacketSender), new SeamothPassengerStateChangeRequest(seamoth.Id));
        await processor.Process(new AuthProcessorContext(passenger, fixture.PacketSender), new SeamothPassengerStateChangeRequest(new NitroxId()));

        SeamothPassengerStateChanged accepted = fixture.PacketSender.Broadcasts.Should().ContainSingle().Which.Should().BeOfType<SeamothPassengerStateChanged>().Which;
        accepted.Accepted.Should().BeTrue();
        (Packet packet, SessionId sessionId) = fixture.PacketSender.Direct.Should().ContainSingle().Which;
        sessionId.Should().Be(passenger.SessionId);
        SeamothPassengerStateChanged rejected = packet.Should().BeOfType<SeamothPassengerStateChanged>().Which;
        rejected.Accepted.Should().BeFalse();
        rejected.SeamothId.Value.Should().Be(seamoth.Id);
    }

    private sealed class Fixture
    {
        private int nextPlayerId;
        private readonly EntityRegistry registry = new(Substitute.For<ILogger<EntityRegistry>>());

        public SimulationOwnershipData Ownership { get; } = new();
        public RecordingPacketSender PacketSender { get; } = new();
        public SeamothPassengerService Service { get; }

        public Fixture()
        {
            Service = new SeamothPassengerService(registry, Ownership, PacketSender);
        }

        public VehicleEntity AddVehicle(string techType)
        {
            VehicleEntity vehicle = new(
                new NitroxId(),
                0,
                new NitroxTransform(NitroxVector3.Zero, NitroxQuaternion.Identity, NitroxVector3.One),
                $"{techType}-class-id",
                true,
                new NitroxId(),
                new NitroxTechType(techType),
                null);
            registry.AddEntity(vehicle);
            return vehicle;
        }

        public Player CreatePlayer(string name)
        {
            int playerNumber = ++nextPlayerId;
            SessionId sessionId = (ushort)playerNumber;
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
                (PeerId)(ushort)playerNumber,
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

        public void EnableDriver(Player player, VehicleEntity vehicle)
        {
            player.PlayerContext!.DrivingVehicle = vehicle.Id;
            Ownership.TryToAcquire(vehicle.Id, player, SimulationLockType.EXCLUSIVE).Should().BeTrue();
        }
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
}
