using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Entities.Spawning.EntityBootstrappers
{
    class CrashFishBootstrapper : IEntityBootstrapper
    {
        public void Prepare(Entity parentEntity)
        {
            Entity crashFish = new Entity(parentEntity.Position, parentEntity.Rotation, TechType.Crash, parentEntity.Level, parentEntity.ClassId);
            Entity sulfur = new Entity(parentEntity.Position, parentEntity.Rotation, TechType.CrashPowder, parentEntity.Level, parentEntity.ClassId);

            parentEntity.ChildEntities.Add(crashFish);
            parentEntity.ChildEntities.Add(sulfur);
        }
    }
}
