using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel_Subnautica.DataStructures;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Helper;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers;

public class CrashFishBootstrapper : IEntityBootstrapper
{
    public void Prepare(WorldEntity entity, DeterministicGenerator deterministicBatchGenerator)
    {
        // Crashfish are PrefabPlaceholderGroups so should always have a PrefabChildEntity in betweeen the crash and the home.
        PrefabChildEntity placeholder = new(deterministicBatchGenerator.NextId(), null, TechType.None.ToDto(), 0, null, entity.Id);
        entity.ChildEntities.Add(placeholder);

        // TODO: Fix this. it currently is an issue for PlaceholderGroupWorldEntitySpawner's child spawning
        // On client-side, multiple crashhomes are spawned (while there probably should only be a few)
        // and their crash fishes are stuck in their homes
        // Potential fix: Only handle child spawning by clients (the simulating one)
        WorldEntity crashFish = SpawnChild(placeholder, deterministicBatchGenerator, TechType.Crash, "7d307502-46b7-4f86-afb0-65fe8867f893", entity.Level);
        crashFish.Transform.LocalRotation = new NitroxQuaternion(-0.7071068f, 0, 0, 0.7071068f);
        placeholder.ChildEntities.Add(crashFish);
    }

    private WorldEntity SpawnChild(Entity parentEntity, DeterministicGenerator deterministicBatchGenerator, TechType techType, string classId, int level)
    {
        NitroxId id = deterministicBatchGenerator.NextId();

        return new WorldEntity(NitroxVector3.Zero, NitroxQuaternion.Identity, NitroxVector3.One, techType.ToDto(), level, classId, true, id, parentEntity);
    }
}
