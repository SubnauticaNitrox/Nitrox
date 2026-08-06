using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nitrox.Model.Configuration;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;
using Nitrox.Server.Subnautica.Models;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;
using Nitrox.Server.Subnautica.Models.Resources;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;
using NSubstitute;

namespace Nitrox.Test.Server.GameLogic.ScannerRooms;

// Disambiguates from Subnautica's global-namespace Player in Assembly-CSharp, which would otherwise win name resolution.
using Player = Nitrox.Server.Subnautica.Models.Player;

[TestClass]
public sealed class ScannerRoomAuthoritativeAcceptanceTest
{
    private static readonly NitroxTechType quartz = new("Quartz");
    private static readonly NitroxTechType copper = new("Copper");

    [TestMethod]
    public async Task PersistedRoomReturnsIdenticalResultsToPlayersWithDisjointVisibleCells()
    {
        TestFixture fixture = CreateFixture();
        Player firstPlayer = CreatePlayer((PeerId)1, (SessionId)1, "First scanner", fixture.Origin + new NitroxVector3(10_000, 0, 0));
        Player secondPlayer = CreatePlayer((PeerId)2, (SessionId)2, "Second scanner", fixture.Origin - new NitroxVector3(10_000, 0, 0));
        firstPlayer.AddCells([new AbsoluteEntityCell(firstPlayer.Position, 3)]);
        secondPlayer.AddCells([new AbsoluteEntityCell(secondPlayer.Position, 3)]);

        firstPlayer.GetVisibleCells().Should().NotIntersectWith(secondPlayer.GetVisibleCells());
        firstPlayer.CanSee(fixture.QuartzResource).Should().BeFalse();
        firstPlayer.CanSee(fixture.CopperResource).Should().BeFalse();
        secondPlayer.CanSee(fixture.QuartzResource).Should().BeFalse();
        secondPlayer.CanSee(fixture.CopperResource).Should().BeFalse();

        ScannerRoomQueryResult first = await fixture.Service.QueryAsync(firstPlayer, fixture.MapRoom.Id, 300, fixture.MapRoom.ScanState.Version, 0, null);
        ScannerRoomQueryResult second = await fixture.Service.QueryAsync(secondPlayer, fixture.MapRoom.Id, 300, fixture.MapRoom.ScanState.Version, 0, null);

        first.Status.Should().Be(ScannerRoomQueryStatus.Complete);
        first.AvailableResources.Should().HaveCount(2);
        first.AvailableResources.Should().ContainSingle(summary => summary.TechType.Equals(quartz) && summary.Count == 1);
        first.AvailableResources.Should().ContainSingle(summary => summary.TechType.Equals(copper) && summary.Count == 1);
        first.Targets.Should().ContainSingle(target => target.EntityId == fixture.QuartzResource.Id);
        AssertSameSnapshot(first, second);

        fixture.BatchLoader.Requests.Should().HaveCount(2);
        fixture.BatchLoader.Requests[1].Should().Equal(fixture.BatchLoader.Requests[0]);
    }

    [TestMethod]
    public async Task UntrackedResourceDisappearsOnNextAuthoritativeQuery()
    {
        TestFixture fixture = CreateFixture();
        Player player = CreatePlayer((PeerId)1, (SessionId)1, "Scanner tester", fixture.Origin + new NitroxVector3(10_000, 0, 0));

        ScannerRoomQueryResult initial = await fixture.Service.QueryAsync(player, fixture.MapRoom.Id, 300, fixture.MapRoom.ScanState.Version, 0, null);
        initial.Status.Should().Be(ScannerRoomQueryStatus.Complete);
        initial.Targets.Should().ContainSingle(target => target.EntityId == fixture.QuartzResource.Id);

        fixture.Index.EntityUntracked(fixture.QuartzResource);
        ScannerRoomQueryResult refreshed = await fixture.Service.QueryAsync(player, fixture.MapRoom.Id, 300, fixture.MapRoom.ScanState.Version, initial.Revision, null);

        refreshed.Status.Should().Be(ScannerRoomQueryStatus.Complete);
        refreshed.Revision.Should().NotBe(initial.Revision);
        refreshed.AvailableResources.Should().ContainSingle(summary => summary.TechType.Equals(copper) && summary.Count == 1);
        refreshed.AvailableResources.Should().NotContain(summary => summary.TechType.Equals(quartz));
        refreshed.Targets.Should().BeEmpty();
    }

