using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel_Subnautica.DataStructures;
using NitroxServer.GameLogic.Entities.Spawning;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers
{
    public class CrashFishBootstrapper : IEntityBootstrapper
    {
        public void Prepare(Entity parentEntity, DeterministicBatchGenerator deterministicBatchGenerator)
        {
            Entity crashFish = SpawnChild(parentEntity, deterministicBatchGenerator, TechType.Crash, "7d307502-46b7-4f86-afb0-65fe8867f893");
            crashFish.Transform.LocalRotation = new NitroxQuaternion(-0.7071068f, 0, 0, 0.7071068f);
            //parentEntity.ChildEntities.Add(crashFish);
        }

        private Entity SpawnChild(Entity parentEntity, DeterministicBatchGenerator deterministicBatchGenerator, TechType techType, string classId)
        {

            NitroxObject crashChild = new NitroxObject(deterministicBatchGenerator.NextId());

            Entity crashChildEntity = new Entity(techType.ToDto(), parentEntity.Level, classId, true, null);

            crashChild.AddBehavior(crashChildEntity);
            crashChild.Transform.SetParent(parentEntity.Transform);

            return crashChildEntity;
        }
    }
}
