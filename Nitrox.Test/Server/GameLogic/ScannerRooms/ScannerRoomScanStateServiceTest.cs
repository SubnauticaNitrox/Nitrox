using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nitrox.Model.Configuration;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;
using Nitrox.Server.Subnautica.Models.Resources;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;
using NSubstitute;

namespace Nitrox.Test.Server.GameLogic.ScannerRooms;

[TestClass]
public sealed class ScannerRoomScanStateServiceTest
{
    private static readonly NitroxTechType quartz = new("Quartz");
    private static readonly NitroxTechType copper = new("Copper");

    [TestMethod]
    public void StartSwitchAndCancelAdvanceAndPersistCanonicalState()
    {
        Fixture fixture = new(true, quartz, copper);
        MapRoomEntity room = fixture.AddRoom();

        ScannerRoomScanStateChangeResult started = fixture.Service.Change(room.Id, quartz);
        ScannerRoomScanStateChangeResult switched = fixture.Service.Change(room.Id, copper);
        ScannerRoomScanStateChangeResult cancelled = fixture.Service.Change(room.Id, NitroxTechType.None);

        started.Status.Should().Be(ScannerRoomScanStateChangeStatus.Changed);
        started.State.SelectedTechType.Should().Be(quartz);
        started.State.Version.Should().Be(1);
        switched.Status.Should().Be(ScannerRoomScanStateChangeStatus.Changed);
        switched.State.SelectedTechType.Should().Be(copper);
        switched.State.Version.Should().Be(2);
        cancelled.Status.Should().Be(ScannerRoomScanStateChangeStatus.Changed);
        cancelled.State.SelectedTechType.Should().BeNull();
        cancelled.State.Version.Should().Be(3);
        room.ScanState.Should().BeSameAs(cancelled.State);
    }

    [TestMethod]
    public void DuplicateSelectionReturnsPersistedStateWithoutIncrementingVersion()
    {
        Fixture fixture = new(true, quartz);
        ScannerRoomScanState persistedState = new(quartz, 7);
        MapRoomEntity room = fixture.AddRoom(persistedState);

        ScannerRoomScanStateChangeResult result = fixture.Service.Change(room.Id, new NitroxTechType("Quartz"));

        result.Status.Should().Be(ScannerRoomScanStateChangeStatus.Unchanged);
        result.State.Should().BeSameAs(persistedState);
        result.State.Version.Should().Be(7);
        room.ScanState.Should().BeSameAs(persistedState);
    }

    [TestMethod]
    public void UnknownSelectionIsRejectedWithoutMutatingState()
    {
        Fixture fixture = new(true, quartz);
        ScannerRoomScanState persistedState = new(quartz, 5);
        MapRoomEntity room = fixture.AddRoom(persistedState);

        ScannerRoomScanStateChangeResult result = fixture.Service.Change(room.Id, copper);

        result.Status.Should().Be(ScannerRoomScanStateChangeStatus.Rejected);
        result.State.Should().BeSameAs(persistedState);
        room.ScanState.Should().BeSameAs(persistedState);
    }

    [TestMethod]
    public void DisabledSyncRejectsKnownSelectionWithoutMutatingState()
    {
        Fixture fixture = new(false, quartz, copper);
        ScannerRoomScanState persistedState = new(quartz, 5);
        MapRoomEntity room = fixture.AddRoom(persistedState);

        ScannerRoomScanStateChangeResult result = fixture.Service.Change(room.Id, copper);

        result.Status.Should().Be(ScannerRoomScanStateChangeStatus.Rejected);
        result.State.Should().BeSameAs(persistedState);
        room.ScanState.Should().BeSameAs(persistedState);
    }

    [TestMethod]
    public void InvalidRoomReturnsEmptyCanonicalState()
    {
        Fixture fixture = new(true, quartz);

        ScannerRoomScanStateChangeResult result = fixture.Service.Change(new NitroxId(), quartz);

        result.Status.Should().Be(ScannerRoomScanStateChangeStatus.InvalidRoom);
        result.State.Should().BeSameAs(ScannerRoomScanState.Empty);
    }

    [TestMethod]
    public void IndependentRoomsAdvanceFromTheirOwnVersions()
    {
        Fixture fixture = new(true, quartz, copper);
        MapRoomEntity firstRoom = fixture.AddRoom();
        MapRoomEntity secondRoom = fixture.AddRoom(new ScannerRoomScanState(quartz, 10));

        ScannerRoomScanStateChangeResult first = fixture.Service.Change(firstRoom.Id, quartz);
        ScannerRoomScanStateChangeResult second = fixture.Service.Change(secondRoom.Id, copper);

        first.State.Version.Should().Be(1);
        second.State.Version.Should().Be(11);
        firstRoom.ScanState.SelectedTechType.Should().Be(quartz);
        secondRoom.ScanState.SelectedTechType.Should().Be(copper);
    }

    [TestMethod]
    public async Task ConcurrentDistinctSelectionsReceiveMonotonicVersions()
    {
        NitroxTechType[] selections = Enumerable.Range(0, 32)
                                                .Select(index => new NitroxTechType($"Resource{index}"))
                                                .ToArray();
        Fixture fixture = new(true, selections);
        MapRoomEntity room = fixture.AddRoom();

        ScannerRoomScanStateChangeResult[] results = await Task.WhenAll(
            selections.Select(selection => Task.Run(() => fixture.Service.Change(room.Id, selection))));

        results.Should().OnlyContain(result => result.Status == ScannerRoomScanStateChangeStatus.Changed);
        results.Select(result => result.State.Version)
               .OrderBy(version => version)
               .Should()
               .Equal(Enumerable.Range(1, selections.Length).Select(version => (ulong)version));
        room.ScanState.Version.Should().Be((ulong)selections.Length);
        selections.Should().Contain(selection => selection.Equals(room.ScanState.SelectedTechType));
    }

    [TestMethod]
    public void ExhaustedVersionIsRejectedWithoutWrapping()
    {
        Fixture fixture = new(true, quartz, copper);
        ScannerRoomScanState exhaustedState = new(quartz, ulong.MaxValue);
        MapRoomEntity room = fixture.AddRoom(exhaustedState);

        ScannerRoomScanStateChangeResult result = fixture.Service.Change(room.Id, copper);

        result.Status.Should().Be(ScannerRoomScanStateChangeStatus.Rejected);
        result.State.Should().BeSameAs(exhaustedState);
        room.ScanState.Should().BeSameAs(exhaustedState);
    }

    private sealed class Fixture
    {
        private readonly EntityRegistry registry = new(Substitute.For<ILogger<EntityRegistry>>());

        public ScannerRoomScanStateService Service { get; }

        public Fixture(bool enabled, params NitroxTechType[] knownTechTypes)
        {
            TestCatalog catalog = new(knownTechTypes);
            Service = new ScannerRoomScanStateService(
                registry,
                catalog,
                Options.Create(new SubnauticaServerOptions { EnableScannerRoomResourceSync = enabled }),
                Substitute.For<ILogger<ScannerRoomScanStateService>>());
        }

        public MapRoomEntity AddRoom(ScannerRoomScanState? scanState = null)
        {
            MapRoomEntity room = new(
                new NitroxId(),
                new NitroxId(),
                new NitroxInt3(0, 0, 0),
                NitroxVector3.Zero,
                scanState ?? ScannerRoomScanState.Empty);
            registry.AddEntity(room);
            return room;
        }
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
}
