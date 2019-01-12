using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Entities.Spawning.EntityBootstrappers
{
    class CrashFishBootstrapper : IEntityBootstrapper
    {
        public void Prepare(Entity parentEntity, DeterministicBatchGenerator deterministicBatchGenerator)
        {
            Entity crashFish = SpawnChild(parentEntity, deterministicBatchGenerator, TechType.Crash);
            parentEntity.ChildEntities.Add(crashFish);

            Entity crashPower = SpawnChild(parentEntity, deterministicBatchGenerator, TechType.CrashPowder);
            parentEntity.ChildEntities.Add(crashPower);
        }

        private Entity SpawnChild(Entity parentEntity, DeterministicBatchGenerator deterministicBatchGenerator, TechType techType)
        {
            string guid = deterministicBatchGenerator.NextGuid();

            return new Entity(parentEntity.Position, parentEntity.Rotation, techType, parentEntity.Level, parentEntity.ClassId, true, guid);
        }
    }
}
