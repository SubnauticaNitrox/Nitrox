using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nitrox.Model.Configuration;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;
using Nitrox.Server.Subnautica.Models;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;
using Nitrox.Server.Subnautica.Models.Resources;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;
using NSubstitute;

namespace Nitrox.Test.Server.GameLogic.ScannerRooms;

[TestClass]
public sealed class ScannerRoomLegacyOriginRepairTest
{
    [TestMethod]
    public async Task RepairsMissingOriginAtExactDistanceLimit()
    {
        NitroxVector3 playerPosition = NitroxVector3.Zero;
        NitroxVector3 observedOrigin = new(25, 0, 0);
        QueryFixture fixture = new(null, playerPosition);

        ScannerRoomQueryResult result = await fixture.Query(observedOrigin);

        result.Status.Should().Be(ScannerRoomQueryStatus.Complete);
        fixture.MapRoom.ScanOrigin.Should().Be(observedOrigin);
        fixture.BatchLoader.LoadCount.Should().Be(1);
    }

    [TestMethod]
    public async Task RejectsMissingOriginBeyondDistanceLimit()
    {
        NitroxVector3 observedOrigin = new(25.01f, 0, 0);
        QueryFixture fixture = new(null, NitroxVector3.Zero);

        ScannerRoomQueryResult result = await fixture.Query(observedOrigin);

        result.Status.Should().Be(ScannerRoomQueryStatus.OriginUnavailable);
        fixture.MapRoom.ScanOrigin.Should().BeNull();
        fixture.BatchLoader.LoadCount.Should().Be(0);
    }

    [DataTestMethod]
    [DataRow(float.NaN)]
    [DataRow(float.PositiveInfinity)]
    [DataRow(float.NegativeInfinity)]
    public async Task RejectsNonFiniteObservedOrigin(float invalidCoordinate)
    {
        NitroxVector3 observedOrigin = new(invalidCoordinate, 0, 0);
        QueryFixture fixture = new(null, NitroxVector3.Zero);

        ScannerRoomQueryResult result = await fixture.Query(observedOrigin);

        result.Status.Should().Be(ScannerRoomQueryStatus.OriginUnavailable);
        fixture.MapRoom.ScanOrigin.Should().BeNull();
        fixture.BatchLoader.LoadCount.Should().Be(0);
    }

    [TestMethod]
    public async Task NeverOverwritesPersistedOrigin()
    {
        NitroxVector3 persistedOrigin = new(10, 20, 30);
        NitroxVector3 observedOrigin = new(1, 2, 3);
        QueryFixture fixture = new(persistedOrigin, observedOrigin);

        ScannerRoomQueryResult result = await fixture.Query(observedOrigin);

        result.Status.Should().Be(ScannerRoomQueryStatus.Complete);
        fixture.MapRoom.ScanOrigin.Should().Be(persistedOrigin);
        fixture.BatchLoader.LoadCount.Should().Be(1);
    }

    private sealed class QueryFixture
    {
        private readonly ScannerRoomQueryService service;
        private readonly Player player;

        public MapRoomEntity MapRoom { get; }
        public RecordingBatchLoader BatchLoader { get; } = new();

        public QueryFixture(NitroxVector3? persistedOrigin, NitroxVector3 playerPosition)
        {
            EmptyCatalog catalog = new();
            EntityRegistry registry = new(Substitute.For<ILogger<EntityRegistry>>());
            MapRoom = new MapRoomEntity(new NitroxId(), new NitroxId(), new NitroxInt3(0, 0, 0), persistedOrigin);
            registry.AddEntity(MapRoom);

            SubnauticaServerOptions serverOptions = new() { EnableScannerRoomResourceSync = true };
            ScannerRoomDiagnostics diagnostics = new(
                Options.Create(serverOptions),
                Substitute.For<ILogger<ScannerRoomDiagnostics>>());
            service = new ScannerRoomQueryService(
                registry,
                new ScannerResourceIndex(catalog),
                catalog,
                BatchLoader,
                diagnostics,
                Options.Create(serverOptions),
                Substitute.For<ILogger<ScannerRoomQueryService>>());
            player = CreatePlayer(playerPosition);
        }

        public Task<ScannerRoomQueryResult> Query(NitroxVector3? observedOrigin) =>
            service.QueryAsync(player, MapRoom.Id, 300, null, 0, observedOrigin);
    }

    private static Player CreatePlayer(NitroxVector3 position) => new(
        (PeerId)1,
        (SessionId)1,
        "Scanner origin tester",
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

    private sealed class RecordingBatchLoader : IScannerRoomBatchLoader
    {
        public int LoadCount { get; private set; }

        public Task LoadAsync(IReadOnlyList<NitroxInt3> batchIds, CancellationToken cancellationToken)
        {
            LoadCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class EmptyCatalog : IScannerRoomResourceCatalog
    {
        public float MaximumRelativeOffset => 0;

        public bool TryGetDescriptors(string classId, out IReadOnlyList<ScannerResourceDescriptor> descriptors)
        {
            descriptors = [];
            return false;
        }
    }
}
