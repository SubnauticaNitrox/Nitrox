using Microsoft.Extensions.Logging;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
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

    private sealed class RecordingObserver : IWorldEntityLifecycleObserver
    {
        public List<string> Events { get; } = [];

        public void EntityTracked(WorldEntity entity) => Events.Add("tracked");

        public void EntityMoved(WorldEntity entity) => Events.Add("moved");

        public void EntityUntracked(WorldEntity entity) => Events.Add("untracked");
    }
}
