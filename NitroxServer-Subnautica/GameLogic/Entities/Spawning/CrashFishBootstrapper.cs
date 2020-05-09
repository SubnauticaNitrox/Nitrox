using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel_Subnautica.Helper;
using NitroxServer.GameLogic.Entities.Spawning;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers
{
    public class CrashFishBootstrapper : IEntityBootstrapper
    {
        public void Prepare(Entity parentEntity, DeterministicBatchGenerator deterministicBatchGenerator)
        {
            Entity crashFish = SpawnChild(parentEntity, deterministicBatchGenerator, TechType.Crash, "7d307502-46b7-4f86-afb0-65fe8867f893");
            crashFish.Transform.LocalRotation = new NitroxQuaternion(-0.7071068f, 0, 0, 0.7071068f);
            parentEntity.ChildEntities.Add(crashFish);
        }

        private Entity SpawnChild(Entity parentEntity, DeterministicBatchGenerator deterministicBatchGenerator, TechType techType, string classId)
        {
            NitroxId id = deterministicBatchGenerator.NextId();

            return new Entity(new NitroxVector3(0, 0, 0), new NitroxQuaternion(0, 0, 0, 1), new NitroxVector3(1, 1, 1), techType.Model(), parentEntity.Level, classId, true, id, null, parentEntity);
        }
    }
}
