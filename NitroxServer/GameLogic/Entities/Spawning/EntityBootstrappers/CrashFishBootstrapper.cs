﻿using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Entities.Spawning.EntityBootstrappers
{
    class CrashFishBootstrapper : IEntityBootstrapper
    {
        public void Prepare(Entity parentEntity, DeterministicBatchGenerator deterministicBatchGenerator)
        {
            Entity crashFish = SpawnChild(parentEntity, deterministicBatchGenerator, TechType.Crash, "7d307502-46b7-4f86-afb0-65fe8867f893");
            parentEntity.ChildEntities.Add(crashFish);

            Entity crashPower = SpawnChild(parentEntity, deterministicBatchGenerator, TechType.CrashPowder, "1ce074ee-1a58-439b-bb5b-e5e3d9f0886f");
            parentEntity.ChildEntities.Add(crashPower);
        }

        private Entity SpawnChild(Entity parentEntity, DeterministicBatchGenerator deterministicBatchGenerator, TechType techType, string classId)
        {
            string guid = deterministicBatchGenerator.NextGuid();

            return new Entity(parentEntity.Position, parentEntity.Rotation, new UnityEngine.Vector3(1, 1, 1), techType, parentEntity.Level, classId, true, guid);
        }
    }
}