    private static TestFixture CreateFixture()
    {
        NitroxVector3 origin = new(100, -50, 200);
        TestCatalog catalog = new();
        ScannerResourceIndex index = new(catalog);
        WorldEntity quartzResource = CreateResource(origin + new NitroxVector3(10, 0, 0), quartz, TestCatalog.QuartzClassId);
        WorldEntity copperResource = CreateResource(origin + new NitroxVector3(20, 0, 0), copper, TestCatalog.CopperClassId);
        index.Hydrate([quartzResource, copperResource]);

        EntityRegistry registry = new(Substitute.For<ILogger<EntityRegistry>>());
        MapRoomEntity mapRoom = new(
            new NitroxId(),
            new NitroxId(),
            new NitroxInt3(1, 2, 3),
            origin,
            new ScannerRoomScanState(quartz, 3));
        registry.AddEntity(mapRoom);

        RecordingBatchLoader batchLoader = new();
        SubnauticaServerOptions serverOptions = new() { EnableScannerRoomResourceSync = true };
        IOptions<SubnauticaServerOptions> options = Options.Create(serverOptions);
        ScannerRoomDiagnostics diagnostics = new(options, Substitute.For<ILogger<ScannerRoomDiagnostics>>());
        ScannerRoomScanStateService scanStateService = new(
            registry,
            catalog,
            options,
            Substitute.For<ILogger<ScannerRoomScanStateService>>());
        ScannerRoomQueryService service = new(
            registry,
            index,
            catalog,
            batchLoader,
            scanStateService,
            diagnostics,
            options,
            Substitute.For<ILogger<ScannerRoomQueryService>>());
        return new TestFixture(origin, mapRoom, quartzResource, copperResource, index, batchLoader, service);
    }

    private static WorldEntity CreateResource(NitroxVector3 position, NitroxTechType techType, string classId) => new(
        position,
        NitroxQuaternion.Identity,
        NitroxVector3.One,
        techType,
        3,
        classId,
        true,
        new NitroxId(),
        null);

    private static Player CreatePlayer(PeerId peerId, SessionId sessionId, string name, NitroxVector3 position) => new(
        peerId,
        sessionId,
        name,
        false,
        null,
        position,
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

    private static void AssertSameSnapshot(ScannerRoomQueryResult expected, ScannerRoomQueryResult actual)
    {
        actual.Status.Should().Be(expected.Status);
        actual.EffectiveRange.Should().Be(expected.EffectiveRange);
        actual.ScanState.SelectedTechType.Should().Be(expected.ScanState.SelectedTechType);
        actual.ScanState.Version.Should().Be(expected.ScanState.Version);
        actual.Revision.Should().Be(expected.Revision);
        actual.AvailableResources.Should().HaveCount(expected.AvailableResources.Count);
        for (int i = 0; i < expected.AvailableResources.Count; i++)
        {
            actual.AvailableResources[i].TechType.Should().Be(expected.AvailableResources[i].TechType);
            actual.AvailableResources[i].Count.Should().Be(expected.AvailableResources[i].Count);
        }

        actual.Targets.Should().HaveCount(expected.Targets.Count);
        for (int i = 0; i < expected.Targets.Count; i++)
        {
            actual.Targets[i].EntityId.Should().Be(expected.Targets[i].EntityId);
            actual.Targets[i].TrackerIndex.Should().Be(expected.Targets[i].TrackerIndex);
            actual.Targets[i].TechType.Should().Be(expected.Targets[i].TechType);
            actual.Targets[i].Position.Should().Be(expected.Targets[i].Position);
        }
    }

    private sealed record TestFixture(
        NitroxVector3 Origin,
        MapRoomEntity MapRoom,
        WorldEntity QuartzResource,
        WorldEntity CopperResource,
        ScannerResourceIndex Index,
        RecordingBatchLoader BatchLoader,
        ScannerRoomQueryService Service);

    private sealed class RecordingBatchLoader : IScannerRoomBatchLoader
    {
        public List<IReadOnlyList<NitroxInt3>> Requests { get; } = [];

        public Task LoadAsync(IReadOnlyList<NitroxInt3> batchIds, CancellationToken cancellationToken)
        {
            Requests.Add([.. batchIds]);
            return Task.CompletedTask;
        }
    }

    private sealed class TestCatalog : IScannerRoomResourceCatalog
    {
        public const string QuartzClassId = "authoritative-query-quartz";
        public const string CopperClassId = "authoritative-query-copper";

        public float MaximumRelativeOffset => 0;

        public bool IsKnownTechType(NitroxTechType techType) => techType.Equals(quartz) || techType.Equals(copper);

        public bool TryGetDescriptors(string classId, out IReadOnlyList<ScannerResourceDescriptor> descriptors)
        {
            if (classId == QuartzClassId)
            {
                descriptors = [new ScannerResourceDescriptor(quartz, 0, NitroxVector3.Zero)];
                return true;
            }
            if (classId == CopperClassId)
            {
                descriptors = [new ScannerResourceDescriptor(copper, 0, NitroxVector3.Zero)];
                return true;
            }

            descriptors = [];
            return false;
        }
    }
}
