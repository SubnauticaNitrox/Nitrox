using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel_Subnautica.Helper;
using NitroxServer.GameLogic.Entities.EntityBootstrappers;
using NitroxServer.GameLogic.Entities.Spawning;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers
{
    public class CrashFishBootstrapper : IEntityBootstrapper
    {
        public void Prepare(Entity parentEntity, DeterministicBatchGenerator deterministicBatchGenerator)
        {
            Entity crashFish = SpawnChild(parentEntity, deterministicBatchGenerator, TechType.Crash, "7d307502-46b7-4f86-afb0-65fe8867f893");
            parentEntity.ChildEntities.Add(crashFish);

            Entity crashPowder = SpawnChild(parentEntity, deterministicBatchGenerator, TechType.CrashPowder, "1ce074ee-1a58-439b-bb5b-e5e3d9f0886f");
            parentEntity.ChildEntities.Add(crashPowder);
        }

        private Entity SpawnChild(Entity parentEntity, DeterministicBatchGenerator deterministicBatchGenerator, TechType techType, string classId)
        {
            NitroxId id = deterministicBatchGenerator.NextId();

            return new Entity(parentEntity.Position, parentEntity.Rotation, new UnityEngine.Vector3(1, 1, 1), techType.Model(), parentEntity.Level, classId, true, id);
        }
    }
}
