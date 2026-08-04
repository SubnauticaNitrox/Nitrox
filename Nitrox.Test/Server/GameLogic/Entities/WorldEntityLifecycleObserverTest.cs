using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nitrox.Model.Configuration;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using NSubstitute;

namespace Nitrox.Test.Server.GameLogic.Entities;

[TestClass]
public sealed class WorldEntityLifecycleObserverTest
{
    [TestMethod]
    public void TrackMoveWithinSameCellAndUntrackAreObserved()
    {
        RecordingObserver observer = new();
        EntityRegistry registry = new(Substitute.For<ILogger<EntityRegistry>>());
        WorldEntityManager manager = new(null!, registry, null!, null!, [observer], Substitute.For<ILogger<WorldEntityManager>>());
        WorldEntity entity = new(
            NitroxVector3.Zero,
            NitroxQuaternion.Identity,
            NitroxVector3.One,
            new NitroxTechType("Quartz"),
            3,
            "quartz-class-id",
            true,
            new NitroxId(),
            null);
        registry.AddEntity(entity);

        manager.RegisterWorldEntity(entity);
        manager.TryUpdateEntityPosition(entity.Id, new NitroxVector3(1, 0, 0), NitroxQuaternion.Identity, out _, out _).Should().BeTrue();
        manager.StopTrackingEntity(entity);

        observer.Events.Should().Equal("tracked", "moved", "untracked");
    }

    [TestMethod]
    public void DestroyingParentUntracksEveryWorldEntityInHierarchyOnce()
    {
        RecordingObserver observer = new();
        EntityRegistry registry = new(Substitute.For<ILogger<EntityRegistry>>());
        WorldEntityManager manager = new(null!, registry, null!, null!, [observer], Substitute.For<ILogger<WorldEntityManager>>());
        WorldEntity parent = CreateWorldEntity();
        WorldEntity child = CreateWorldEntity(parent);
        parent.ChildEntities.Add(child);
        registry.AddEntitiesIgnoringDuplicate([parent]);
        manager.RegisterWorldEntity(parent);
        manager.RegisterWorldEntity(child);

        manager.TryDestroyEntity(parent.Id, out Entity? removedEntity).Should().BeTrue();

        removedEntity.Should().BeSameAs(parent);
        registry.GetEntityById(parent.Id).HasValue.Should().BeFalse();
        registry.GetEntityById(child.Id).HasValue.Should().BeFalse();
        manager.GetEntities(parent.AbsoluteEntityCell).Should().NotContain(entity => entity.Id == parent.Id || entity.Id == child.Id);
        observer.UntrackedEntityIds.Should().BeEquivalentTo(parent.Id, child.Id);
        observer.UntrackedEntityIds.Should().OnlyHaveUniqueItems();
    }

    [TestMethod]
    public void RemovingGlobalRootUntracksNestedWorldEntitiesOnce()
    {
        RecordingObserver observer = new();
        EntityRegistry registry = new(Substitute.For<ILogger<EntityRegistry>>());
        WorldEntityManager manager = new(null!, registry, null!, null!, [observer], Substitute.For<ILogger<WorldEntityManager>>());
        GlobalRootEntity parent = new(
            new NitroxTransform(NitroxVector3.Zero, NitroxQuaternion.Identity, NitroxVector3.One),
            GlobalRootEntity.GLOBAL_ROOT_LEVEL,
            "global-root-class-id",
            true,
            new NitroxId(),
            new NitroxTechType("Base"),
            null,
            null,
            []);
        WorldEntity child = CreateWorldEntity(parent);
        parent.ChildEntities.Add(child);
        manager.AddOrUpdateGlobalRootEntity(parent);
        manager.RegisterWorldEntity(child);

        manager.RemoveGlobalRootEntity(parent.Id).HasValue.Should().BeTrue();

        registry.GetEntityById(parent.Id).HasValue.Should().BeFalse();
        registry.GetEntityById(child.Id).HasValue.Should().BeFalse();
        manager.globalRootEntitiesById.Should().NotContainKey(parent.Id);
        manager.GetEntities(child.AbsoluteEntityCell).Should().NotContain(entity => entity.Id == child.Id);
        observer.UntrackedEntityIds.Should().BeEquivalentTo(parent.Id, child.Id);
        observer.UntrackedEntityIds.Should().OnlyHaveUniqueItems();
    }

    [TestMethod]
    public void FullyDeconstructingRegularWorldEntityPublishesUntrackedEvent()
    {
        RecordingObserver observer = new();
        EntityRegistry registry = new(Substitute.For<ILogger<EntityRegistry>>());
        WorldEntityManager worldEntityManager = new(null!, registry, null!, null!, [observer], Substitute.For<ILogger<WorldEntityManager>>());
        BuildingManager buildingManager = new(
            null!,
            registry,
            worldEntityManager,
            Options.Create(new SubnauticaServerOptions()),
            Substitute.For<ILogger<BuildingManager>>());
        WorldEntity entity = CreateWorldEntity();
        registry.AddEntity(entity);
        worldEntityManager.RegisterWorldEntity(entity);

        buildingManager.ModifyConstructedAmount(new ModifyConstructedAmount(entity.Id, 0f)).Should().BeTrue();

        registry.GetEntityById(entity.Id).HasValue.Should().BeFalse();
        worldEntityManager.GetEntities(entity.AbsoluteEntityCell).Should().NotContain(worldEntity => worldEntity.Id == entity.Id);
        observer.UntrackedEntityIds.Should().Equal(entity.Id);
    }

    private static WorldEntity CreateWorldEntity(Entity? parent = null) => new(
        NitroxVector3.Zero,
        NitroxQuaternion.Identity,
        NitroxVector3.One,
        new NitroxTechType("Quartz"),
        3,
        "quartz-class-id",
        true,
        new NitroxId(),
        parent);

    private sealed class RecordingObserver : IWorldEntityLifecycleObserver
    {
        public List<string> Events { get; } = [];
        public List<NitroxId> UntrackedEntityIds { get; } = [];

        public void EntityTracked(WorldEntity entity) => Events.Add("tracked");

        public void EntityMoved(WorldEntity entity) => Events.Add("moved");

        public void EntityUntracked(WorldEntity entity)
        {
            Events.Add("untracked");
            UntrackedEntityIds.Add(entity.Id);
        }
    }
}
