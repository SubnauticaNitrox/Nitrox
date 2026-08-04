using Microsoft.Extensions.Logging;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Processors;
using NSubstitute;

namespace Nitrox.Test.Server.Packets.Processors;

[TestClass]
public sealed class ClearPlanterProcessorTest
{
    [TestMethod]
    public async Task ClearingPlanterUntracksWorldEntityChildren()
    {
        IWorldEntityLifecycleObserver observer = Substitute.For<IWorldEntityLifecycleObserver>();
        EntityRegistry registry = new(Substitute.For<ILogger<EntityRegistry>>());
        WorldEntityManager manager = new(null!, registry, null!, null!, [observer], Substitute.For<ILogger<WorldEntityManager>>());
        PlanterEntity planter = new(
            new NitroxTransform(NitroxVector3.Zero, NitroxQuaternion.Identity, NitroxVector3.One),
            GlobalRootEntity.GLOBAL_ROOT_LEVEL,
            "planter-class-id",
            true,
            new NitroxId(),
            new NitroxTechType("Planter"),
            null,
            null,
            []);
        WorldEntity plant = CreateWorldEntity(planter);
        planter.ChildEntities.Add(plant);
        registry.AddEntitiesIgnoringDuplicate([planter]);
        manager.RegisterWorldEntity(plant);
        ClearPlanterProcessor processor = new(registry, manager, Substitute.For<ILogger<ClearPlanterProcessor>>());

        await processor.Process(null!, new ClearPlanter(planter.Id));

        registry.GetEntityById(planter.Id).HasValue.Should().BeTrue();
        registry.GetEntityById(plant.Id).HasValue.Should().BeFalse();
        planter.ChildEntities.Should().BeEmpty();
        manager.GetEntities(plant.AbsoluteEntityCell).Should().NotContain(entity => entity.Id == plant.Id);
        observer.Received(1).EntityUntracked(plant);
    }

    private static WorldEntity CreateWorldEntity(Entity parent) => new(
        NitroxVector3.Zero,
        NitroxQuaternion.Identity,
        NitroxVector3.One,
        new NitroxTechType("CreepvineSeedCluster"),
        3,
        "creepvine-seed-cluster-class-id",
        true,
        new NitroxId(),
        parent);
}
