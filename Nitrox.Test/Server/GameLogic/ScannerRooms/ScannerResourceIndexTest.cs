using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;
using Nitrox.Server.Subnautica.Models.Resources;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;

namespace Nitrox.Test.Server.GameLogic.ScannerRooms;

[TestClass]
public sealed class ScannerResourceIndexTest
{
    private static readonly NitroxTechType quartz = new("Quartz");

    [TestMethod]
    public void TrackMoveAndUntrackUpdatesRoomQueries()
    {
        ScannerResourceIndex index = CreateIndex();
        WorldEntity entity = CreateEntity(new NitroxVector3(10, -20, 30));
        IReadOnlyList<NitroxInt3> initialBatches = BatchesFor(entity.Transform.Position);

        index.EntityTracked(entity);
        ScannerResourceNode initialNode = index.Query(initialBatches, entity.Transform.Position, 20).Should().ContainSingle().Which;
        initialNode.Position.Should().Be(new NitroxVector3(12, -20, 30));

        entity.Transform.Position = new NitroxVector3(50, -20, 30);
        index.EntityMoved(entity);

        index.Query(initialBatches, new NitroxVector3(10, -20, 30), 20).Should().BeEmpty();
        IReadOnlyList<NitroxInt3> movedBatches = BatchesFor(entity.Transform.Position);
        index.Query(movedBatches, entity.Transform.Position, 20).Should().ContainSingle();

        index.EntityUntracked(entity);
        index.Query(movedBatches, entity.Transform.Position, 20).Should().BeEmpty();
    }

    [TestMethod]
    public void RepeatedTrackReplacesInsteadOfDuplicating()
    {
        ScannerResourceIndex index = CreateIndex();
        WorldEntity entity = CreateEntity(NitroxVector3.Zero);
        IReadOnlyList<NitroxInt3> batches = BatchesFor(new NitroxVector3(2, 0, 0));

        index.EntityTracked(entity);
        index.EntityTracked(entity);

        index.Query(batches, NitroxVector3.Zero, 10).Should().ContainSingle();
    }

    [TestMethod]
    public void ExactRadiusBoundaryIsIncluded()
    {
        ScannerResourceIndex index = CreateIndex();
        WorldEntity entity = CreateEntity(new NitroxVector3(8, 0, 0));
        index.EntityTracked(entity);
        IReadOnlyList<NitroxInt3> batches = BatchesFor(new NitroxVector3(10, 0, 0));

        index.Query(batches, NitroxVector3.Zero, 10).Should().ContainSingle();
    }

    [TestMethod]
    public void HydratingRestoredSaveEntitiesIndexesScannablesWithoutTrackingEvents()
    {
        ScannerResourceIndex index = CreateIndex();
        WorldEntity restoredEntity = CreateEntity(new NitroxVector3(10, -20, 30));
        IReadOnlyList<NitroxInt3> batches = BatchesFor(new NitroxVector3(12, -20, 30));

        index.Hydrate([restoredEntity]);

        ScannerResourceNode node = index.Query(batches, restoredEntity.Transform.Position, 20).Should().ContainSingle().Which;
        node.Key.EntityId.Should().Be(restoredEntity.Id);
        node.Position.Should().Be(new NitroxVector3(12, -20, 30));
    }

    [TestMethod]
    public void RepeatedRestoredSaveHydrationIsIdempotent()
    {
        ScannerResourceIndex index = CreateIndex();
        WorldEntity restoredEntity = CreateEntity(NitroxVector3.Zero);
        IReadOnlyList<NitroxInt3> batches = BatchesFor(new NitroxVector3(2, 0, 0));

        index.Hydrate([restoredEntity]);
        long firstRevision = index.Revision;
        firstRevision.Should().Be(1);

        index.Hydrate([restoredEntity]);

        index.Revision.Should().Be(firstRevision);
        index.Query(batches, NitroxVector3.Zero, 10).Should().ContainSingle();
    }

    [TestMethod]
    public void RestoredSaveHydrationReplacesPreviouslyIndexedEntities()
    {
        ScannerResourceIndex index = CreateIndex();
        WorldEntity staleEntity = CreateEntity(NitroxVector3.Zero);
        IReadOnlyList<NitroxInt3> batches = BatchesFor(new NitroxVector3(2, 0, 0));
        index.EntityTracked(staleEntity);

        index.Hydrate([]);

        index.Query(batches, NitroxVector3.Zero, 10).Should().BeEmpty();
    }

    private static ScannerResourceIndex CreateIndex() => new(new TestCatalog());

    private static IReadOnlyList<NitroxInt3> BatchesFor(NitroxVector3 position) => [new AbsoluteEntityCell(position, 3).BatchId];

    private static WorldEntity CreateEntity(NitroxVector3 position) => new(
        position,
        NitroxQuaternion.Identity,
        NitroxVector3.One,
        quartz,
        3,
        TestCatalog.ClassId,
        true,
        new NitroxId(),
        null);

    private sealed class TestCatalog : IScannerRoomResourceCatalog
    {
        public const string ClassId = "scanner-test-quartz";
        public float MaximumRelativeOffset => 2f;

        public bool TryGetDescriptors(string classId, out IReadOnlyList<ScannerResourceDescriptor> descriptors)
        {
            if (classId == ClassId)
            {
                descriptors = [new ScannerResourceDescriptor(quartz, 0, new NitroxVector3(2, 0, 0))];
                return true;
            }

            descriptors = [];
            return false;
        }
    }
}
