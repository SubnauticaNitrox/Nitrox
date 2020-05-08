using System.Collections.Generic;

namespace NitroxServer.Serialization.Resources.Datastructures
{
    public class PrefabPlaceholdersGroupAsset
    {
        // Spawnable prefabs are created dynamically when the game loads 
        // as a child of the PrefabPlaceholderGroup.  We'll need to spawn
        // these to replicate in-game behavior. 
        public List<PrefabAsset> SpawnablePrefabs { get; }

        // Existing prefabs are baked into the gameobject that comes
        // from the resource assets.  This will already be spawned 
        // with the master PrefabPlaceholderGroup prefab.
        public List<PrefabAsset> ExistingPrefabs { get; }

        public PrefabPlaceholdersGroupAsset(List<PrefabAsset> spawnablePrefabs, List<PrefabAsset> existingPrefabs)
        {
            SpawnablePrefabs = spawnablePrefabs;
            ExistingPrefabs = existingPrefabs;
        }
    }
}
