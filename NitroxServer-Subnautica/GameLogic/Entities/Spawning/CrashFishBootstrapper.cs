using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel_Subnautica.DataStructures;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Helper;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers
{
    public class CrashFishBootstrapper : IEntityBootstrapper
    {
        public void Prepare(Entity entity, Entity parentEntity, DeterministicGenerator deterministicBatchGenerator)
        {
            Entity crashFish = SpawnChild(entity, deterministicBatchGenerator, TechType.Crash, "7d307502-46b7-4f86-afb0-65fe8867f893");
            crashFish.Transform.LocalRotation = new NitroxQuaternion(-0.7071068f, 0, 0, 0.7071068f);
            entity.ChildEntities.Add(crashFish);
        }

        private Entity SpawnChild(Entity parentEntity, DeterministicGenerator deterministicBatchGenerator, TechType techType, string classId)
        {
            NitroxId id = deterministicBatchGenerator.NextId();

            return new Entity(new NitroxVector3(0, 0, 0), new NitroxQuaternion(0, 0, 0, 1), new NitroxVector3(1, 1, 1), techType.ToDto(), parentEntity.Level, classId, true, id, null, parentEntity);
        }
    }
}
