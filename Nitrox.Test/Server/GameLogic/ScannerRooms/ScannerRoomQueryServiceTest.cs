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

[TestClass]
public sealed class ScannerRoomQueryServiceTest
{
    [DataTestMethod]
    [DataRow(float.NaN, 300f)]
    [DataRow(float.PositiveInfinity, 300f)]
    [DataRow(1f, 300f)]
    [DataRow(300f, 300f)]
    [DataRow(349.9f, 300f)]
    [DataRow(350f, 350f)]
    [DataRow(499.9f, 450f)]
    [DataRow(500f, 500f)]
    [DataRow(1000f, 500f)]
    public void RangeIsClampedAndQuantized(float input, float expected)
    {
        ScannerRoomQueryParameters.NormalizeRange(input).Should().Be(expected);
    }

    [TestMethod]
    public void NoSelectionIsCanonicalizedToNull()
    {
        NitroxTechType quartz = new("Quartz");

        ScannerRoomQueryParameters.NormalizeSelection(NitroxTechType.None).Should().BeNull();
        ScannerRoomQueryParameters.NormalizeSelection(null).Should().BeNull();
        ScannerRoomQueryParameters.NormalizeSelection(quartz).Should().BeSameAs(quartz);
    }

    [TestMethod]
    public void SnapshotRevisionIsDeterministicAndContentSensitive()
    {
        NitroxTechType quartz = new("Quartz");
        NitroxId entityId = new("1a0aef50-8de8-4c09-80af-45f434b0845f");
        List<ScannerResourceSummary> summaries = [new(quartz, 2)];
        List<ScannerResourceTarget> targets = [new(entityId, 0, quartz, new NitroxVector3(1, 2, 3))];

        ulong first = ScannerRoomSnapshotRevision.Compute(300, quartz, summaries, targets);
        ulong second = ScannerRoomSnapshotRevision.Compute(300, quartz, summaries, targets);
        ulong changed = ScannerRoomSnapshotRevision.Compute(350, quartz, summaries, targets);

        second.Should().Be(first);
        changed.Should().NotBe(first);
        first.Should().NotBe(0);
    }

    [TestMethod]
    public async Task QueryDoesNotDependOnPlayerVisibleCellsAndSupportsNotModified()
    {
        NitroxVector3 origin = new(100, -50, 200);
        NitroxTechType quartz = new("Quartz");
        TestCatalog catalog = new(quartz);
        ScannerResourceIndex index = new(catalog);
        WorldEntity resource = new(
            origin + new NitroxVector3(10, 0, 0),
            NitroxQuaternion.Identity,
            NitroxVector3.One,
            quartz,
            3,
            TestCatalog.ClassId,
            true,
            new NitroxId(),
            null);
        index.EntityTracked(resource);

        EntityRegistry registry = new(Substitute.For<ILogger<EntityRegistry>>());
        MapRoomEntity mapRoom = new(new NitroxId(), new NitroxId(), new NitroxInt3(1, 2, 3), origin);
        registry.AddEntity(mapRoom);
        RecordingBatchLoader batchLoader = new();
        SubnauticaServerOptions serverOptions = new() { EnableScannerRoomResourceSync = true };
        ScannerRoomQueryService service = new(
            registry,
            index,
            catalog,
            batchLoader,
            Options.Create(serverOptions),
            Substitute.For<ILogger<ScannerRoomQueryService>>());
        Player player = CreatePlayer(origin);

        ScannerRoomQueryResult first = await service.QueryAsync(player, mapRoom.Id, 300, quartz, 0, null);
        ScannerRoomQueryResult unchanged = await service.QueryAsync(player, mapRoom.Id, 300, quartz, first.Revision, null);

        first.Status.Should().Be(ScannerRoomQueryStatus.Complete);
        first.AvailableResources.Should().ContainSingle(summary => summary.TechType.Equals(quartz) && summary.Count == 1);
        first.Targets.Should().ContainSingle(target => target.EntityId == resource.Id);
        batchLoader.LastBatchCount.Should().BeGreaterThan(0);
        unchanged.Status.Should().Be(ScannerRoomQueryStatus.NotModified);
        unchanged.AvailableResources.Should().BeEmpty();
        unchanged.Targets.Should().BeEmpty();
    }

    private static Player CreatePlayer(NitroxVector3 position) => new(
        (PeerId)1,
        (SessionId)1,
        "Scanner tester",
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
        public int LastBatchCount { get; private set; }

        public Task LoadAsync(IReadOnlyList<NitroxInt3> batchIds, CancellationToken cancellationToken)
        {
            LastBatchCount = batchIds.Count;
            return Task.CompletedTask;
        }
    }

    private sealed class TestCatalog(NitroxTechType techType) : IScannerRoomResourceCatalog
    {
        public const string ClassId = "query-service-quartz";
        public float MaximumRelativeOffset => 0;

        public bool TryGetDescriptors(string classId, out IReadOnlyList<ScannerResourceDescriptor> descriptors)
        {
            if (classId == ClassId)
            {
                descriptors = [new ScannerResourceDescriptor(techType, 0, NitroxVector3.Zero)];
                return true;
            }

            descriptors = [];
            return false;
        }
    }
}
